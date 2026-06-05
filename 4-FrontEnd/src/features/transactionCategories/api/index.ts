import { apiClient } from '../../../core/api/client';
import {
  TransactionCategory,
  CreateTransactionCategoryDto,
  UpdateTransactionCategoryDto,
} from '../types';

const BASE = '/transaction-categories';

export async function getAllTransactionCategories(): Promise<TransactionCategory[]> {
  const res = await apiClient.get<TransactionCategory[]>(BASE);
  return res.data;
}

export async function getActiveTransactionCategories(): Promise<TransactionCategory[]> {
  const res = await apiClient.get<TransactionCategory[]>(`${BASE}/active`);
  return res.data;
}

export async function getTransactionCategoryById(id: string): Promise<TransactionCategory> {
  const res = await apiClient.get<TransactionCategory>(`${BASE}/${id}`);
  return res.data;
}

export async function createTransactionCategory(
  dto: CreateTransactionCategoryDto,
): Promise<TransactionCategory> {
  const res = await apiClient.post<TransactionCategory>(BASE, dto);
  return res.data;
}

export async function updateTransactionCategory(
  id: string,
  dto: UpdateTransactionCategoryDto,
): Promise<TransactionCategory> {
  const res = await apiClient.put<TransactionCategory>(`${BASE}/${id}`, dto);
  return res.data;
}

export async function deleteTransactionCategory(id: string): Promise<void> {
  await apiClient.delete(`${BASE}/${id}`);
}
