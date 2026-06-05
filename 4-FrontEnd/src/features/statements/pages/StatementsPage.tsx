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
  useGetAllStatements,
  useCreateStatement,
  useUpdateStatement,
  useDeleteStatement,
} from '../hooks/useStatements';
import { Statement, CreateStatementDto } from '../types';

type FormValues = {
  accountId: string;
  trnuid: string;
  statusCode: string;
  statusSeverity: string;
  startDate: string;
  endDate: string;
  timeZone: string;
};

function severityVariant(severity: string) {
  if (severity === 'INFO') return 'info' as const;
  if (severity === 'WARN') return 'warning' as const;
  if (severity === 'ERROR') return 'negative' as const;
  return 'default' as const;
}

export default function StatementsPage() {
  const { data = [], isLoading, error } = useGetAllStatements();
  const createMutation = useCreateStatement();
  const updateMutation = useUpdateStatement();
  const deleteMutation = useDeleteStatement();

  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<Statement | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Statement | null>(null);

  const createForm = useForm<FormValues>({
    defaultValues: { accountId: '', trnuid: '', statusCode: '0', statusSeverity: 'INFO', startDate: '', endDate: '', timeZone: '' },
  });
  const editForm = useForm<FormValues>();

  function openEdit(statement: Statement) {
    setEditTarget(statement);
    editForm.reset({
      accountId: statement.accountId,
      trnuid: statement.trnuid ?? '',
      statusCode: statement.statusCode,
      statusSeverity: statement.statusSeverity,
      startDate: statement.startDate.slice(0, 10),
      endDate: statement.endDate.slice(0, 10),
      timeZone: statement.timeZone ?? '',
    });
  }

  function handleCreate(values: FormValues) {
    const dto: CreateStatementDto = {
      accountId: values.accountId,
      trnuid: values.trnuid || undefined,
      statusCode: values.statusCode,
      statusSeverity: values.statusSeverity,
      startDate: values.startDate,
      endDate: values.endDate,
      timeZone: values.timeZone || undefined,
    };
    createMutation.mutate(dto, {
      onSuccess: () => { setCreateOpen(false); createForm.reset(); },
    });
  }

  function handleEdit(values: FormValues) {
    if (!editTarget) return;
    updateMutation.mutate(
      {
        id: editTarget.id,
        dto: {
          accountId: values.accountId,
          trnuid: values.trnuid || undefined,
          statusCode: values.statusCode,
          statusSeverity: values.statusSeverity,
          startDate: values.startDate,
          endDate: values.endDate,
          timeZone: values.timeZone || undefined,
        },
      },
      { onSuccess: () => setEditTarget(null) },
    );
  }

  function handleDelete() {
    if (!deleteTarget) return;
    deleteMutation.mutate(deleteTarget.id, { onSuccess: () => setDeleteTarget(null) });
  }

  const columns = [
    { key: 'accountId', header: 'Account ID', mono: true },
    { key: 'trnuid', header: 'TRNUID', mono: true, width: '120px', render: (row: Statement) => row.trnuid ?? '—' },
    { key: 'statusCode', header: 'Status Code', mono: true, width: '120px' },
    {
      key: 'statusSeverity',
      header: 'Severity',
      width: '100px',
      render: (row: Statement) => <Badge variant={severityVariant(row.statusSeverity)}>{row.statusSeverity}</Badge>,
    },
    {
      key: 'startDate',
      header: 'Start Date',
      mono: true,
      width: '120px',
      render: (row: Statement) => new Date(row.startDate).toLocaleDateString(),
    },
    {
      key: 'endDate',
      header: 'End Date',
      mono: true,
      width: '120px',
      render: (row: Statement) => new Date(row.endDate).toLocaleDateString(),
    },
    { key: 'timeZone', header: 'TZ', width: '80px', render: (row: Statement) => row.timeZone ?? '—' },
    {
      key: 'actions',
      header: '',
      width: '140px',
      render: (row: Statement) => (
        <div style={{ display: 'flex', gap: '8px' }}>
          <Button variant="ghost" size="sm" onClick={e => { e.stopPropagation(); openEdit(row); }}>Edit</Button>
          <Button variant="danger" size="sm" onClick={e => { e.stopPropagation(); setDeleteTarget(row); }}>Delete</Button>
        </div>
      ),
    },
  ];

  const formFields = (form: ReturnType<typeof useForm<FormValues>>) => (
    <>
      <Input label="Account ID" {...form.register('accountId', { required: 'Account ID is required' })} error={form.formState.errors.accountId?.message} />
      <Input label="TRNUID" {...form.register('trnuid')} />
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <Input label="Status Code" {...form.register('statusCode', { required: 'Status code is required' })} error={form.formState.errors.statusCode?.message} />
        <div>
          <label style={{ display: 'block', fontSize: 'var(--text-sm)', color: 'var(--text-secondary)', marginBottom: '6px' }}>Status Severity</label>
          <select {...form.register('statusSeverity')} style={{ width: '100%', padding: '8px 12px', background: 'var(--bg-elevated)', border: '1px solid var(--border)', borderRadius: 'var(--radius-md)', color: 'var(--text-primary)', fontSize: 'var(--text-sm)' }}>
            {['INFO', 'WARN', 'ERROR'].map(s => <option key={s} value={s}>{s}</option>)}
          </select>
        </div>
        <Input label="Start Date" type="date" {...form.register('startDate', { required: 'Start date is required' })} error={form.formState.errors.startDate?.message} />
        <Input label="End Date" type="date" {...form.register('endDate', { required: 'End date is required' })} error={form.formState.errors.endDate?.message} />
      </div>
      <Input label="Time Zone" {...form.register('timeZone')} hint="e.g. UTC, America/Sao_Paulo" />
    </>
  );

  if (isLoading) return <Spinner fullPage />;
  if (error) return <div style={{ color: 'var(--error)', padding: '24px' }}>Error: {(error as Error).message}</div>;

  return (
    <div>
      <PageHeader
        title="Statements"
        subtitle="Account bank statements from OFX imports"
        actions={<Button variant="primary" onClick={() => setCreateOpen(true)}>New Statement</Button>}
      />

      <Table columns={columns} data={data} keyFn={row => row.id} loading={isLoading} emptyMessage="No statements found." />

      {/* Create Modal */}
      <Modal open={createOpen} onClose={() => setCreateOpen(false)} title="NEW STATEMENT" width={540}>
        <form onSubmit={createForm.handleSubmit(handleCreate)} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          {formFields(createForm)}
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
      <Modal open={!!editTarget} onClose={() => setEditTarget(null)} title="EDIT STATEMENT" width={540}>
        <form onSubmit={editForm.handleSubmit(handleEdit)} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          {formFields(editForm)}
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
          Delete statement for account <strong style={{ color: 'var(--text-primary)' }}>{deleteTarget?.accountId}</strong>? This action cannot be undone.
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
