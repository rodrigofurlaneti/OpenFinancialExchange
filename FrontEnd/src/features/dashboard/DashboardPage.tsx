import { useState } from 'react'
import { TrendingUp, TrendingDown, Scale, Hash, Calendar } from 'lucide-react'
import { useDashboardSummary } from './hooks/useDashboard'

type Period = 'day' | 'week' | 'month' | 'bimonth' | 'quarter' | 'semester' | 'year'

interface PeriodOption {
  key: Period
  label: string
}

const PERIODS: PeriodOption[] = [
  { key: 'day',      label: 'Diário'     },
  { key: 'week',     label: 'Semanal'    },
  { key: 'month',    label: 'Mensal'     },
  { key: 'bimonth',  label: 'Bimestral'  },
  { key: 'quarter',  label: 'Trimestral' },
  { key: 'semester', label: 'Semestral'  },
  { key: 'year',     label: 'Anual'      },
]

function getRange(period: Period): { from: Date; to: Date } {
  const to = new Date()
  const from = new Date()

  switch (period) {
    case 'day':      from.setDate(to.getDate() - 1);         break
    case 'week':     from.setDate(to.getDate() - 7);         break
    case 'month':    from.setMonth(to.getMonth() - 1);       break
    case 'bimonth':  from.setMonth(to.getMonth() - 2);       break
    case 'quarter':  from.setMonth(to.getMonth() - 3);       break
    case 'semester': from.setMonth(to.getMonth() - 6);       break
    case 'year':     from.setFullYear(to.getFullYear() - 1); break
  }

  from.setHours(0, 0, 0, 0)
  to.setHours(23, 59, 59, 999)
  return { from, to }
}

function toIso(d: Date) {
  return d.toISOString()
}

function fmt(value: number) {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(value)
}

function fmtDate(iso: string) {
  return new Date(iso).toLocaleDateString('pt-BR')
}

const TRN_TYPE_LABELS: Record<string, string> = {
  CREDIT: 'Crédito',
  DEBIT:  'Débito',
  INT:    'Juros',
  DIV:    'Dividendo',
  FEE:    'Tarifa',
  SRVCHG: 'Serviço',
  DEP:    'Depósito',
  ATM:    'Saque ATM',
  POS:    'Ponto de Venda',
  XFER:   'Transferência',
  CHECK:  'Cheque',
  PAYMENT:'Pagamento',
  CASH:   'Dinheiro',
  DIRECTDEP: 'Depósito Direto',
  DIRECTDEBIT: 'Débito Direto',
  REPEATPMT:  'Pagamento Recorrente',
  OTHER:  'Outros',
}

function trnLabel(type: string) {
  return TRN_TYPE_LABELS[type] ?? type
}

export function DashboardPage() {
  const [period, setPeriod] = useState<Period>('month')
  const { from, to } = getRange(period)
  const { data, isLoading, isError } = useDashboardSummary(toIso(from), toIso(to))

  const cards = data
    ? [
        {
          label: 'Entradas',
          value: fmt(data.totalCredits),
          icon: TrendingUp,
          color: 'text-emerald-400',
          bg: 'bg-emerald-500/10 border-emerald-500/20',
        },
        {
          label: 'Saídas',
          value: fmt(data.totalDebits),
          icon: TrendingDown,
          color: 'text-red-400',
          bg: 'bg-red-500/10 border-red-500/20',
        },
        {
          label: 'Saldo Líquido',
          value: fmt(data.netBalance),
          icon: Scale,
          color: data.netBalance >= 0 ? 'text-emerald-400' : 'text-red-400',
          bg: data.netBalance >= 0
            ? 'bg-emerald-500/10 border-emerald-500/20'
            : 'bg-red-500/10 border-red-500/20',
        },
        {
          label: 'Transações',
          value: data.transactionCount.toLocaleString('pt-BR'),
          icon: Hash,
          color: 'text-blue-400',
          bg: 'bg-blue-500/10 border-blue-500/20',
        },
      ]
    : []

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-100">Dashboard</h1>
          <p className="text-slate-400 text-sm mt-1">Resumo financeiro das transações OFX</p>
        </div>

        {data && (
          <div className="flex items-center gap-2 text-xs text-slate-500">
            <Calendar size={13} />
            <span>
              {fmtDate(data.from)} — {fmtDate(data.to)}
            </span>
          </div>
        )}
      </div>

      {/* Period selector */}
      <div className="flex flex-wrap gap-2">
        {PERIODS.map(({ key, label }) => (
          <button
            key={key}
            onClick={() => setPeriod(key)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200 ${
              period === key
                ? 'bg-emerald-600 text-white shadow-lg shadow-emerald-900/30'
                : 'bg-slate-800/60 text-slate-400 hover:bg-slate-700/60 hover:text-slate-200 border border-slate-700/50'
            }`}
          >
            {label}
          </button>
        ))}
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {[1, 2, 3, 4].map((i) => (
            <div key={i} className="glass-card rounded-xl p-5 animate-pulse">
              <div className="h-4 bg-slate-700 rounded w-24 mb-3" />
              <div className="h-7 bg-slate-700 rounded w-36" />
            </div>
          ))}
        </div>
      )}

      {/* Error */}
      {isError && (
        <div className="glass-card rounded-xl p-6 border border-red-500/20 bg-red-500/5 text-red-400 text-sm">
          Erro ao carregar dados. Verifique se a API está rodando.
        </div>
      )}

      {/* Cards */}
      {data && (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {cards.map(({ label, value, icon: Icon, color, bg }) => (
              <div key={label} className={`glass-card rounded-xl p-5 border ${bg}`}>
                <div className="flex items-center justify-between mb-3">
                  <span className="text-slate-400 text-sm font-medium">{label}</span>
                  <div className={`p-2 rounded-lg ${bg}`}>
                    <Icon size={16} className={color} />
                  </div>
                </div>
                <p className={`text-2xl font-bold ${color}`}>{value}</p>
              </div>
            ))}
          </div>

          {/* Breakdown by type */}
          {data.byType.length > 0 && (
            <div className="glass-card rounded-xl overflow-hidden">
              <div className="px-6 py-4 border-b border-slate-700/50">
                <h2 className="text-slate-200 font-semibold">Detalhamento por Tipo</h2>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-slate-700/50">
                      <th className="text-left px-6 py-3 text-slate-400 font-medium">Tipo</th>
                      <th className="text-right px-6 py-3 text-slate-400 font-medium">Qtd</th>
                      <th className="text-right px-6 py-3 text-slate-400 font-medium">Total</th>
                      <th className="text-right px-6 py-3 text-slate-400 font-medium">% do Volume</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.byType.map((row) => {
                      const totalVolume = data.totalCredits + data.totalDebits
                      const pct = totalVolume > 0
                        ? ((Math.abs(row.total) / totalVolume) * 100).toFixed(1)
                        : '0.0'
                      const isPositive = row.total >= 0
                      return (
                        <tr
                          key={row.trnType}
                          className="border-b border-slate-700/30 hover:bg-slate-700/20 transition-colors"
                        >
                          <td className="px-6 py-3 text-slate-300 font-medium">
                            {trnLabel(row.trnType)}
                            <span className="ml-2 text-xs text-slate-500 font-mono">{row.trnType}</span>
                          </td>
                          <td className="px-6 py-3 text-right text-slate-400">
                            {row.count.toLocaleString('pt-BR')}
                          </td>
                          <td className={`px-6 py-3 text-right font-semibold ${isPositive ? 'text-emerald-400' : 'text-red-400'}`}>
                            {fmt(row.total)}
                          </td>
                          <td className="px-6 py-3 text-right">
                            <div className="flex items-center justify-end gap-2">
                              <div className="w-24 bg-slate-700 rounded-full h-1.5">
                                <div
                                  className={`h-1.5 rounded-full ${isPositive ? 'bg-emerald-500' : 'bg-red-500'}`}
                                  style={{ width: `${Math.min(Number(pct), 100)}%` }}
                                />
                              </div>
                              <span className="text-slate-400 text-xs w-10 text-right">{pct}%</span>
                            </div>
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {data.transactionCount === 0 && (
            <div className="glass-card rounded-xl p-12 text-center text-slate-500">
              Nenhuma transação encontrada no período selecionado.
            </div>
          )}
        </>
      )}
    </div>
  )
}
