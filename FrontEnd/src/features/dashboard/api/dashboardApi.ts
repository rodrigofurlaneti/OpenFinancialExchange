import { api } from '../../../core/api/client'

export interface TypeSummaryItem {
  trnType: string
  total: number
  count: number
}

export interface CategorySummaryItem {
  categoryId: number | null
  categoryName: string
  color: string | null
  isInternal: boolean
  credit: number
  debit: number
  count: number
}

export interface FinancialSummaryResponse {
  totalCredits: number
  totalDebits: number
  netBalance: number
  internalCredits: number
  internalDebits: number
  transactionCount: number
  byType: TypeSummaryItem[]
  byCategory: CategorySummaryItem[]
  from: string
  to: string
}

export const dashboardApi = {
  getSummary: (from: string, to: string) =>
    api
      .get<FinancialSummaryResponse>('/dashboard/summary', { params: { from, to } })
      .then((r) => r.data),
}
