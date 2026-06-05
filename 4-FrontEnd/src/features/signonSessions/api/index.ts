import { apiClient } from '../../../core/api/client';
import { SignonSession, CreateSignonSessionDto, UpdateSignonSessionDto } from '../types';

const BASE = '/signon-sessions';

export async function getAllSignonSessions(): Promise<SignonSession[]> {
  const res = await apiClient.get<SignonSession[]>(BASE);
  return res.data;
}

export async function getSignonSessionById(id: string): Promise<SignonSession> {
  const res = await apiClient.get<SignonSession>(`${BASE}/${id}`);
  return res.data;
}

export async function getSignonSessionsByImport(importId: string): Promise<SignonSession[]> {
  const res = await apiClient.get<SignonSession[]>(`${BASE}/by-import/${importId}`);
  return res.data;
}

export async function createSignonSession(dto: CreateSignonSessionDto): Promise<SignonSession> {
  const res = await apiClient.post<SignonSession>(BASE, dto);
  return res.data;
}

export async function updateSignonSession(
  id: string,
  dto: UpdateSignonSessionDto,
): Promise<SignonSession> {
  const res = await apiClient.put<SignonSession>(`${BASE}/${id}`, dto);
  return res.data;
}

export async function deleteSignonSession(id: string): Promise<void> {
  await apiClient.delete(`${BASE}/${id}`);
}
