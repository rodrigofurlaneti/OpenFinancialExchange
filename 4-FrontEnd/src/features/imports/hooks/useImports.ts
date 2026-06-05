import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getAllImports, createImport, updateImport, deleteImport } from '../api';
import { CreateImportDto, UpdateImportDto } from '../types';

const QUERY_KEY = ['imports'];

export function useGetAllImports() {
  return useQuery({
    queryKey: QUERY_KEY,
    queryFn: getAllImports,
  });
}

export function useCreateImport() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateImportDto) => createImport(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useUpdateImport() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: UpdateImportDto }) => updateImport(id, dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useDeleteImport() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteImport(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}
