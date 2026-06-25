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
  isInternal: boolean
  keywords: string
}

const KIND_OPTIONS: { value: CategoryKind; label: string }[] = [
  { value: 'DEBIT', label: 'Despesa (débito)' },
  { value: 'CREDIT', label: 'Receita (crédito)' },
  { value: 'BOTH', label: 'Ambos' },
]

export function CategoryModal({ editing, onClose }: CategoryModalProps) {
  const isEdit = editing !== null
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormValues>({
    defaultValues: { name: '', kind: 'DEBIT', color: '#10b981', isInternal: false, keywords: '' },
  })

  const createMutation = useCreateCategory()
  const updateMutation = useUpdateCategory()
  const isPending = createMutation.isPending || updateMutation.isPending
  const mutationError = createMutation.error ?? updateMutation.error

  useEffect(() => {
    if (editing) {
      reset({
        name: editing.name,
        kind: editing.kind,
        color: editing.color,
        isInternal: editing.isInternal,
        keywords: editing.keywords,
      })
    } else {
      reset({ name: '', kind: 'DEBIT', color: '#10b981', isInternal: false, keywords: '' })
    }
  }, [editing, reset])

  function onSubmit(values: FormValues) {
    const payload = {
      name: values.name.trim(),
      kind: values.kind,
      color: values.color,
      isInternal: values.isInternal,
      keywords: values.keywords,
    }
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
            placeholder="Alimentação"
            {...register('name', {
              required: 'Nome é obrigatório',
              maxLength: { value: 50, message: 'Máx. 50 caracteres' },
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

        <div>
          <label className="form-label">Palavras-chave (auto-categorização)</label>
          <textarea
            className="form-input font-mono text-xs"
            rows={4}
            placeholder={'Uma por linha. Ex.:\nRESTAURANTE\nSUPERMERCADO\nPANIFICADORA'}
            {...register('keywords')}
          />
          <p className="text-xs text-slate-500 mt-1">
            Se a descrição da transação contiver alguma destas palavras, ela recebe esta categoria
            no import/reprocessamento. A maior palavra que casar vence.
          </p>
        </div>

        <label className="flex items-start gap-3 cursor-pointer pt-1">
          <input type="checkbox" className="mt-1 accent-emerald-500" {...register('isInternal')} />
          <span className="text-sm text-slate-300">
            Movimentação interna
            <span className="block text-xs text-slate-500">
              Transferências / aplicação e resgate de investimento. Não conta como receita nem despesa no dashboard.
            </span>
          </span>
        </label>

        <div className="flex justify-end gap-3 pt-2">
          <button type="button" onClick={onClose} className="btn-secondary">Cancelar</button>
          <button type="submit" disabled={isPending} className="btn-primary">
            {isPending ? 'Salvando…' : isEdit ? 'Salvar' : 'Criar'}
          </button>
        </div>
      </form>
    </Modal>
  )
}
