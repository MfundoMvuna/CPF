import { api, storeTokens, clearTokens } from './api';
import type { AuthTokens, RegisterRequest, LoginRequest } from '../types';

export async function register(data: RegisterRequest): Promise<AuthTokens> {
  const tokens = await api.post<AuthTokens>('/api/auth/register', data, false);
  await storeTokens(tokens);
  return tokens;
}

export async function login(data: LoginRequest): Promise<AuthTokens> {
  const tokens = await api.post<AuthTokens>('/api/auth/login', data, false);
  await storeTokens(tokens);
  return tokens;
}

export async function logout(): Promise<void> {
  await clearTokens();
}
