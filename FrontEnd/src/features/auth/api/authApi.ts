import { api } from '../../../core/api/client'
import type { LoginRequest, LoginResponse } from '../../../shared/types/api'

export const authApi = {
  login: (data: LoginRequest) =>
    api.post<LoginResponse>('/auth/login', data).then((r) => r.data),
}
