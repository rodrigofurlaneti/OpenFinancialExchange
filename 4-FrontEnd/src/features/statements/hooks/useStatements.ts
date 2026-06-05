import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getAllStatements,
  getStatementsByAccount,
  createStatement,
  updateStatement,
  deleteStatement,
} from '../api';
import { CreateStatementDto, UpdateStatementDto } from '../types';

const QUERY_KEY = ['statements'];

export function useGetAllStatements() {
  return useQuery({
    queryKey: QUERY_KEY,
    queryFn: getAllStatements,
  });
}

export function useGetStatementsByAccount(accountId: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-account', accountId],
    queryFn: () => getStatementsByAccount(accountId),
    enabled: !!accountId,
  });
}

export function useCreateStatement() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateStatementDto) => createStatement(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useUpdateStatement() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: UpdateStatementDto }) =>
      updateStatement(id, dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useDeleteStatement() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteStatement(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}
