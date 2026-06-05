import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getAllBanks, createBank, updateBank, deleteBank } from '../api';
import { CreateBankDto, UpdateBankDto } from '../types';

const QUERY_KEY = ['banks'];

export function useGetAllBanks() {
  return useQuery({
    queryKey: QUERY_KEY,
    queryFn: getAllBanks,
  });
}

export function useCreateBank() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateBankDto) => createBank(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useUpdateBank() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: UpdateBankDto }) => updateBank(id, dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useDeleteBank() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteBank(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}
