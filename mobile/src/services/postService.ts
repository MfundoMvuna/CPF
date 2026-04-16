import { api } from './api';
import type { CreatePostRequest, Post } from '../types';

export async function getPosts(page = 1, pageSize = 20): Promise<Post[]> {
  return api.get<Post[]>(`/api/posts?page=${page}&pageSize=${pageSize}`);
}

export async function createPost(data: CreatePostRequest): Promise<Post> {
  return api.post<Post>('/api/posts', data);
}
