import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { PageHeader } from '../../../design-system/components/PageHeader';
import { Button } from '../../../design-system/components/Button';
import { Table } from '../../../design-system/components/Table';
import { Modal } from '../../../design-system/components/Modal';
import { Input } from '../../../design-system/components/Input';
import { Badge } from '../../../design-system/components/Badge';
import { Spinner } from '../../../design-system/components/Spinner';
import {
  useGetAllSignonSessions,
  useCreateSignonSession,
  useUpdateSignonSession,
  useDeleteSignonSession,
} from '../hooks/useSignonSessions';
import { SignonSession, CreateSignonSessionDto, UpdateSignonSessionDto } from '../types';

type CreateFormValues = {
  importId: string;
  statusCode: string;
  statusSeverity: string;
  serverDateRaw: string;
  language: string;
};

type EditFormValues = {
  statusCode: string;
  statusSeverity: string;
  serverDateRaw: string;
  language: string;
};

function severityVariant(severity: string) {
  if (severity === 'INFO') return 'info' as const;
  if (severity === 'WARN') return 'warning' as const;
  if (severity === 'ERROR') return 'negative' as const;
  return 'default' as const;
}

export default function SignonSessionsPage() {
  const { data = [], isLoading, error } = useGetAllSignonSessions();
  const createMutation = useCreateSignonSession();
  const updateMutation = useUpdateSignonSession();
  const deleteMutation = useDeleteSignonSession();

  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<SignonSession | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<SignonSession | null>(null);

  const createForm = useForm<CreateFormValues>({
    defaultValues: { importId: '', statusCode: '0', statusSeverity: 'INFO', serverDateRaw: '', language: 'ENG' },
  });
  const editForm = useForm<EditFormValues>();

  function openEdit(session: SignonSession) {
    setEditTarget(session);
    editForm.reset({
      statusCode: session.statusCode,
      statusSeverity: session.statusSeverity,
      serverDateRaw: session.serverDateRaw ?? '',
      language: session.language ?? '',
    });
  }

  function handleCreate(values: CreateFormValues) {
    const dto: CreateSignonSessionDto = {
      importId: values.importId,
      statusCode: values.statusCode,
      statusSeverity: values.statusSeverity,
      serverDateRaw: values.serverDateRaw || undefined,
      language: values.language || undefined,
    };
    createMutation.mutate(dto, {
      onSuccess: () => { setCreateOpen(false); createForm.reset(); },
    });
  }

  function handleEdit(values: EditFormValues) {
    if (!editTarget) return;
    const dto: UpdateSignonSessionDto = {
      statusCode: values.statusCode,
      statusSeverity: values.statusSeverity,
      serverDateRaw: values.serverDateRaw || undefined,
      language: values.language || undefined,
    };
    updateMutation.mutate({ id: editTarget.id, dto }, { onSuccess: () => setEditTarget(null) });
  }

  function handleDelete() {
    if (!deleteTarget) return;
    deleteMutation.mutate(deleteTarget.id, { onSuccess: () => setDeleteTarget(null) });
  }

  const columns = [
    { key: 'importId', header: 'Import ID', mono: true },
    { key: 'statusCode', header: 'Status Code', mono: true, width: '120px' },
    {
      key: 'statusSeverity',
      header: 'Severity',
      width: '110px',
      render: (row: SignonSession) => (
        <Badge variant={severityVariant(row.statusSeverity)}>{row.statusSeverity}</Badge>
      ),
    },
    { key: 'language', header: 'Language', width: '100px', render: (row: SignonSession) => row.language ?? '—' },
    {
      key: 'serverDate',
      header: 'Server Date',
      mono: true,
      width: '160px',
      render: (row: SignonSession) => row.serverDate ? new Date(row.serverDate).toLocaleString() : (row.serverDateRaw ?? '—'),
    },
    {
      key: 'createdAt',
      header: 'Created',
      mono: true,
      width: '120px',
      render: (row: SignonSession) => new Date(row.createdAt).toLocaleDateString(),
    },
    {
      key: 'actions',
      header: '',
      width: '140px',
      render: (row: SignonSession) => (
        <div style={{ display: 'flex', gap: '8px' }}>
          <Button variant="ghost" size="sm" onClick={e => { e.stopPropagation(); openEdit(row); }}>Edit</Button>
          <Button variant="danger" size="sm" onClick={e => { e.stopPropagation(); setDeleteTarget(row); }}>Delete</Button>
        </div>
      ),
    },
  ];

  if (isLoading) return <Spinner fullPage />;
  if (error) return <div style={{ color: 'var(--error)', padding: '24px' }}>Error: {(error as Error).message}</div>;

  return (
    <div>
      <PageHeader
        title="Signon Sessions"
        subtitle="OFX authentication session records"
        actions={<Button variant="primary" onClick={() => setCreateOpen(true)}>New Session</Button>}
      />

      <Table columns={columns} data={data} keyFn={row => row.id} loading={isLoading} emptyMessage="No signon sessions found." />

      {/* Create Modal */}
      <Modal open={createOpen} onClose={() => setCreateOpen(false)} title="NEW SIGNON SESSION">
        <form onSubmit={createForm.handleSubmit(handleCreate)} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <Input label="Import ID" {...createForm.register('importId', { required: 'Import ID is required' })} error={createForm.formState.errors.importId?.message} />
          <Input label="Status Code" {...createForm.register('statusCode', { required: 'Status code is required' })} error={createForm.formState.errors.statusCode?.message} />
          <div>
            <label style={{ display: 'block', fontSize: 'var(--text-sm)', color: 'var(--text-secondary)', marginBottom: '6px' }}>Status Severity</label>
            <select {...createForm.register('statusSeverity')} style={{ width: '100%', padding: '8px 12px', background: 'var(--bg-elevated)', border: '1px solid var(--border)', borderRadius: 'var(--radius-md)', color: 'var(--text-primary)', fontSize: 'var(--text-sm)' }}>
              {['INFO', 'WARN', 'ERROR'].map(s => <option key={s} value={s}>{s}</option>)}
            </select>
          </div>
          <Input label="Server Date (raw)" {...createForm.register('serverDateRaw')} hint="e.g. 20240101120000" />
          <Input label="Language" {...createForm.register('language')} hint="e.g. ENG" />
          {createMutation.error && (
            <div style={{ color: 'var(--error)', fontSize: 'var(--text-sm)', padding: '8px', background: 'rgba(239,68,68,0.1)', borderRadius: 'var(--radius-md)' }}>
              {(createMutation.error as Error).message}
            </div>
          )}
          <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
            <Button variant="ghost" onClick={() => setCreateOpen(false)} type="button">Cancel</Button>
            <Button variant="primary" type="submit" loading={createMutation.isPending}>Create</Button>
          </div>
        </form>
      </Modal>

      {/* Edit Modal */}
      <Modal open={!!editTarget} onClose={() => setEditTarget(null)} title="EDIT SIGNON SESSION">
        <form onSubmit={editForm.handleSubmit(handleEdit)} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <Input label="Status Code" {...editForm.register('statusCode', { required: 'Status code is required' })} error={editForm.formState.errors.statusCode?.message} />
          <div>
            <label style={{ display: 'block', fontSize: 'var(--text-sm)', color: 'var(--text-secondary)', marginBottom: '6px' }}>Status Severity</label>
            <select {...editForm.register('statusSeverity')} style={{ width: '100%', padding: '8px 12px', background: 'var(--bg-elevated)', border: '1px solid var(--border)', borderRadius: 'var(--radius-md)', color: 'var(--text-primary)', fontSize: 'var(--text-sm)' }}>
              {['INFO', 'WARN', 'ERROR'].map(s => <option key={s} value={s}>{s}</option>)}
            </select>
          </div>
          <Input label="Server Date (raw)" {...editForm.register('serverDateRaw')} />
          <Input label="Language" {...editForm.register('language')} />
          {updateMutation.error && (
            <div style={{ color: 'var(--error)', fontSize: 'var(--text-sm)', padding: '8px', background: 'rgba(239,68,68,0.1)', borderRadius: 'var(--radius-md)' }}>
              {(updateMutation.error as Error).message}
            </div>
          )}
          <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
            <Button variant="ghost" onClick={() => setEditTarget(null)} type="button">Cancel</Button>
            <Button variant="primary" type="submit" loading={updateMutation.isPending}>Save</Button>
          </div>
        </form>
      </Modal>

      {/* Delete Modal */}
      <Modal open={!!deleteTarget} onClose={() => setDeleteTarget(null)} title="CONFIRM DELETE" width={400}>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '20px' }}>
          Delete signon session for import <strong style={{ color: 'var(--text-primary)' }}>{deleteTarget?.importId}</strong>? This action cannot be undone.
        </p>
        {deleteMutation.error && (
          <div style={{ color: 'var(--error)', fontSize: 'var(--text-sm)', padding: '8px', background: 'rgba(239,68,68,0.1)', borderRadius: 'var(--radius-md)', marginBottom: '16px' }}>
            {(deleteMutation.error as Error).message}
          </div>
        )}
        <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
          <Button variant="ghost" onClick={() => setDeleteTarget(null)}>Cancel</Button>
          <Button variant="danger" onClick={handleDelete} loading={deleteMutation.isPending}>Delete</Button>
        </div>
      </Modal>
    </div>
  );
}
