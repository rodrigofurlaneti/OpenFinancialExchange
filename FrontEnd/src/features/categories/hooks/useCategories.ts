import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { categoriesApi } from '../api/categoriesApi'
import type { CreateCategoryRequest, UpdateCategoryRequest } from '../../../shared/types/api'

const QUERY_KEY = ['categories']

export function useCategories() {
  return useQuery({ queryKey: QUERY_KEY, queryFn: categoriesApi.getAll })
}

export function useCreateCategory() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateCategoryRequest) => categoriesApi.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}

export function useUpdateCategory() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateCategoryRequest }) =>
      categoriesApi.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}

export function useDeleteCategory() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => categoriesApi.remove(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}
