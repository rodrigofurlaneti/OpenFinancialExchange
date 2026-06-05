import { useState, useMemo } from 'react';
import { useForm } from 'react-hook-form';
import { PageHeader } from '../../../design-system/components/PageHeader';
import { Button } from '../../../design-system/components/Button';
import { Table } from '../../../design-system/components/Table';
import { Modal } from '../../../design-system/components/Modal';
import { Input } from '../../../design-system/components/Input';
import { Badge } from '../../../design-system/components/Badge';
import { Spinner } from '../../../design-system/components/Spinner';
import {
  useGetAllTransactions,
  useCreateTransaction,
  useUpdateTransaction,
  useDeleteTransaction,
  useReconcileTransaction,
} from '../hooks/useTransactions';
import { Transaction, MovementNature, CreateTransactionDto } from '../types';

type FormValues = {
  statementId: string;
  categoryId: string;
  transactionType: string;
  postedDate: string;
  amount: string;
  absoluteAmount: string;
  movementNature: MovementNature;
  fitid: string;
  memo: string;
  payeeName: string;
  checkNumber: string;
  operationSubtype: string;
  timeZone: string;
};

type FilterValues = {
  from: string;
  to: string;
  reconciled: string;
  search: string;
};

function formatAmount(amount: number, nature: MovementNature) {
  const formatted = `R$ ${Math.abs(amount).toFixed(2)}`;
  return (
    <span style={{ color: nature === 'CREDIT' ? 'var(--success)' : 'var(--error)', fontFamily: 'var(--font-mono)', fontSize: 'var(--text-sm)' }}>
      {nature === 'CREDIT' ? '+' : '-'}{formatted}
    </span>
  );
}

export default function TransactionsPage() {
  const { data = [], isLoading, error } = useGetAllTransactions();
  const createMutation = useCreateTransaction();
  const updateMutation = useUpdateTransaction();
  const deleteMutation = useDeleteTransaction();
  const reconcileMutation = useReconcileTransaction();

  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<Transaction | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Transaction | null>(null);

  const [filters, setFilters] = useState<FilterValues>({ from: '', to: '', reconciled: 'all', search: '' });
  const filterForm = useForm<FilterValues>({ defaultValues: filters });

  const createForm = useForm<FormValues>({
    defaultValues: {
      statementId: '', categoryId: '', transactionType: 'DEBIT', postedDate: '',
      amount: '0', absoluteAmount: '0', movementNature: 'DEBIT',
      fitid: '', memo: '', payeeName: '', checkNumber: '', operationSubtype: '', timeZone: '',
    },
  });
  const editForm = useForm<FormValues>();

  function openEdit(tx: Transaction) {
    setEditTarget(tx);
    editForm.reset({
      statementId: tx.statementId,
      categoryId: tx.categoryId ?? '',
      transactionType: tx.transactionType,
      postedDate: tx.postedDate ? tx.postedDate.slice(0, 10) : '',
      amount: String(tx.amount),
      absoluteAmount: String(tx.absoluteAmount),
      movementNature: tx.movementNature,
      fitid: tx.fitid ?? '',
      memo: tx.memo ?? '',
      payeeName: tx.payeeName ?? '',
      checkNumber: tx.checkNumber ?? '',
      operationSubtype: tx.operationSubtype ?? '',
      timeZone: tx.timeZone ?? '',
    });
  }

  function buildDto(values: FormValues): CreateTransactionDto {
    return {
      statementId: values.statementId,
      categoryId: values.categoryId || undefined,
      transactionType: values.transactionType,
      postedDate: values.postedDate || undefined,
      timeZone: values.timeZone || undefined,
      amount: parseFloat(values.amount),
      fitid: values.fitid || undefined,
      checkNumber: values.checkNumber || undefined,
      memo: values.memo || undefined,
      absoluteAmount: parseFloat(values.absoluteAmount),
      movementNature: values.movementNature,
      payeeName: values.payeeName || undefined,
      operationSubtype: values.operationSubtype || undefined,
    };
  }

  function handleCreate(values: FormValues) {
    createMutation.mutate(buildDto(values), {
      onSuccess: () => { setCreateOpen(false); createForm.reset(); },
    });
  }

  function handleEdit(values: FormValues) {
    if (!editTarget) return;
    updateMutation.mutate(
      { id: editTarget.id, dto: buildDto(values) },
      { onSuccess: () => setEditTarget(null) },
    );
  }

  function handleDelete() {
    if (!deleteTarget) return;
    deleteMutation.mutate(deleteTarget.id, { onSuccess: () => setDeleteTarget(null) });
  }

  function handleReconcile(tx: Transaction) {
    reconcileMutation.mutate(tx.id);
  }

  function applyFilters(values: FilterValues) {
    setFilters(values);
  }

  function clearFilters() {
    const empty: FilterValues = { from: '', to: '', reconciled: 'all', search: '' };
    filterForm.reset(empty);
    setFilters(empty);
  }

  const filteredData = useMemo(() => {
    return data.filter(tx => {
      if (filters.from && tx.postedDate && tx.postedDate < filters.from) return false;
      if (filters.to && tx.postedDate && tx.postedDate > filters.to + 'T23:59:59') return false;
      if (filters.reconciled === 'reconciled' && !tx.isReconciled) return false;
      if (filters.reconciled === 'unreconciled' && tx.isReconciled) return false;
      if (filters.search) {
        const q = filters.search.toLowerCase();
        if (
          !tx.memo?.toLowerCase().includes(q) &&
          !tx.payeeName?.toLowerCase().includes(q) &&
          !tx.fitid?.toLowerCase().includes(q)
        ) return false;
      }
      return true;
    });
  }, [data, filters]);

  const columns = [
    {
      key: 'postedDate',
      header: 'Date',
      mono: true,
      width: '110px',
      render: (row: Transaction) => row.postedDate ? new Date(row.postedDate).toLocaleDateString() : (row.postedDateRaw ?? '—'),
    },
    { key: 'payeeName', header: 'Payee', render: (row: Transaction) => row.payeeName ?? row.memo ?? '—' },
    { key: 'memo', header: 'Memo', render: (row: Transaction) => row.memo ?? '—' },
    { key: 'transactionType', header: 'Type', mono: true, width: '80px' },
    {
      key: 'movementNature',
      header: 'Nature',
      width: '90px',
      render: (row: Transaction) => (
        <Badge variant={row.movementNature === 'CREDIT' ? 'positive' : 'negative'}>{row.movementNature}</Badge>
      ),
    },
    {
      key: 'amount',
      header: 'Amount',
      align: 'right' as const,
      width: '140px',
      render: (row: Transaction) => formatAmount(row.absoluteAmount, row.movementNature),
    },
    {
      key: 'isReconciled',
      header: 'Reconciled',
      width: '110px',
      render: (row: Transaction) => (
        <Badge variant={row.isReconciled ? 'positive' : 'warning'}>{row.isReconciled ? 'Yes' : 'No'}</Badge>
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '220px',
      render: (row: Transaction) => (
        <div style={{ display: 'flex', gap: '6px' }}>
          {!row.isReconciled && (
            <Button variant="secondary" size="sm" onClick={e => { e.stopPropagation(); handleReconcile(row); }} loading={reconcileMutation.isPending && reconcileMutation.variables === row.id}>
              Reconcile
            </Button>
          )}
          <Button variant="ghost" size="sm" onClick={e => { e.stopPropagation(); openEdit(row); }}>Edit</Button>
          <Button variant="danger" size="sm" onClick={e => { e.stopPropagation(); setDeleteTarget(row); }}>Delete</Button>
        </div>
      ),
    },
  ];

  const formFields = (form: ReturnType<typeof useForm<FormValues>>) => (
    <>
      <Input label="Statement ID" {...form.register('statementId', { required: 'Statement ID is required' })} error={form.formState.errors.statementId?.message} />
      <Input label="Category ID" {...form.register('categoryId')} hint="Optional" />
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <Input label="Transaction Type" {...form.register('transactionType', { required: 'Required' })} error={form.formState.errors.transactionType?.message} />
        <div>
          <label style={{ display: 'block', fontSize: 'var(--text-sm)', color: 'var(--text-secondary)', marginBottom: '6px' }}>Movement Nature</label>
          <select {...form.register('movementNature')} style={{ width: '100%', padding: '8px 12px', background: 'var(--bg-elevated)', border: '1px solid var(--border)', borderRadius: 'var(--radius-md)', color: 'var(--text-primary)', fontSize: 'var(--text-sm)' }}>
            <option value="DEBIT">DEBIT</option>
            <option value="CREDIT">CREDIT</option>
          </select>
        </div>
        <Input label="Amount" type="number" step="0.01" {...form.register('amount', { required: 'Required' })} error={form.formState.errors.amount?.message} />
        <Input label="Absolute Amount" type="number" step="0.01" {...form.register('absoluteAmount', { required: 'Required' })} error={form.formState.errors.absoluteAmount?.message} />
        <Input label="Posted Date" type="date" {...form.register('postedDate')} />
        <Input label="Time Zone" {...form.register('timeZone')} />
      </div>
      <Input label="FIT ID" {...form.register('fitid')} />
      <Input label="Payee Name" {...form.register('payeeName')} />
      <Input label="Memo" {...form.register('memo')} />
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <Input label="Check Number" {...form.register('checkNumber')} />
        <Input label="Operation Subtype" {...form.register('operationSubtype')} />
      </div>
    </>
  );

  if (isLoading) return <Spinner fullPage />;
  if (error) return <div style={{ color: 'var(--error)', padding: '24px' }}>Error: {(error as Error).message}</div>;

  return (
    <div>
      <PageHeader
        title="Transactions"
        subtitle="Financial transactions from bank statements"
        actions={<Button variant="primary" onClick={() => setCreateOpen(true)}>New Transaction</Button>}
      />

      {/* Filter Section */}
      <div style={{
        background: 'var(--bg-elevated)',
        border: '1px solid var(--border)',
        borderRadius: 'var(--radius-lg)',
        padding: '16px 20px',
        marginBottom: '20px',
      }}>
        <p style={{ fontSize: 'var(--text-xs)', fontFamily: 'var(--font-mono)', color: 'var(--text-muted)', letterSpacing: '0.08em', textTransform: 'uppercase', marginBottom: '12px' }}>
          Filters
        </p>
        <form onSubmit={filterForm.handleSubmit(applyFilters)}>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))', gap: '12px', alignItems: 'flex-end' }}>
            <Input label="From Date" type="date" {...filterForm.register('from')} />
            <Input label="To Date" type="date" {...filterForm.register('to')} />
            <div>
              <label style={{ display: 'block', fontSize: 'var(--text-sm)', color: 'var(--text-secondary)', marginBottom: '6px' }}>Reconciled</label>
              <select {...filterForm.register('reconciled')} style={{ width: '100%', padding: '8px 12px', background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 'var(--radius-md)', color: 'var(--text-primary)', fontSize: 'var(--text-sm)' }}>
                <option value="all">All</option>
                <option value="reconciled">Reconciled</option>
                <option value="unreconciled">Unreconciled</option>
              </select>
            </div>
            <Input label="Search" placeholder="Payee, memo, FITID..." {...filterForm.register('search')} />
            <div style={{ display: 'flex', gap: '8px' }}>
              <Button variant="primary" type="submit" size="sm">Apply</Button>
              <Button variant="ghost" type="button" size="sm" onClick={clearFilters}>Clear</Button>
            </div>
          </div>
        </form>

        {/* Summary row */}
        <div style={{ display: 'flex', gap: '24px', marginTop: '12px', paddingTop: '12px', borderTop: '1px solid var(--border)', fontSize: 'var(--text-sm)', color: 'var(--text-muted)', fontFamily: 'var(--font-mono)' }}>
          <span>{filteredData.length} records</span>
          <span style={{ color: 'var(--success)' }}>
            Credits: R$ {filteredData.filter(t => t.movementNature === 'CREDIT').reduce((s, t) => s + t.absoluteAmount, 0).toFixed(2)}
          </span>
          <span style={{ color: 'var(--error)' }}>
            Debits: R$ {filteredData.filter(t => t.movementNature === 'DEBIT').reduce((s, t) => s + t.absoluteAmount, 0).toFixed(2)}
          </span>
          <span>
            Unreconciled: {filteredData.filter(t => !t.isReconciled).length}
          </span>
        </div>
      </div>

      <Table
        columns={columns}
        data={filteredData}
        keyFn={row => row.id}
        loading={isLoading}
        emptyMessage="No transactions match the current filters."
      />

      {/* Create Modal */}
      <Modal open={createOpen} onClose={() => setCreateOpen(false)} title="NEW TRANSACTION" width={600}>
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
      <Modal open={!!editTarget} onClose={() => setEditTarget(null)} title="EDIT TRANSACTION" width={600}>
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
      <Modal open={!!deleteTarget} onClose={() => setDeleteTarget(null)} title="CONFIRM DELETE" width={420}>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '20px' }}>
          Delete transaction{' '}
          <strong style={{ color: 'var(--text-primary)' }}>
            {deleteTarget?.payeeName ?? deleteTarget?.memo ?? deleteTarget?.fitid ?? deleteTarget?.id}
          </strong>? This action cannot be undone.
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
