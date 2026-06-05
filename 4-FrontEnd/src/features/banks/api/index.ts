import { apiClient } from '../../../core/api/client';
import { Bank, CreateBankDto, UpdateBankDto } from '../types';

const BASE = '/banks';

export async function getAllBanks(): Promise<Bank[]> {
  const res = await apiClient.get<Bank[]>(BASE);
  return res.data;
}

export async function getBankById(id: string): Promise<Bank> {
  const res = await apiClient.get<Bank>(`${BASE}/${id}`);
  return res.data;
}

export async function createBank(dto: CreateBankDto): Promise<Bank> {
  const res = await apiClient.post<Bank>(BASE, dto);
  return res.data;
}

export async function updateBank(id: string, dto: UpdateBankDto): Promise<Bank> {
  const res = await apiClient.put<Bank>(`${BASE}/${id}`, dto);
  return res.data;
}

export async function deleteBank(id: string): Promise<void> {
  await apiClient.delete(`${BASE}/${id}`);
}
