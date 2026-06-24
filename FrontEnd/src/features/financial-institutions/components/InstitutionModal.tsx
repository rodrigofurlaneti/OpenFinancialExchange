import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { Modal } from '../../../shared/components/Modal'
import { useCreateInstitution, useUpdateInstitution } from '../hooks/useInstitutions'
import { extractErrorMessage } from '../../../core/api/client'
import type {
  FinancialInstitutionResponse,
  CreateFinancialInstitutionRequest,
} from '../../../shared/types/api'

interface InstitutionModalProps {
  editing: FinancialInstitutionResponse | null
  onClose: () => void
}

interface FormValues {
  bankId: string
  orgName: string
  fid: string
}

export function InstitutionModal({ editing, onClose }: InstitutionModalProps) {
  const isEdit = editing !== null
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormValues>()

  const createMutation = useCreateInstitution()
  const updateMutation = useUpdateInstitution()
  const isPending = createMutation.isPending || updateMutation.isPending
  const mutationError = createMutation.error ?? updateMutation.error

  useEffect(() => {
    if (editing) {
      reset({ bankId: editing.bankId, orgName: editing.orgName ?? '', fid: editing.fid ?? '' })
    } else {
      reset({ bankId: '', orgName: '', fid: '' })
    }
  }, [editing, reset])

  function onSubmit(values: FormValues) {
    const payload: CreateFinancialInstitutionRequest = {
      bankId: values.bankId.trim(),
      orgName: values.orgName.trim() || null,
      fid: values.fid.trim() || null,
    }
    if (isEdit) {
      updateMutation.mutate(
        { id: editing.id, data: { orgName: payload.orgName, fid: payload.fid } },
        { onSuccess: onClose }
      )
    } else {
      createMutation.mutate(payload, { onSuccess: onClose })
    }
  }

  return (
    <Modal title={isEdit ? 'Editar Instituição' : 'Nova Instituição'} onClose={onClose}>
      {mutationError && (
        <div className="mb-4 px-4 py-3 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
          {extractErrorMessage(mutationError)}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
        <div>
          <label className="form-label">Bank ID *</label>
          <input
            className="form-input"
            placeholder="237"
            disabled={isEdit}
            {...register('bankId', { required: 'Bank ID é obrigatório', maxLength: { value: 20, message: 'Máx. 20 caracteres' } })}
          />
          {errors.bankId && <p className="form-error">{errors.bankId.message}</p>}
        </div>

        <div>
          <label className="form-label">Org Name</label>
          <input
            className="form-input"
            placeholder="Banco do Brasil (opcional)"
            {...register('orgName', { maxLength: { value: 100, message: 'Máx. 100 caracteres' } })}
          />
          {errors.orgName && <p className="form-error">{errors.orgName.message}</p>}
          <p className="text-xs text-slate-500 mt-1">Deixe em branco para Bradesco.</p>
        </div>

        <div>
          <label className="form-label">FID</label>
          <input
            className="form-input"
            placeholder="Opcional"
            {...register('fid', { maxLength: { value: 50, message: 'Máx. 50 caracteres' } })}
          />
          {errors.fid && <p className="form-error">{errors.fid.message}</p>}
        </div>

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
