import { useMemo, useState } from 'react'
import {
  TrendingUp,
  TrendingDown,
  Scale,
  Hash,
  ChevronLeft,
  ChevronRight,
  PieChart,
  ArrowLeftRight,
} from 'lucide-react'
import { useDashboardSummary } from './hooks/useDashboard'
import { PERIODS, getPeriodRange, toLocalIso, type Period } from './period'
import { CategoryDonut, type DonutSegment } from './components/CategoryDonut'

const UNCATEGORIZED_COLOR = '#64748b'
const DONUT_MAX = 8

function fmt(value: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
}

const TRN_TYPE_LABELS: Record<string, string> = {
  CREDIT: 'Crédito', DEBIT: 'Débito', INT: 'Juros', DIV: 'Dividendo', FEE: 'Tarifa',
  SRVCHG: 'Serviço', DEP: 'Depósito', ATM: 'Saque ATM', POS: 'Ponto de Venda',
  XFER: 'Transferência', CHECK: 'Cheque', PAYMENT: 'Pagamento', CASH: 'Dinheiro',
  DIRECTDEP: 'Depósito Direto', DIRECTDEBIT: 'Débito Direto', REPEATPMT: 'Pagamento Recorrente',
  OTHER: 'Outros',
}
const trnLabel = (t: string) => TRN_TYPE_LABELS[t] ?? t

type Metric = 'debit' | 'credit'

export function DashboardPage() {
  const [period, setPeriod] = useState<Period>('month')
  const [offset, setOffset] = useState(0)
  const [metric, setMetric] = useState<Metric>('debit')

  const range = useMemo(() => getPeriodRange(period, offset), [period, offset])
  const { data, isLoading, isError } = useDashboardSummary(toLocalIso(range.from), toLocalIso(range.to))

  function changePeriod(p: Period) {
    setPeriod(p)
    setOffset(0)
  }

  const catRows = useMemo(() => {
    if (!data) return []
    return data.byCategory
      .filter((c) => !c.isInternal)
      .map((c) => ({
        id: c.categoryId,
        name: c.categoryName,
        color: c.color ?? UNCATEGORIZED_COLOR,
        value: metric === 'debit' ? c.debit : c.credit,
        count: c.count,
      }))
      .filter((c) => c.value > 0)
      .sort((a, b) => b.value - a.value)
  }, [data, metric])

  const catTotal = catRows.reduce((s, c) => s + c.value, 0)

  const donutSegments: DonutSegment[] = useMemo(() => {
    if (catRows.length <= DONUT_MAX) {
      return catRows.map((c) => ({ name: c.name, value: c.value, color: c.color }))
    }
    const top = catRows.slice(0, DONUT_MAX - 1).map((c) => ({ name: c.name, value: c.value, color: c.color }))
    const rest = catRows.slice(DONUT_MAX - 1).reduce((s, c) => s + c.value, 0)
    return [...top, { name: 'Outros', value: rest, color: '#475569' }]
  }, [catRows])

  const cards = data
    ? [
        { label: 'Entradas', value: fmt(data.totalCredits), icon: TrendingUp, color: 'text-emerald-400', bg: 'bg-emerald-500/10 border-emerald-500/20' },
        { label: 'Saídas', value: fmt(data.totalDebits), icon: TrendingDown, color: 'text-red-400', bg: 'bg-red-500/10 border-red-500/20' },
        { label: 'Saldo Líquido', value: fmt(data.netBalance), icon: Scale, color: data.netBalance >= 0 ? 'text-emerald-400' : 'text-red-400', bg: data.netBalance >= 0 ? 'bg-emerald-500/10 border-emerald-500/20' : 'bg-red-500/10 border-red-500/20' },
        { label: 'Transações', value: data.transactionCount.toLocaleString('pt-BR'), icon: Hash, color: 'text-blue-400', bg: 'bg-blue-500/10 border-blue-500/20' },
      ]
    : []

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-100">Dashboard</h1>
        <p className="text-slate-400 text-sm mt-1">Resumo financeiro das transações OFX</p>
      </div>

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex flex-wrap gap-2">
          {PERIODS.map(({ key, label }) => (
            <button
              key={key}
              onClick={() => changePeriod(key)}
              className={`px-3.5 py-1.5 rounded-lg text-sm font-medium transition-all duration-200 ${
                period === key
                  ? 'bg-emerald-600 text-white shadow-lg shadow-emerald-900/30'
                  : 'bg-slate-800/60 text-slate-400 hover:bg-slate-700/60 hover:text-slate-200 border border-slate-700/50'
              }`}
            >
              {label}
            </button>
          ))}
        </div>

        <div className="flex items-center gap-1 self-start sm:self-auto">
          <button
            onClick={() => setOffset((o) => o - 1)}
            aria-label="Período anterior"
            className="p-2 rounded-lg text-slate-400 hover:text-slate-100 hover:bg-slate-700/60 border border-slate-700/50 transition-colors"
          >
            <ChevronLeft size={16} />
          </button>
          <span className="min-w-[9rem] text-center text-sm font-semibold text-slate-200 px-2 capitalize">
            {range.label}
          </span>
          <button
            onClick={() => setOffset((o) => Math.min(0, o + 1))}
            disabled={offset >= 0}
            aria-label="Próximo período"
            className="p-2 rounded-lg text-slate-400 hover:text-slate-100 hover:bg-slate-700/60 border border-slate-700/50 transition-colors disabled:opacity-30 disabled:cursor-not-allowed disabled:hover:bg-transparent"
          >
            <ChevronRight size={16} />
          </button>
        </div>
      </div>

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

      {isError && (
        <div className="glass-card rounded-xl p-6 border border-red-500/20 bg-red-500/5 text-red-400 text-sm">
          Erro ao carregar dados. Verifique se a API está rodando.
        </div>
      )}

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

          {(data.internalCredits > 0 || data.internalDebits > 0) && (
            <div className="glass-card rounded-xl px-5 py-3 flex flex-wrap items-center gap-x-6 gap-y-1 text-sm border border-slate-700/40">
              <span className="text-slate-400 flex items-center gap-2">
                <ArrowLeftRight size={14} className="text-slate-500" />
                Movimentações internas <span className="text-slate-600">(não entram no resultado)</span>
              </span>
              <span className="text-slate-400">
                Resgatado: <span className="font-semibold text-slate-200">{fmt(data.internalCredits)}</span>
              </span>
              <span className="text-slate-400">
                Aplicado: <span className="font-semibold text-slate-200">{fmt(data.internalDebits)}</span>
              </span>
            </div>
          )}

          <div className="glass-card rounded-xl overflow-hidden">
            <div className="px-6 py-4 border-b border-slate-700/50 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
              <h2 className="text-slate-200 font-semibold flex items-center gap-2">
                <PieChart size={16} className="text-emerald-400" />
                {metric === 'debit' ? 'Gastos por categoria' : 'Receitas por categoria'}
              </h2>
              <div className="inline-flex rounded-lg border border-slate-700/60 p-0.5 bg-slate-800/40 self-start">
                <button
                  onClick={() => setMetric('debit')}
                  className={`px-3 py-1 rounded-md text-xs font-medium transition-colors ${metric === 'debit' ? 'bg-red-500/20 text-red-300' : 'text-slate-400 hover:text-slate-200'}`}
                >
                  Despesas
                </button>
                <button
                  onClick={() => setMetric('credit')}
                  className={`px-3 py-1 rounded-md text-xs font-medium transition-colors ${metric === 'credit' ? 'bg-emerald-500/20 text-emerald-300' : 'text-slate-400 hover:text-slate-200'}`}
                >
                  Receitas
                </button>
              </div>
            </div>

            {catRows.length === 0 ? (
              <div className="p-10 text-center text-slate-500 text-sm">
                Nenhuma {metric === 'debit' ? 'despesa' : 'receita'} categorizada neste período.
              </div>
            ) : (
              <div className="flex flex-col lg:flex-row gap-6 p-6">
                <div className="flex justify-center lg:justify-start shrink-0">
                  <CategoryDonut
                    segments={donutSegments}
                    centerValue={fmt(catTotal)}
                    centerLabel={metric === 'debit' ? 'Despesas' : 'Receitas'}
                  />
                </div>

                <div className="flex-1 space-y-2.5">
                  {catRows.map((c) => {
                    const pct = catTotal > 0 ? (c.value / catTotal) * 100 : 0
                    return (
                      <div key={c.id ?? 'none'} className="flex items-center gap-3">
                        <span className="w-2.5 h-2.5 rounded-full shrink-0" style={{ backgroundColor: c.color }} />
                        <span className="text-sm text-slate-300 w-32 sm:w-40 truncate" title={c.name}>
                          {c.name}
                        </span>
                        <div className="flex-1 h-2 bg-slate-700/50 rounded-full overflow-hidden">
                          <div className="h-full rounded-full" style={{ width: `${pct}%`, backgroundColor: c.color }} />
                        </div>
                        <span className="text-xs text-slate-500 w-10 text-right">{pct.toFixed(0)}%</span>
                        <span className={`text-sm font-semibold w-28 text-right ${metric === 'debit' ? 'text-red-400' : 'text-emerald-400'}`}>
                          {fmt(c.value)}
                        </span>
                      </div>
                    )
                  })}
                </div>
              </div>
            )}
          </div>

          {data.byType.length > 0 && (
            <div className="glass-card rounded-xl overflow-hidden">
              <div className="px-6 py-4 border-b border-slate-700/50">
                <h2 className="text-slate-200 font-semibold">Detalhamento por tipo</h2>
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
                      const pct = totalVolume > 0 ? ((Math.abs(row.total) / totalVolume) * 100).toFixed(1) : '0.0'
                      const isPositive = row.total >= 0
                      return (
                        <tr key={row.trnType} className="border-b border-slate-700/30 hover:bg-slate-700/20 transition-colors">
                          <td className="px-6 py-3 text-slate-300 font-medium">
                            {trnLabel(row.trnType)}
                            <span className="ml-2 text-xs text-slate-500 font-mono">{row.trnType}</span>
                          </td>
                          <td className="px-6 py-3 text-right text-slate-400">{row.count.toLocaleString('pt-BR')}</td>
                          <td className={`px-6 py-3 text-right font-semibold ${isPositive ? 'text-emerald-400' : 'text-red-400'}`}>
                            {fmt(row.total)}
                          </td>
                          <td className="px-6 py-3 text-right">
                            <div className="flex items-center justify-end gap-2">
                              <div className="w-24 bg-slate-700 rounded-full h-1.5">
                                <div className={`h-1.5 rounded-full ${isPositive ? 'bg-emerald-500' : 'bg-red-500'}`} style={{ width: `${Math.min(Number(pct), 100)}%` }} />
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
