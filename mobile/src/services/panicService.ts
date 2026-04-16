import { api } from './api';
import type { TriggerPanicRequest, PanicAlert } from '../types';

export async function triggerPanic(data: TriggerPanicRequest): Promise<{ alertId: string }> {
  return api.post<{ alertId: string }>('/api/panic/trigger', data);
}

export async function getActiveAlerts(): Promise<PanicAlert[]> {
  return api.get<PanicAlert[]>('/api/panic/active');
}
