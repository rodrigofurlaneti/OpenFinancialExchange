import { useForm } from 'react-hook-form'
import { useLogin } from './hooks/useLogin'
import { extractErrorMessage } from '../../core/api/client'
import { BarChart3 } from 'lucide-react'
import type { LoginRequest } from '../../shared/types/api'

export function LoginPage() {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginRequest>()

  const { mutate, isPending, error } = useLogin()

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-gradient-to-br from-emerald-950/30 via-slate-950 to-slate-950 pointer-events-none" />

      <div className="relative w-full max-w-sm">
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-emerald-600/20 border border-emerald-600/30 mb-4">
            <BarChart3 size={28} className="text-emerald-400" />
          </div>
          <h1 className="text-2xl font-bold text-slate-100">OpenFinancialExchange</h1>
          <p className="text-sm text-slate-500 mt-1">Gestão de extratos OFX</p>
        </div>

        <div className="glass-card p-8">
          <h2 className="text-lg font-semibold text-slate-200 mb-6">Entrar na plataforma</h2>

          {error && (
            <div className="mb-4 px-4 py-3 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
              {extractErrorMessage(error)}
            </div>
          )}

          <form onSubmit={handleSubmit((d: LoginRequest) => mutate(d))} noValidate className="space-y-4">
            <div>
              <label htmlFor="email" className="form-label">E-mail</label>
              <input
                id="email"
                type="email"
                autoComplete="email"
                placeholder="seu@email.com"
                className="form-input"
                {...register('email', {
                  required: 'E-mail é obrigatório',
                  pattern: { value: /^\S+@\S+\.\S+$/, message: 'E-mail inválido' },
                })}
              />
              {errors.email && <p className="form-error">{errors.email.message}</p>}
            </div>

            <div>
              <label htmlFor="password" className="form-label">Senha</label>
              <input
                id="password"
                type="password"
                autoComplete="current-password"
                placeholder="••••••••"
                className="form-input"
                {...register('password', { required: 'Senha é obrigatória' })}
              />
              {errors.password && <p className="form-error">{errors.password.message}</p>}
            </div>

            <button
              type="submit"
              disabled={isPending}
              className="btn-primary w-full mt-2"
            >
              {isPending ? 'Entrando…' : 'Entrar'}
            </button>
          </form>
        </div>
      </div>
    </div>
  )
}
