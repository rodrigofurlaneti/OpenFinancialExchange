import { Badge } from '../../../shared/components/Badge'
import type { OfxTransactionResponse } from '../../../shared/types/api'

interface TransactionTableProps {
  transactions: OfxTransactionResponse[]
  isLoading: boolean
}

function trnTypeBadge(type: string) {
  const credits = ['CREDIT', 'INT', 'DIV', 'DEP', 'DIRECTDEP']
  if (credits.includes(type)) return 'green'
  if (['DEBIT', 'ATM', 'FEE', 'SRVCHG'].includes(type)) return 'red'
  return 'slate'
}

export function TransactionTable({ transactions, isLoading }: TransactionTableProps) {
  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="w-8 h-8 border-2 border-emerald-500 border-t-transparent rounded-full animate-spin" />
      </div>
    )
  }

  if (transactions.length === 0) {
    return (
      <p className="text-center text-slate-500 text-sm py-12">
        Nenhuma transação encontrada para o extrato selecionado.
      </p>
    )
  }

  const total = transactions.reduce((s, t) => s + t.trnAmt, 0)

  return (
    <div>
      {/* Summary bar */}
      <div className="flex items-center justify-between px-4 py-3 bg-slate-800/40 rounded-t-xl border-b border-slate-700/50">
        <span className="text-sm text-slate-400">
          <span className="font-semibold text-slate-200">{transactions.length}</span> transações
        </span>
        <span className={`text-sm font-semibold ${total >= 0 ? 'text-emerald-400' : 'text-red-400'}`}>
          Total: {new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(total)}
        </span>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full">
          <thead className="bg-slate-800/60">
            <tr>
              <th className="table-header">Data</th>
              <th className="table-header">Tipo</th>
              <th className="table-header">Valor</th>
              <th className="table-header">Descrição</th>
              <th className="table-header hidden lg:table-cell">FitId</th>
            </tr>
          </thead>
          <tbody>
            {transactions.map((t) => {
              const isCredit = t.trnAmt >= 0
              return (
                <tr key={t.id} className="table-row">
                  <td className="table-cell text-slate-400 text-xs whitespace-nowrap">
                    {new Date(t.dtPosted).toLocaleDateString('pt-BR')}
                  </td>
                  <td className="table-cell">
                    <Badge variant={trnTypeBadge(t.trnType)}>{t.trnType}</Badge>
                  </td>
                  <td className={`table-cell font-mono font-semibold ${isCredit ? 'text-emerald-400' : 'text-red-400'}`}>
                    {new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(t.trnAmt)}
                  </td>
                  <td className="table-cell max-w-xs">
                    <div className="truncate font-medium">{t.name ?? '—'}</div>
                    {t.memo && <div className="truncate text-xs text-slate-500">{t.memo}</div>}
                  </td>
                  <td className="table-cell hidden lg:table-cell font-mono text-xs text-slate-600">
                    {t.fitId ?? '—'}
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    </div>
  )
}
