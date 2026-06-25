import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ofxApi } from '../api/ofxApi'
import type { CreateOfxImportRequest } from '../../../shared/types/api'

export function useOfxImports() {
  return useQuery({ queryKey: ['ofx-imports'], queryFn: ofxApi.getAllImports })
}

export function useCreateOfxImport() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateOfxImportRequest) => ofxApi.createImport(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['ofx-imports'] }),
  })
}

export function useOfxStatements() {
  return useQuery({ queryKey: ['ofx-statements'], queryFn: ofxApi.getAllStatements })
}

export function useTransactionsByStatement(statementId: number | null) {
  return useQuery({
    queryKey: ['ofx-transactions', 'statement', statementId],
    queryFn: () => ofxApi.getByStatement(statementId!),
    enabled: statementId !== null,
  })
}

export function useTransactionsByBankAccount(bankAccountId: number | null) {
  return useQuery({
    queryKey: ['ofx-transactions', 'account', bankAccountId],
    queryFn: () => ofxApi.getByBankAccount(bankAccountId!),
    enabled: bankAccountId !== null,
  })
}

export function useAssignCategory() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ transactionId, categoryId }: { transactionId: number; categoryId: number | null }) =>
      ofxApi.assignCategory(transactionId, categoryId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['ofx-transactions'] }),
  })
}

export function useReprocessAll() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () => ofxApi.reprocessAll(),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['ofx-imports'] })
      qc.invalidateQueries({ queryKey: ['ofx-statements'] })
      qc.invalidateQueries({ queryKey: ['ofx-transactions'] })
      qc.invalidateQueries({ queryKey: ['bank-accounts'] })
      qc.invalidateQueries({ queryKey: ['financial-institutions'] })
    },
  })
}
