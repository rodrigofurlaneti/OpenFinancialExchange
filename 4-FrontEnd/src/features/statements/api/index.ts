import { apiClient } from '../../../core/api/client';
import { Statement, CreateStatementDto, UpdateStatementDto } from '../types';

const BASE = '/statements';

export async function getAllStatements(): Promise<Statement[]> {
  const res = await apiClient.get<Statement[]>(BASE);
  return res.data;
}

export async function getStatementById(id: string): Promise<Statement> {
  const res = await apiClient.get<Statement>(`${BASE}/${id}`);
  return res.data;
}

export async function getStatementsByAccount(accountId: string): Promise<Statement[]> {
  const res = await apiClient.get<Statement[]>(`${BASE}/by-account/${accountId}`);
  return res.data;
}

export async function createStatement(dto: CreateStatementDto): Promise<Statement> {
  const res = await apiClient.post<Statement>(BASE, dto);
  return res.data;
}

export async function updateStatement(id: string, dto: UpdateStatementDto): Promise<Statement> {
  const res = await apiClient.put<Statement>(`${BASE}/${id}`, dto);
  return res.data;
}

export async function deleteStatement(id: string): Promise<void> {
  await apiClient.delete(`${BASE}/${id}`);
}
