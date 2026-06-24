import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { institutionsApi } from '../api/institutionsApi'
import type {
  CreateFinancialInstitutionRequest,
  UpdateFinancialInstitutionRequest,
} from '../../../shared/types/api'

const QUERY_KEY = ['financial-institutions']

export function useInstitutions() {
  return useQuery({ queryKey: QUERY_KEY, queryFn: institutionsApi.getAll })
}

export function useCreateInstitution() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateFinancialInstitutionRequest) => institutionsApi.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}

export function useUpdateInstitution() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateFinancialInstitutionRequest }) =>
      institutionsApi.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}
