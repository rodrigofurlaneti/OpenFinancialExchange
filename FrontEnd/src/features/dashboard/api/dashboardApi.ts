import { api } from '../../../core/api/client'

export interface TypeSummaryItem {
  trnType: string
  total: number
  count: number
}

export interface FinancialSummaryResponse {
  totalCredits: number
  totalDebits: number
  netBalance: number
  transactionCount: number
  byType: TypeSummaryItem[]
  from: string
  to: string
}

export const dashboardApi = {
  getSummary: (from: string, to: string) =>
    api
      .get<FinancialSummaryResponse>('/dashboard/summary', { params: { from, to } })
      .then((r) => r.data),
}
