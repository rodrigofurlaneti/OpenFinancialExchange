import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { bankAccountsApi } from '../api/bankAccountsApi'
import type { CreateBankAccountRequest, UpdateBankAccountRequest } from '../../../shared/types/api'

const QUERY_KEY = ['bank-accounts']

export function useBankAccounts() {
  return useQuery({ queryKey: QUERY_KEY, queryFn: bankAccountsApi.getAll })
}

export function useCreateBankAccount() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateBankAccountRequest) => bankAccountsApi.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}

export function useUpdateBankAccount() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateBankAccountRequest }) =>
      bankAccountsApi.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}
