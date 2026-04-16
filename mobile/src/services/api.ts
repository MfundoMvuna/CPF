import * as SecureStore from 'expo-secure-store';
import { Config } from '../config';
import type { AuthTokens } from '../types';

const TOKEN_KEY = 'cpf_access_token';
const REFRESH_KEY = 'cpf_refresh_token';

async function getStoredTokens(): Promise<AuthTokens | null> {
  const accessToken = await SecureStore.getItemAsync(TOKEN_KEY);
  const refreshToken = await SecureStore.getItemAsync(REFRESH_KEY);
  if (accessToken && refreshToken) return { accessToken, refreshToken };
  return null;
}

async function storeTokens(tokens: AuthTokens): Promise<void> {
  await SecureStore.setItemAsync(TOKEN_KEY, tokens.accessToken);
  await SecureStore.setItemAsync(REFRESH_KEY, tokens.refreshToken);
}

async function clearTokens(): Promise<void> {
  await SecureStore.deleteItemAsync(TOKEN_KEY);
  await SecureStore.deleteItemAsync(REFRESH_KEY);
}

async function refreshAccessToken(): Promise<string | null> {
  const tokens = await getStoredTokens();
  if (!tokens) return null;

  try {
    const res = await fetch(`${Config.API_BASE_URL}/api/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        accessToken: tokens.accessToken,
        refreshToken: tokens.refreshToken,
      }),
    });

    if (!res.ok) {
      await clearTokens();
      return null;
    }

    const newTokens: AuthTokens = await res.json();
    await storeTokens(newTokens);
    return newTokens.accessToken;
  } catch {
    return null;
  }
}

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE';

async function request<T>(
  path: string,
  method: HttpMethod = 'GET',
  body?: unknown,
  requireAuth = true,
): Promise<T> {
  const url = `${Config.API_BASE_URL}${path}`;
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  if (requireAuth) {
    let token = await SecureStore.getItemAsync(TOKEN_KEY);
    if (!token) throw new Error('Not authenticated');
    headers['Authorization'] = `Bearer ${token}`;
  }

  let res = await fetch(url, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  });

  // If 401, try refreshing the token once
  if (res.status === 401 && requireAuth) {
    const newToken = await refreshAccessToken();
    if (newToken) {
      headers['Authorization'] = `Bearer ${newToken}`;
      res = await fetch(url, {
        method,
        headers,
        body: body ? JSON.stringify(body) : undefined,
      });
    }
  }

  if (!res.ok) {
    const errorBody = await res.text();
    let message = `Request failed (${res.status})`;
    try {
      const parsed = JSON.parse(errorBody);
      if (parsed.error) message = parsed.error;
    } catch {
      // ignore parse error
    }
    throw new Error(message);
  }

  const text = await res.text();
  return text ? JSON.parse(text) : ({} as T);
}

export const api = {
  get: <T>(path: string, requireAuth = true) =>
    request<T>(path, 'GET', undefined, requireAuth),
  post: <T>(path: string, body?: unknown, requireAuth = true) =>
    request<T>(path, 'POST', body, requireAuth),
};

export { storeTokens, clearTokens, getStoredTokens };
