import { apiClient } from '../../../core/api/client';
import { LedgerBalance, CreateLedgerBalanceDto, UpdateLedgerBalanceDto } from '../types';

const BASE = '/ledger-balances';

export async function getAllLedgerBalances(): Promise<LedgerBalance[]> {
  const res = await apiClient.get<LedgerBalance[]>(BASE);
  return res.data;
}

export async function getLedgerBalanceById(id: string): Promise<LedgerBalance> {
  const res = await apiClient.get<LedgerBalance>(`${BASE}/${id}`);
  return res.data;
}

export async function getLedgerBalancesByStatement(statementId: string): Promise<LedgerBalance[]> {
  const res = await apiClient.get<LedgerBalance[]>(`${BASE}/by-statement/${statementId}`);
  return res.data;
}

export async function createLedgerBalance(dto: CreateLedgerBalanceDto): Promise<LedgerBalance> {
  const res = await apiClient.post<LedgerBalance>(BASE, dto);
  return res.data;
}

export async function updateLedgerBalance(
  id: string,
  dto: UpdateLedgerBalanceDto,
): Promise<LedgerBalance> {
  const res = await apiClient.put<LedgerBalance>(`${BASE}/${id}`, dto);
  return res.data;
}

export async function deleteLedgerBalance(id: string): Promise<void> {
  await apiClient.delete(`${BASE}/${id}`);
}
