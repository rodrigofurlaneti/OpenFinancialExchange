import { apiClient } from '../../../core/api/client';
import { Transaction, CreateTransactionDto, UpdateTransactionDto } from '../types';

const BASE = '/transactions';

export async function getAllTransactions(): Promise<Transaction[]> {
  const res = await apiClient.get<Transaction[]>(BASE);
  return res.data;
}

export async function getTransactionById(id: string): Promise<Transaction> {
  const res = await apiClient.get<Transaction>(`${BASE}/${id}`);
  return res.data;
}

export async function getTransactionsByStatement(statementId: string): Promise<Transaction[]> {
  const res = await apiClient.get<Transaction[]>(`${BASE}/by-statement/${statementId}`);
  return res.data;
}

export async function getTransactionsByCategory(categoryId: string): Promise<Transaction[]> {
  const res = await apiClient.get<Transaction[]>(`${BASE}/by-category/${categoryId}`);
  return res.data;
}

export async function getTransactionsByDateRange(
  from: string,
  to: string,
): Promise<Transaction[]> {
  const res = await apiClient.get<Transaction[]>(`${BASE}/by-date-range`, {
    params: { from, to },
  });
  return res.data;
}

export async function getUnreconciledTransactions(): Promise<Transaction[]> {
  const res = await apiClient.get<Transaction[]>(`${BASE}/unreconciled`);
  return res.data;
}

export async function createTransaction(dto: CreateTransactionDto): Promise<Transaction> {
  const res = await apiClient.post<Transaction>(BASE, dto);
  return res.data;
}

export async function updateTransaction(
  id: string,
  dto: UpdateTransactionDto,
): Promise<Transaction> {
  const res = await apiClient.put<Transaction>(`${BASE}/${id}`, dto);
  return res.data;
}

export async function deleteTransaction(id: string): Promise<void> {
  await apiClient.delete(`${BASE}/${id}`);
}

export async function reconcileTransaction(id: string): Promise<Transaction> {
  const res = await apiClient.post<Transaction>(`${BASE}/${id}/reconcile`);
  return res.data;
}
