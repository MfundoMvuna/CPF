// Type definitions matching the backend API contracts

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
}

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  phoneNumber: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}

export interface TriggerPanicRequest {
  latitude: number;
  longitude: number;
  description?: string;
}

export interface PanicAlert {
  id: string;
  userId: string;
  userFullName: string;
  latitude: number;
  longitude: number;
  description?: string;
  status: string;
  triggeredAtUtc: string;
}

export interface CreatePostRequest {
  title: string;
  content: string;
  imageUrl?: string | null;
}

export interface Post {
  id: string;
  userId: string;
  userFullName: string;
  title: string;
  content: string;
  imageUrl?: string | null;
  createdAtUtc: string;
}

export interface ApiError {
  error: string;
}
