import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { Modal } from '../../../shared/components/Modal'
import { useCreateBankAccount, useUpdateBankAccount } from '../hooks/useBankAccounts'
import { useInstitutions } from '../../financial-institutions/hooks/useInstitutions'
import { extractErrorMessage } from '../../../core/api/client'
import type { BankAccountResponse, AcctType } from '../../../shared/types/api'

const ACCT_TYPES: AcctType[] = ['CHECKING', 'SAVINGS', 'MONEYMRKT', 'CREDITLINE', 'CD', 'OTHER']
const ACCT_LABELS: Record<AcctType, string> = {
  CHECKING: 'Conta Corrente', SAVINGS: 'Poupança', MONEYMRKT: 'Money Market',
  CREDITLINE: 'Crédito', CD: 'CDB', OTHER: 'Outro',
}

interface BankAccountModalProps {
  editing: BankAccountResponse | null
  onClose: () => void
}

interface FormValues {
  financialInstitutionId: string
  bankId: string
  branchId: string
  acctId: string
  acctType: AcctType
}

export function BankAccountModal({ editing, onClose }: BankAccountModalProps) {
  const isEdit = editing !== null
  const { data: institutions } = useInstitutions()
  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormValues>()
  const createMutation = useCreateBankAccount()
  const updateMutation = useUpdateBankAccount()
  const isPending = createMutation.isPending || updateMutation.isPending
  const mutationError = createMutation.error ?? updateMutation.error

  useEffect(() => {
    if (editing) {
      reset({
        financialInstitutionId: String(editing.financialInstitutionId),
        bankId: editing.bankId,
        branchId: editing.branchId ?? '',
        acctId: editing.acctId,
        acctType: editing.acctType,
      })
    } else {
      reset({ financialInstitutionId: '', bankId: '', branchId: '', acctId: '', acctType: 'CHECKING' })
    }
  }, [editing, reset])

  function onSubmit(values: FormValues) {
    if (isEdit) {
      updateMutation.mutate(
        { id: editing.id, data: { acctType: values.acctType } },
        { onSuccess: onClose }
      )
    } else {
      createMutation.mutate(
        {
          financialInstitutionId: Number(values.financialInstitutionId),
          bankId: values.bankId.trim(),
          branchId: values.branchId.trim() || null,
          acctId: values.acctId.trim(),
          acctType: values.acctType,
        },
        { onSuccess: onClose }
      )
    }
  }

  return (
    <Modal title={isEdit ? 'Editar Conta' : 'Nova Conta Bancária'} onClose={onClose}>
      {mutationError && (
        <div className="mb-4 px-4 py-3 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
          {extractErrorMessage(mutationError)}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
        <div>
          <label className="form-label">Instituição Financeira *</label>
          <select
            className="form-input"
            disabled={isEdit}
            {...register('financialInstitutionId', { required: 'Selecione uma instituição' })}
          >
            <option value="">Selecione…</option>
            {institutions?.map((i) => (
              <option key={i.id} value={i.id}>
                {i.bankId}{i.orgName ? ` — ${i.orgName}` : ''}
              </option>
            ))}
          </select>
          {errors.financialInstitutionId && <p className="form-error">{errors.financialInstitutionId.message}</p>}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="form-label">Bank ID *</label>
            <input
              className="form-input"
              placeholder="237"
              disabled={isEdit}
              {...register('bankId', { required: 'Obrigatório' })}
            />
            {errors.bankId && <p className="form-error">{errors.bankId.message}</p>}
          </div>
          <div>
            <label className="form-label">Agência (Branch ID)</label>
            <input
              className="form-input"
              placeholder="Opcional"
              disabled={isEdit}
              {...register('branchId')}
            />
          </div>
        </div>

        <div>
          <label className="form-label">Número da Conta *</label>
          <input
            className="form-input"
            placeholder="000123456-7"
            disabled={isEdit}
            {...register('acctId', { required: 'Obrigatório' })}
          />
          {errors.acctId && <p className="form-error">{errors.acctId.message}</p>}
        </div>

        <div>
          <label className="form-label">Tipo de Conta *</label>
          <select className="form-input" {...register('acctType', { required: 'Obrigatório' })}>
            {ACCT_TYPES.map((t) => (
              <option key={t} value={t}>{ACCT_LABELS[t]}</option>
            ))}
          </select>
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
