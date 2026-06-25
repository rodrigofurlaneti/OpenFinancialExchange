import { api } from '../../../core/api/client'
import type {
  CategoryResponse,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from '../../../shared/types/api'

const BASE = '/categories'

export const categoriesApi = {
  getAll: () => api.get<CategoryResponse[]>(BASE).then((r) => r.data),
  create: (data: CreateCategoryRequest) => api.post<number>(BASE, data).then((r) => r.data),
  update: (id: number, data: UpdateCategoryRequest) => api.put(`${BASE}/${id}`, data),
  remove: (id: number) => api.delete(`${BASE}/${id}`),
}
