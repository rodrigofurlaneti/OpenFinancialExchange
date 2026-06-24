import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { authApi } from '../api/authApi'
import { useAuthStore } from '../../../core/auth/authStore'
import { extractErrorMessage } from '../../../core/api/client'

export function useLogin() {
  const login = useAuthStore((s) => s.login)
  const navigate = useNavigate()

  return useMutation({
    mutationFn: authApi.login,
    onSuccess: ({ token }) => {
      login(token)
      navigate('/institutions')
    },
    onError: (err) => extractErrorMessage(err),
  })
}
