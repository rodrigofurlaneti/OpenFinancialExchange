import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getAllSignonSessions,
  getSignonSessionsByImport,
  createSignonSession,
  updateSignonSession,
  deleteSignonSession,
} from '../api';
import { CreateSignonSessionDto, UpdateSignonSessionDto } from '../types';

const QUERY_KEY = ['signon-sessions'];

export function useGetAllSignonSessions() {
  return useQuery({
    queryKey: QUERY_KEY,
    queryFn: getAllSignonSessions,
  });
}

export function useGetSignonSessionsByImport(importId: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-import', importId],
    queryFn: () => getSignonSessionsByImport(importId),
    enabled: !!importId,
  });
}

export function useCreateSignonSession() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateSignonSessionDto) => createSignonSession(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useUpdateSignonSession() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: UpdateSignonSessionDto }) =>
      updateSignonSession(id, dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

export function useDeleteSignonSession() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteSignonSession(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}
