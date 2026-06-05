import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getAllTransactions,
  getTransactionsByStatement,
  getTransactionsByCategory,
  getTransactionsByDateRange,
  getUnreconciledTransactions,
  createTransaction,
  updateTransaction,
  deleteTransaction,
  reconcileTransaction,
} from '../api';
import { CreateTransactionDto, UpdateTransactionDto } from '../types';

const QUERY_KEY = ['transactions'];

export function useGetAllTransactions() {
  return useQuery({
    queryKey: QUERY_KEY,
    queryFn: getAllTransactions,
  });
}

export function useGetTransactionsByStatement(statementId: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-statement', statementId],
    queryFn: () => getTransactionsByStatement(statementId),
    enabled: !!statementId,
  });
}

export function useGetTransactionsByCategory(categoryId: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-category', categoryId],
    queryFn: () => getTransactionsByCategory(categoryId),
    enabled: !!categoryId,
  });
}

export function useGetTransactionsByDateRange(from: string, to: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-date-range', from, to],
    queryFn: () => getTransactionsByDateRange(from, to),
    enabled: !!from && !!to,
  });
}

export function useGetUnreconciledTransactions() {
  return useQuery({
    queryKey: [...QUERY_KEY, 'unreconciled'],
    queryFn: getUnreconciledTransactions,
  });
}

export function useCreateTransaction() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateTransactionDto) => createTransaction(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useUpdateTransaction() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: UpdateTransactionDto }) =>
      updateTransaction(id, dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useDeleteTransaction() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteTransaction(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useReconcileTransaction() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => reconcileTransaction(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}
