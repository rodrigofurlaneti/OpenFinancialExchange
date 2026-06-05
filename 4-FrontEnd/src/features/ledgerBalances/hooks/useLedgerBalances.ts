import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getAllLedgerBalances,
  getLedgerBalancesByStatement,
  createLedgerBalance,
  updateLedgerBalance,
  deleteLedgerBalance,
} from '../api';
import { CreateLedgerBalanceDto, UpdateLedgerBalanceDto } from '../types';

const QUERY_KEY = ['ledger-balances'];

export function useGetAllLedgerBalances() {
  return useQuery({
    queryKey: QUERY_KEY,
    queryFn: getAllLedgerBalances,
  });
}

export function useGetLedgerBalancesByStatement(statementId: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-statement', statementId],
    queryFn: () => getLedgerBalancesByStatement(statementId),
    enabled: !!statementId,
  });
}

export function useCreateLedgerBalance() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateLedgerBalanceDto) => createLedgerBalance(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useUpdateLedgerBalance() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: UpdateLedgerBalanceDto }) =>
      updateLedgerBalance(id, dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useDeleteLedgerBalance() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteLedgerBalance(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}
