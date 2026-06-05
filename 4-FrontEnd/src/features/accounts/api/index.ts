import { apiClient } from '../../../core/api/client';
import { Account, CreateAccountDto, UpdateAccountDto } from '../types';

const BASE = '/accounts';

export async function getAllAccounts(): Promise<Account[]> {
  const res = await apiClient.get<Account[]>(BASE);
  return res.data;
}

export async function getAccountById(id: string): Promise<Account> {
  const res = await apiClient.get<Account>(`${BASE}/${id}`);
  return res.data;
}

export async function getAccountsByImport(importId: string): Promise<Account[]> {
  const res = await apiClient.get<Account[]>(`${BASE}/by-import/${importId}`);
  return res.data;
}

export async function getAccountsByBank(bankId: string): Promise<Account[]> {
  const res = await apiClient.get<Account[]>(`${BASE}/by-bank/${bankId}`);
  return res.data;
}

export async function createAccount(dto: CreateAccountDto): Promise<Account> {
  const res = await apiClient.post<Account>(BASE, dto);
  return res.data;
}

export async function updateAccount(id: string, dto: UpdateAccountDto): Promise<Account> {
  const res = await apiClient.put<Account>(`${BASE}/${id}`, dto);
  return res.data;
}

export async function deleteAccount(id: string): Promise<void> {
  await apiClient.delete(`${BASE}/${id}`);
}
