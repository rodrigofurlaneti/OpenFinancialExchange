import { api } from '../../../core/api/client'
import type {
  FinancialInstitutionResponse,
  CreateFinancialInstitutionRequest,
  UpdateFinancialInstitutionRequest,
} from '../../../shared/types/api'

const BASE = '/financialinstitutions'

export const institutionsApi = {
  getAll: () => api.get<FinancialInstitutionResponse[]>(BASE).then((r) => r.data),
  getById: (id: number) => api.get<FinancialInstitutionResponse>(`${BASE}/${id}`).then((r) => r.data),
  create: (data: CreateFinancialInstitutionRequest) =>
    api.post<number>(BASE, data).then((r) => r.data),
  update: (id: number, data: UpdateFinancialInstitutionRequest) =>
    api.put(`${BASE}/${id}`, data),
}
