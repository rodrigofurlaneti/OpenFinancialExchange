import { useState, useRef } from 'react'
import { useForm } from 'react-hook-form'
import { Modal } from '../../../shared/components/Modal'
import { useCreateOfxImport } from '../hooks/useOfx'
import { useBankAccounts } from '../../bank-accounts/hooks/useBankAccounts'
import { extractErrorMessage } from '../../../core/api/client'
import { Upload, FileText } from 'lucide-react'

interface ImportModalProps { onClose: () => void }
interface FormValues { bankAccountId: string }

// Parse OFX file header fields
function parseOfxHeader(content: string) {
  const get = (key: string) => {
    const m = content.match(new RegExp(`${key}:(\\S+)`))
    return m ? m[1].trim() : null
  }
  return {
    ofxHeaderVersion: get('OFXHEADER') ? Number(get('OFXHEADER')) : null,
    ofxVersion: get('VERSION') ? Number(get('VERSION')) : null,
    encoding: get('ENCODING'),
    charset: get('CHARSET'),
    security: get('SECURITY'),
    compression: get('COMPRESSION'),
    oldFileUid: get('OLDFILEUID'),
    newFileUid: get('NEWFILEUID'),
  }
}

async function sha256(content: string): Promise<string> {
  const encoder = new TextEncoder()
  const data = encoder.encode(content)
  const hashBuffer = await crypto.subtle.digest('SHA-256', data)
  return Array.from(new Uint8Array(hashBuffer))
    .map((b) => b.toString(16).padStart(2, '0'))
    .join('')
}

export function ImportModal({ onClose }: ImportModalProps) {
  const { data: accounts } = useBankAccounts()
  const { register, handleSubmit, formState: { errors } } = useForm<FormValues>()
  const createMutation = useCreateOfxImport()
  const [file, setFile] = useState<File | null>(null)
  const [dragOver, setDragOver] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)

  function handleFile(f: File) { setFile(f) }

  async function onSubmit(values: FormValues) {
    if (!file) return

    const content = await file.text()
    const hash = await sha256(content)
    const header = parseOfxHeader(content)

    createMutation.mutate(
      {
        bankAccountId: Number(values.bankAccountId),
        fileName: file.name,
        fileHash: hash,
        ofxData: content,
        ...header,
      },
      { onSuccess: onClose }
    )
  }

  return (
    <Modal title="Importar Arquivo OFX" onClose={onClose}>
      {createMutation.error && (
        <div className="mb-4 px-4 py-3 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
          {extractErrorMessage(createMutation.error)}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
        {/* File drop zone */}
        <div
          role="button"
          aria-label="Área de upload de arquivo OFX"
          tabIndex={0}
          className={`relative border-2 border-dashed rounded-xl p-8 text-center cursor-pointer transition-all duration-200 ${
            dragOver ? 'border-emerald-500 bg-emerald-500/10' : 'border-slate-600 hover:border-slate-400'
          } ${file ? 'border-emerald-600 bg-emerald-600/10' : ''}`}
          onDragOver={(e) => { e.preventDefault(); setDragOver(true) }}
          onDragLeave={() => setDragOver(false)}
          onDrop={(e) => { e.preventDefault(); setDragOver(false); const f = e.dataTransfer.files[0]; if (f) handleFile(f) }}
          onClick={() => inputRef.current?.click()}
          onKeyDown={(e) => e.key === 'Enter' && inputRef.current?.click()}
        >
          <input
            ref={inputRef}
            type="file"
            accept=".ofx,.OFX"
            className="hidden"
            onChange={(e) => { const f = e.target.files?.[0]; if (f) handleFile(f) }}
          />
          {file ? (
            <div className="flex flex-col items-center gap-2">
              <FileText size={32} className="text-emerald-400" />
              <p className="font-medium text-slate-200">{file.name}</p>
              <p className="text-xs text-slate-500">{(file.size / 1024).toFixed(1)} KB</p>
            </div>
          ) : (
            <div className="flex flex-col items-center gap-2">
              <Upload size={32} className="text-slate-500" />
              <p className="text-slate-400">Arraste o arquivo OFX ou clique para selecionar</p>
              <p className="text-xs text-slate-600">Formatos: .ofx</p>
            </div>
          )}
        </div>

        {!file && (
          <p className="text-red-400 text-xs text-center -mt-2">
            {createMutation.isPending ? '' : file === null ? '' : 'Selecione um arquivo'}
          </p>
        )}

        <div>
          <label className="form-label">Conta Bancária *</label>
          <select
            className="form-input"
            {...register('bankAccountId', { required: 'Selecione uma conta' })}
          >
            <option value="">Selecione a conta…</option>
            {accounts?.map((a) => (
              <option key={a.id} value={a.id}>
                {a.bankId} — {a.acctId}
                {a.branchId ? ` (ag. ${a.branchId})` : ''}
              </option>
            ))}
          </select>
          {errors.bankAccountId && <p className="form-error">{errors.bankAccountId.message}</p>}
        </div>

        <div className="flex justify-end gap-3 pt-2">
          <button type="button" onClick={onClose} className="btn-secondary">Cancelar</button>
          <button type="submit" disabled={createMutation.isPending || !file} className="btn-primary">
            {createMutation.isPending ? 'Importando…' : 'Importar'}
          </button>
        </div>
      </form>
    </Modal>
  )
}
