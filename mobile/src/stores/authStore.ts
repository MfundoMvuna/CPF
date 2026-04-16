import { create } from 'zustand';
import { jwtDecode } from './jwtDecode';
import * as authService from '../services/authService';
import { getStoredTokens } from '../services/api';
import type { User, RegisterRequest, LoginRequest } from '../types';

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  initialize: () => Promise<void>;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  clearError: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  isAuthenticated: false,
  isLoading: true,
  error: null,

  initialize: async () => {
    try {
      const tokens = await getStoredTokens();
      if (tokens) {
        const user = decodeUser(tokens.accessToken);
        set({ user, isAuthenticated: true, isLoading: false });
      } else {
        set({ isLoading: false });
      }
    } catch {
      set({ isLoading: false });
    }
  },

  login: async (data) => {
    set({ isLoading: true, error: null });
    try {
      const tokens = await authService.login(data);
      const user = decodeUser(tokens.accessToken);
      set({ user, isAuthenticated: true, isLoading: false });
    } catch (e: any) {
      set({ error: e.message || 'Login failed', isLoading: false });
    }
  },

  register: async (data) => {
    set({ isLoading: true, error: null });
    try {
      const tokens = await authService.register(data);
      const user = decodeUser(tokens.accessToken);
      set({ user, isAuthenticated: true, isLoading: false });
    } catch (e: any) {
      set({ error: e.message || 'Registration failed', isLoading: false });
    }
  },

  logout: async () => {
    await authService.logout();
    set({ user: null, isAuthenticated: false, error: null });
  },

  clearError: () => set({ error: null }),
}));

function decodeUser(token: string): User {
  const payload = jwtDecode(token);
  return {
    id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
    email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
    fullName: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
    role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
  };
}
