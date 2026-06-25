import { api } from '../../../core/api/client'
import type {
  OfxImportResponse,
  OfxStatementResponse,
  OfxTransactionResponse,
  CreateOfxImportRequest,
} from '../../../shared/types/api'

export const ofxApi = {
  // Imports
  getAllImports: () => api.get<OfxImportResponse[]>('/ofximports').then((r) => r.data),
  createImport: (data: CreateOfxImportRequest) =>
    api.post<number>('/ofximports', data).then((r) => r.data),
  reprocessAll: () =>
    api
      .post<{ importsProcessed: number; transactionsCreated: number }>('/ofximports/reprocess-all')
      .then((r) => r.data),
  reprocessImport: (id: number) =>
    api.post<{ transactionsCreated: number }>(`/ofximports/${id}/reprocess`).then((r) => r.data),

  // Statements
  getAllStatements: () => api.get<OfxStatementResponse[]>('/ofxstatements').then((r) => r.data),
  getStatementById: (id: number) =>
    api.get<OfxStatementResponse>(`/ofxstatements/${id}`).then((r) => r.data),

  // Transactions
  getByStatement: (statementId: number) =>
    api.get<OfxTransactionResponse[]>(`/ofxtransactions/by-statement/${statementId}`).then((r) => r.data),
  getByBankAccount: (bankAccountId: number) =>
    api.get<OfxTransactionResponse[]>(`/ofxtransactions/by-account/${bankAccountId}`).then((r) => r.data),
  assignCategory: (transactionId: number, categoryId: number | null) =>
    api.patch(`/ofxtransactions/${transactionId}/category`, { categoryId }),
}
