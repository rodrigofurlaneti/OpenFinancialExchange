import { api } from '../../../core/api/client'
import type {
  BankAccountResponse,
  CreateBankAccountRequest,
  UpdateBankAccountRequest,
} from '../../../shared/types/api'

const BASE = '/bankaccounts'

export const bankAccountsApi = {
  getAll: () => api.get<BankAccountResponse[]>(BASE).then((r) => r.data),
  getById: (id: number) => api.get<BankAccountResponse>(`${BASE}/${id}`).then((r) => r.data),
  create: (data: CreateBankAccountRequest) => api.post<number>(BASE, data).then((r) => r.data),
  update: (id: number, data: UpdateBankAccountRequest) => api.put(`${BASE}/${id}`, data),
}
