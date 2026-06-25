import { useState } from 'react'
import { useCategories, useDeleteCategory } from './hooks/useCategories'
import { CategoryModal } from './components/CategoryModal'
import { Badge } from '../../shared/components/Badge'
import { EmptyState } from '../../shared/components/EmptyState'
import { extractErrorMessage } from '../../core/api/client'
import { Tag, Plus, Pencil, Trash2, Lock } from 'lucide-react'
import type { CategoryResponse, CategoryKind } from '../../shared/types/api'

function kindBadge(kind: CategoryKind): { variant: 'green' | 'red' | 'slate'; label: string } {
  if (kind === 'CREDIT') return { variant: 'green', label: 'Receita' }
  if (kind === 'DEBIT') return { variant: 'red', label: 'Despesa' }
  return { variant: 'slate', label: 'Ambos' }
}

export function CategoriesPage() {
  const { data: categories, isLoading, isError } = useCategories()
  const deleteMutation = useDeleteCategory()
  const [editing, setEditing] = useState<CategoryResponse | null | undefined>(undefined)

  const isModalOpen = editing !== undefined

  function handleDelete(cat: CategoryResponse) {
    if (window.confirm(`Remover a categoria "${cat.name}"?`)) {
      deleteMutation.mutate(cat.id)
    }
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-slate-100">Categorias</h1>
          <p className="text-sm text-slate-500 mt-0.5">
            Organize suas transações. Categorias do sistema não podem ser editadas.
          </p>
        </div>
        <button onClick={() => setEditing(null)} className="btn-primary flex items-center gap-2">
          <Plus size={16} /> Nova Categoria
        </button>
      </div>

      {deleteMutation.isError && (
        <div className="mb-4 px-4 py-3 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
          {extractErrorMessage(deleteMutation.error)}
        </div>
      )}

      <div className="glass-card overflow-hidden">
        {isLoading && (
          <div className="flex items-center justify-center py-16">
            <div className="w-8 h-8 border-2 border-emerald-500 border-t-transparent rounded-full animate-spin" />
          </div>
        )}

        {isError && (
          <div className="flex items-center justify-center py-16 text-red-400 text-sm">
            Erro ao carregar categorias. Verifique a conexão com a API.
          </div>
        )}

        {!isLoading && !isError && (!categories || categories.length === 0) && (
          <EmptyState
            icon={Tag}
            title="Nenhuma categoria"
            description="Adicione uma categoria para classificar suas transações."
            action={
              <button onClick={() => setEditing(null)} className="btn-primary flex items-center gap-2 mx-auto">
                <Plus size={16} /> Adicionar categoria
              </button>
            }
          />
        )}

        {categories && categories.length > 0 && (
          <table className="w-full">
            <thead className="bg-slate-800/80">
              <tr>
                <th className="table-header">Categoria</th>
                <th className="table-header">Tipo</th>
                <th className="table-header">Origem</th>
                <th className="table-header w-24" />
              </tr>
            </thead>
            <tbody>
              {categories.map((cat) => {
                const badge = kindBadge(cat.kind)
                return (
                  <tr key={cat.id} className="table-row">
                    <td className="table-cell">
                      <div className="flex items-center gap-2.5">
                        <span
                          className="w-3 h-3 rounded-full flex-shrink-0"
                          style={{ backgroundColor: cat.color }}
                        />
                        <span className="font-medium text-slate-200">{cat.name}</span>
                      </div>
                    </td>
                    <td className="table-cell">
                      <Badge variant={badge.variant}>{badge.label}</Badge>
                    </td>
                    <td className="table-cell">
                      {cat.isSystem ? (
                        <span className="inline-flex items-center gap-1 text-xs text-slate-500">
                          <Lock size={12} /> Sistema
                        </span>
                      ) : (
                        <span className="text-xs text-emerald-400">Personalizada</span>
                      )}
                    </td>
                    <td className="table-cell">
                      {!cat.isSystem && (
                        <div className="flex items-center gap-1">
                          <button
                            onClick={() => setEditing(cat)}
                            aria-label={`Editar ${cat.name}`}
                            className="p-1.5 rounded-lg text-slate-500 hover:text-slate-200 hover:bg-slate-700 transition-colors"
                          >
                            <Pencil size={14} />
                          </button>
                          <button
                            onClick={() => handleDelete(cat)}
                            aria-label={`Remover ${cat.name}`}
                            className="p-1.5 rounded-lg text-slate-500 hover:text-red-400 hover:bg-red-500/10 transition-colors"
                          >
                            <Trash2 size={14} />
                          </button>
                        </div>
                      )}
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        )}
      </div>

      {isModalOpen && <CategoryModal editing={editing ?? null} onClose={() => setEditing(undefined)} />}
    </div>
  )
}
