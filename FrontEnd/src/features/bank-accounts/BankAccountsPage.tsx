import { useState } from 'react'
import { useBankAccounts } from './hooks/useBankAccounts'
import { useInstitutions } from '../financial-institutions/hooks/useInstitutions'
import { BankAccountModal } from './components/BankAccountModal'
import { Badge } from '../../shared/components/Badge'
import { EmptyState } from '../../shared/components/EmptyState'
import { CreditCard, Plus, Pencil } from 'lucide-react'
import type { BankAccountResponse, AcctType } from '../../shared/types/api'

const ACCT_TYPE_LABELS: Record<AcctType, string> = {
  CHECKING: 'Corrente', SAVINGS: 'Poupança', MONEYMRKT: 'Money Market',
  CREDITLINE: 'Crédito', CD: 'CDB', OTHER: 'Outro',
}

export function BankAccountsPage() {
  const { data: accounts, isLoading, isError } = useBankAccounts()
  const { data: institutions } = useInstitutions()
  const [editing, setEditing] = useState<BankAccountResponse | null | undefined>(undefined)

  const isModalOpen = editing !== undefined
  const instMap = new Map(institutions?.map((i) => [i.id, i]))

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-slate-100">Contas Bancárias</h1>
          <p className="text-sm text-slate-500 mt-0.5">Contas vinculadas às instituições para importação OFX.</p>
        </div>
        <button onClick={() => setEditing(null)} className="btn-primary flex items-center gap-2">
          <Plus size={16} /> Nova Conta
        </button>
      </div>

      <div className="glass-card overflow-hidden">
        {isLoading && (
          <div className="flex items-center justify-center py-16">
            <div className="w-8 h-8 border-2 border-emerald-500 border-t-transparent rounded-full animate-spin" />
          </div>
        )}
        {isError && (
          <div className="flex items-center justify-center py-16 text-red-400 text-sm">
            Erro ao carregar contas.
          </div>
        )}
        {!isLoading && !isError && (!accounts || accounts.length === 0) && (
          <EmptyState
            icon={CreditCard}
            title="Nenhuma conta cadastrada"
            description="Adicione uma conta bancária vinculada a uma instituição."
            action={
              <button onClick={() => setEditing(null)} className="btn-primary flex items-center gap-2 mx-auto">
                <Plus size={16} /> Adicionar conta
              </button>
            }
          />
        )}
        {accounts && accounts.length > 0 && (
          <table className="w-full">
            <thead className="bg-slate-800/80">
              <tr>
                <th className="table-header">Banco</th>
                <th className="table-header">Agência</th>
                <th className="table-header">Conta</th>
                <th className="table-header">Tipo</th>
                <th className="table-header">Status</th>
                <th className="table-header w-16" />
              </tr>
            </thead>
            <tbody>
              {accounts.map((acc) => {
                const inst = instMap.get(acc.financialInstitutionId)
                return (
                  <tr key={acc.id} className="table-row">
                    <td className="table-cell">
                      <div className="font-semibold text-emerald-400 font-mono">{acc.bankId}</div>
                      <div className="text-xs text-slate-500">{inst?.orgName ?? '—'}</div>
                    </td>
                    <td className="table-cell font-mono text-sm">
                      {acc.branchId ?? <span className="text-slate-600">—</span>}
                    </td>
                    <td className="table-cell font-mono">{acc.acctId}</td>
                    <td className="table-cell">
                      <Badge variant="blue">{ACCT_TYPE_LABELS[acc.acctType]}</Badge>
                    </td>
                    <td className="table-cell">
                      <Badge variant={acc.isActive ? 'green' : 'red'}>
                        {acc.isActive ? 'Ativo' : 'Inativo'}
                      </Badge>
                    </td>
                    <td className="table-cell">
                      <button
                        onClick={() => setEditing(acc)}
                        aria-label={`Editar conta ${acc.acctId}`}
                        className="p-1.5 rounded-lg text-slate-500 hover:text-slate-200 hover:bg-slate-700 transition-colors"
                      >
                        <Pencil size={14} />
                      </button>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        )}
      </div>

      {isModalOpen && (
        <BankAccountModal editing={editing ?? null} onClose={() => setEditing(undefined)} />
      )}
    </div>
  )
}
