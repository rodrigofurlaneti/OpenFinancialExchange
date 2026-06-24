import { useState } from 'react'
import { useInstitutions } from './hooks/useInstitutions'
import { InstitutionModal } from './components/InstitutionModal'
import { Badge } from '../../shared/components/Badge'
import { EmptyState } from '../../shared/components/EmptyState'
import { Building2, Plus, Pencil } from 'lucide-react'
import type { FinancialInstitutionResponse } from '../../shared/types/api'

export function FinancialInstitutionsPage() {
  const { data: institutions, isLoading, isError } = useInstitutions()
  const [editing, setEditing] = useState<FinancialInstitutionResponse | null | undefined>(undefined)

  const isModalOpen = editing !== undefined

  function openCreate() { setEditing(null) }
  function openEdit(inst: FinancialInstitutionResponse) { setEditing(inst) }
  function closeModal() { setEditing(undefined) }

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-slate-100">Instituições Financeiras</h1>
          <p className="text-sm text-slate-500 mt-0.5">Gerencie bancos e seus identificadores OFX.</p>
        </div>
        <button onClick={openCreate} className="btn-primary flex items-center gap-2">
          <Plus size={16} /> Nova Instituição
        </button>
      </div>

      {/* Table */}
      <div className="glass-card overflow-hidden">
        {isLoading && (
          <div className="flex items-center justify-center py-16">
            <div className="w-8 h-8 border-2 border-emerald-500 border-t-transparent rounded-full animate-spin" />
          </div>
        )}

        {isError && (
          <div className="flex items-center justify-center py-16 text-red-400 text-sm">
            Erro ao carregar instituições. Verifique a conexão com a API.
          </div>
        )}

        {!isLoading && !isError && (!institutions || institutions.length === 0) && (
          <EmptyState
            icon={Building2}
            title="Nenhuma instituição cadastrada"
            description="Adicione um banco para começar a gerenciar contas OFX."
            action={
              <button onClick={openCreate} className="btn-primary flex items-center gap-2 mx-auto">
                <Plus size={16} /> Adicionar primeiro banco
              </button>
            }
          />
        )}

        {institutions && institutions.length > 0 && (
          <table className="w-full">
            <thead className="bg-slate-800/80">
              <tr>
                <th className="table-header">Bank ID</th>
                <th className="table-header">Organização</th>
                <th className="table-header">FID</th>
                <th className="table-header">Status</th>
                <th className="table-header">Criado em</th>
                <th className="table-header w-16" />
              </tr>
            </thead>
            <tbody>
              {institutions.map((inst) => (
                <tr key={inst.id} className="table-row">
                  <td className="table-cell font-mono font-semibold text-emerald-400">{inst.bankId}</td>
                  <td className="table-cell">{inst.orgName ?? <span className="text-slate-600">—</span>}</td>
                  <td className="table-cell font-mono text-xs">{inst.fid ?? <span className="text-slate-600">—</span>}</td>
                  <td className="table-cell">
                    <Badge variant={inst.isActive ? 'green' : 'red'}>
                      {inst.isActive ? 'Ativo' : 'Inativo'}
                    </Badge>
                  </td>
                  <td className="table-cell text-slate-500 text-xs">
                    {new Date(inst.createdAt).toLocaleDateString('pt-BR')}
                  </td>
                  <td className="table-cell">
                    <button
                      onClick={() => openEdit(inst)}
                      aria-label={`Editar ${inst.bankId}`}
                      className="p-1.5 rounded-lg text-slate-500 hover:text-slate-200 hover:bg-slate-700 transition-colors"
                    >
                      <Pencil size={14} />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {isModalOpen && <InstitutionModal editing={editing ?? null} onClose={closeModal} />}
    </div>
  )
}
