import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getAllAccounts,
  getAccountsByImport,
  getAccountsByBank,
  createAccount,
  updateAccount,
  deleteAccount,
} from '../api';
import { CreateAccountDto, UpdateAccountDto } from '../types';

const QUERY_KEY = ['accounts'];

export function useGetAllAccounts() {
  return useQuery({
    queryKey: QUERY_KEY,
    queryFn: getAllAccounts,
  });
}

export function useGetAccountsByImport(importId: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-import', importId],
    queryFn: () => getAccountsByImport(importId),
    enabled: !!importId,
  });
}

export function useGetAccountsByBank(bankId: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-bank', bankId],
    queryFn: () => getAccountsByBank(bankId),
    enabled: !!bankId,
  });
}

export function useCreateAccount() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateAccountDto) => createAccount(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useUpdateAccount() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: UpdateAccountDto }) => updateAccount(id, dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useDeleteAccount() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteAccount(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}
