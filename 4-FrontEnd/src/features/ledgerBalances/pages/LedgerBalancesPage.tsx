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
  useGetAllLedgerBalances,
  useCreateLedgerBalance,
  useUpdateLedgerBalance,
  useDeleteLedgerBalance,
} from '../hooks/useLedgerBalances';
import { LedgerBalance, BalanceType, CreateLedgerBalanceDto } from '../types';

type FormValues = {
  statementId: string;
  balanceType: BalanceType;
  amount: string;
  asOfDate: string;
};

const BALANCE_TYPES: BalanceType[] = ['LEDGER', 'AVAIL'];

function formatAmount(amount: number) {
  return `R$ ${amount.toFixed(2)}`;
}

export default function LedgerBalancesPage() {
  const { data = [], isLoading, error } = useGetAllLedgerBalances();
  const createMutation = useCreateLedgerBalance();
  const updateMutation = useUpdateLedgerBalance();
  const deleteMutation = useDeleteLedgerBalance();

  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<LedgerBalance | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<LedgerBalance | null>(null);

  const createForm = useForm<FormValues>({
    defaultValues: { statementId: '', balanceType: 'LEDGER', amount: '0', asOfDate: '' },
  });
  const editForm = useForm<FormValues>();

  function openEdit(balance: LedgerBalance) {
    setEditTarget(balance);
    editForm.reset({
      statementId: balance.statementId,
      balanceType: balance.balanceType,
      amount: String(balance.amount),
      asOfDate: balance.asOfDate.slice(0, 10),
    });
  }

  function handleCreate(values: FormValues) {
    const dto: CreateLedgerBalanceDto = {
      statementId: values.statementId,
      balanceType: values.balanceType,
      amount: parseFloat(values.amount),
      asOfDate: values.asOfDate,
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
          statementId: values.statementId,
          balanceType: values.balanceType,
          amount: parseFloat(values.amount),
          asOfDate: values.asOfDate,
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
    { key: 'statementId', header: 'Statement ID', mono: true },
    {
      key: 'balanceType',
      header: 'Type',
      width: '100px',
      render: (row: LedgerBalance) => (
        <Badge variant={row.balanceType === 'LEDGER' ? 'info' : 'accent'}>{row.balanceType}</Badge>
      ),
    },
    {
      key: 'amount',
      header: 'Amount',
      mono: true,
      align: 'right' as const,
      width: '140px',
      render: (row: LedgerBalance) => (
        <span style={{ color: row.amount >= 0 ? 'var(--success)' : 'var(--error)' }}>
          {formatAmount(row.amount)}
        </span>
      ),
    },
    {
      key: 'asOfDate',
      header: 'As Of Date',
      mono: true,
      width: '130px',
      render: (row: LedgerBalance) => new Date(row.asOfDate).toLocaleDateString(),
    },
    {
      key: 'createdAt',
      header: 'Created',
      mono: true,
      width: '120px',
      render: (row: LedgerBalance) => new Date(row.createdAt).toLocaleDateString(),
    },
    {
      key: 'actions',
      header: '',
      width: '140px',
      render: (row: LedgerBalance) => (
        <div style={{ display: 'flex', gap: '8px' }}>
          <Button variant="ghost" size="sm" onClick={e => { e.stopPropagation(); openEdit(row); }}>Edit</Button>
          <Button variant="danger" size="sm" onClick={e => { e.stopPropagation(); setDeleteTarget(row); }}>Delete</Button>
        </div>
      ),
    },
  ];

  const formFields = (form: ReturnType<typeof useForm<FormValues>>) => (
    <>
      <Input label="Statement ID" {...form.register('statementId', { required: 'Statement ID is required' })} error={form.formState.errors.statementId?.message} />
      <div>
        <label style={{ display: 'block', fontSize: 'var(--text-sm)', color: 'var(--text-secondary)', marginBottom: '6px' }}>Balance Type</label>
        <select {...form.register('balanceType')} style={{ width: '100%', padding: '8px 12px', background: 'var(--bg-elevated)', border: '1px solid var(--border)', borderRadius: 'var(--radius-md)', color: 'var(--text-primary)', fontSize: 'var(--text-sm)' }}>
          {BALANCE_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
        </select>
      </div>
      <Input
        label="Amount"
        type="number"
        step="0.01"
        {...form.register('amount', { required: 'Amount is required' })}
        error={form.formState.errors.amount?.message}
      />
      <Input
        label="As Of Date"
        type="date"
        {...form.register('asOfDate', { required: 'As of date is required' })}
        error={form.formState.errors.asOfDate?.message}
      />
    </>
  );

  if (isLoading) return <Spinner fullPage />;
  if (error) return <div style={{ color: 'var(--error)', padding: '24px' }}>Error: {(error as Error).message}</div>;

  return (
    <div>
      <PageHeader
        title="Ledger Balances"
        subtitle="Statement ledger and available balances"
        actions={<Button variant="primary" onClick={() => setCreateOpen(true)}>New Balance</Button>}
      />

      <Table columns={columns} data={data} keyFn={row => row.id} loading={isLoading} emptyMessage="No ledger balances found." />

      {/* Create Modal */}
      <Modal open={createOpen} onClose={() => setCreateOpen(false)} title="NEW LEDGER BALANCE">
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
      <Modal open={!!editTarget} onClose={() => setEditTarget(null)} title="EDIT LEDGER BALANCE">
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
          Delete this ledger balance (<strong style={{ color: 'var(--text-primary)' }}>{deleteTarget?.balanceType} — {deleteTarget ? formatAmount(deleteTarget.amount) : ''}</strong>)? This action cannot be undone.
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
