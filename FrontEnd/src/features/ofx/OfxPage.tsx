import { useState } from 'react'
import { useOfxImports, useOfxStatements, useTransactionsByStatement, useAssignCategory } from './hooks/useOfx'
import { useCategories } from '../categories/hooks/useCategories'
import { ImportModal } from './components/ImportModal'
import { TransactionTable } from './components/TransactionTable'
import { EmptyState } from '../../shared/components/EmptyState'
import { FileUp, Plus, FileText, ChevronRight } from 'lucide-react'

export function OfxPage() {
  const { data: imports, isLoading: loadingImports } = useOfxImports()
  const { data: statements, isLoading: loadingStatements } = useOfxStatements()
  const [selectedStatementId, setSelectedStatementId] = useState<number | null>(null)
  const [showImportModal, setShowImportModal] = useState(false)

  const { data: transactions = [], isLoading: loadingTxns } = useTransactionsByStatement(selectedStatementId)
  const { data: categories = [] } = useCategories()
  const assignCategory = useAssignCategory()

  const selectedStatement = statements?.find((s) => s.id === selectedStatementId)

  function formatDate(d: string | null) {
    if (!d) return '—'
    return new Date(d).toLocaleDateString('pt-BR')
  }

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-slate-100">Importações OFX</h1>
          <p className="text-sm text-slate-500 mt-0.5">Importe extratos e visualize transações.</p>
        </div>
        <button onClick={() => setShowImportModal(true)} className="btn-primary flex items-center gap-2">
          <Plus size={16} /> Importar OFX
        </button>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
        {/* Imports list */}
        <div className="xl:col-span-1 space-y-4">
          <h2 className="text-sm font-semibold text-slate-400 uppercase tracking-wider">Arquivos Importados</h2>

          {loadingImports && (
            <div className="flex justify-center py-8">
              <div className="w-6 h-6 border-2 border-emerald-500 border-t-transparent rounded-full animate-spin" />
            </div>
          )}

          {!loadingImports && (!imports || imports.length === 0) && (
            <EmptyState
              icon={FileUp}
              title="Nenhum arquivo OFX"
              description="Clique em 'Importar OFX' para processar um extrato."
            />
          )}

          {imports?.map((imp) => (
            <div key={imp.id} className="glass-card p-4">
              <div className="flex items-start gap-3">
                <div className="p-2 bg-slate-700 rounded-lg flex-shrink-0">
                  <FileText size={16} className="text-slate-400" />
                </div>
                <div className="min-w-0 flex-1">
                  <p className="text-sm font-medium text-slate-200 truncate">{imp.fileName}</p>
                  <p className="text-xs text-slate-500 mt-0.5">
                    {new Date(imp.importedAt).toLocaleString('pt-BR')}
                  </p>
                  <p className="text-xs text-slate-600 font-mono mt-1 truncate">{imp.fileHash.slice(0, 16)}…</p>
                </div>
              </div>
            </div>
          ))}

          {/* Statements list */}
          {statements && statements.length > 0 && (
            <>
              <h2 className="text-sm font-semibold text-slate-400 uppercase tracking-wider pt-2">Extratos</h2>
              {loadingStatements && (
                <div className="flex justify-center py-4">
                  <div className="w-6 h-6 border-2 border-emerald-500 border-t-transparent rounded-full animate-spin" />
                </div>
              )}
              <div className="space-y-2">
                {statements.map((stmt) => {
                  const isSelected = selectedStatementId === stmt.id
                  return (
                    <button
                      key={stmt.id}
                      onClick={() => setSelectedStatementId(isSelected ? null : stmt.id)}
                      className={`w-full text-left glass-card p-3 flex items-center justify-between transition-all duration-200 ${
                        isSelected ? 'border-emerald-600/60 bg-emerald-600/10' : 'hover:bg-slate-700/30'
                      }`}
                    >
                      <div>
                        <p className="text-sm font-medium text-slate-200">{stmt.curDef} — Extrato #{stmt.id}</p>
                        <p className="text-xs text-slate-500">
                          {formatDate(stmt.dtStart)} → {formatDate(stmt.dtEnd)}
                        </p>
                      </div>
                      <ChevronRight
                        size={14}
                        className={`text-slate-500 transition-transform ${isSelected ? 'rotate-90 text-emerald-400' : ''}`}
                      />
                    </button>
                  )
                })}
              </div>
            </>
          )}
        </div>

        {/* Transactions panel */}
        <div className="xl:col-span-2">
          {selectedStatement ? (
            <>
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-sm font-semibold text-slate-400 uppercase tracking-wider">
                  Transações — Extrato #{selectedStatement.id}
                </h2>
                <span className="text-xs text-slate-500">
                  {formatDate(selectedStatement.dtStart)} → {formatDate(selectedStatement.dtEnd)}
                </span>
              </div>
              <div className="glass-card overflow-hidden">
                <TransactionTable
                  transactions={transactions}
                  isLoading={loadingTxns}
                  categories={categories}
                  isAssigning={assignCategory.isPending}
                  onAssignCategory={(transactionId, categoryId) =>
                    assignCategory.mutate({ transactionId, categoryId })
                  }
                />
              </div>
            </>
          ) : (
            <div className="glass-card flex items-center justify-center py-24">
              <EmptyState
                icon={FileText}
                title="Selecione um extrato"
                description="Clique em um extrato à esquerda para visualizar as transações."
              />
            </div>
          )}
        </div>
      </div>

      {showImportModal && <ImportModal onClose={() => setShowImportModal(false)} />}
    </div>
  )
}
