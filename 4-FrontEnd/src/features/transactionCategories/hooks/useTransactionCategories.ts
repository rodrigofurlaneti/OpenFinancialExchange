import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getAllTransactionCategories,
  getActiveTransactionCategories,
  createTransactionCategory,
  updateTransactionCategory,
  deleteTransactionCategory,
} from '../api';
import { CreateTransactionCategoryDto, UpdateTransactionCategoryDto } from '../types';

const QUERY_KEY = ['transaction-categories'];

export function useGetAllTransactionCategories() {
  return useQuery({
    queryKey: QUERY_KEY,
    queryFn: getAllTransactionCategories,
  });
}

export function useGetActiveTransactionCategories() {
  return useQuery({
    queryKey: [...QUERY_KEY, 'active'],
    queryFn: getActiveTransactionCategories,
  });
}

export function useCreateTransactionCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateTransactionCategoryDto) => createTransactionCategory(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useUpdateTransactionCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: UpdateTransactionCategoryDto }) =>
      updateTransactionCategory(id, dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useDeleteTransactionCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteTransactionCategory(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}
