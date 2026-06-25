import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { Modal } from '../../../shared/components/Modal'
import { useCreateCategory, useUpdateCategory } from '../hooks/useCategories'
import { extractErrorMessage } from '../../../core/api/client'
import type { CategoryResponse, CategoryKind } from '../../../shared/types/api'

interface CategoryModalProps {
  editing: CategoryResponse | null
  onClose: () => void
}

interface FormValues {
  name: string
  kind: CategoryKind
  color: string
}

const KIND_OPTIONS: { value: CategoryKind; label: string }[] = [
  { value: 'DEBIT', label: 'Despesa (debito)' },
  { value: 'CREDIT', label: 'Receita (credito)' },
  { value: 'BOTH', label: 'Ambos' },
]

export function CategoryModal({ editing, onClose }: CategoryModalProps) {
  const isEdit = editing !== null
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormValues>({ defaultValues: { name: '', kind: 'DEBIT', color: '#10b981' } })

  const createMutation = useCreateCategory()
  const updateMutation = useUpdateCategory()
  const isPending = createMutation.isPending || updateMutation.isPending
  const mutationError = createMutation.error ?? updateMutation.error

  useEffect(() => {
    if (editing) {
      reset({ name: editing.name, kind: editing.kind, color: editing.color })
    } else {
      reset({ name: '', kind: 'DEBIT', color: '#10b981' })
    }
  }, [editing, reset])

  function onSubmit(values: FormValues) {
    const payload = { name: values.name.trim(), kind: values.kind, color: values.color }
    if (isEdit) {
      updateMutation.mutate({ id: editing.id, data: payload }, { onSuccess: onClose })
    } else {
      createMutation.mutate(payload, { onSuccess: onClose })
    }
  }

  return (
    <Modal title={isEdit ? 'Editar Categoria' : 'Nova Categoria'} onClose={onClose}>
      {mutationError && (
        <div className="mb-4 px-4 py-3 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
          {extractErrorMessage(mutationError)}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
        <div>
          <label className="form-label">Nome *</label>
          <input
            className="form-input"
            placeholder="Alimentacao"
            {...register('name', {
              required: 'Nome e obrigatorio',
              maxLength: { value: 50, message: 'Max. 50 caracteres' },
            })}
          />
          {errors.name && <p className="form-error">{errors.name.message}</p>}
        </div>

        <div>
          <label className="form-label">Tipo *</label>
          <select className="form-input" {...register('kind', { required: true })}>
            {KIND_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>{o.label}</option>
            ))}
          </select>
        </div>

        <div>
          <label className="form-label">Cor</label>
          <div className="flex items-center gap-3">
            <input
              type="color"
              className="h-10 w-14 rounded-lg bg-transparent border border-slate-700 cursor-pointer"
              {...register('color', { required: true })}
            />
            <span className="text-xs text-slate-500">Escolha uma cor para a categoria.</span>
          </div>
        </div>

        <div className="flex justify-end gap-3 pt-2">
          <button type="button" onClick={onClose} className="btn-secondary">Cancelar</button>
          <button type="submit" disabled={isPending} className="btn-primary">
            {isPending ? 'Salvando...' : isEdit ? 'Salvar' : 'Criar'}
          </button>
        </div>
      </form>
    </Modal>
  )
}
