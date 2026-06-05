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
  useGetAllTransactionCategories,
  useCreateTransactionCategory,
  useUpdateTransactionCategory,
  useDeleteTransactionCategory,
} from '../hooks/useTransactionCategories';
import { TransactionCategory, CreateTransactionCategoryDto, AccountingNature } from '../types';

type FormValues = {
  code: string;
  description: string;
  operationType: string;
  accountingNature: AccountingNature;
  isActive: boolean;
};

const NATURE_OPTIONS: AccountingNature[] = ['REVENUE', 'EXPENSE', 'TRANSFER'];

export default function TransactionCategoriesPage() {
  const { data = [], isLoading, error } = useGetAllTransactionCategories();
  const createMutation = useCreateTransactionCategory();
  const updateMutation = useUpdateTransactionCategory();
  const deleteMutation = useDeleteTransactionCategory();

  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<TransactionCategory | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<TransactionCategory | null>(null);

  const createForm = useForm<FormValues>({
    defaultValues: { code: '', description: '', operationType: '', accountingNature: 'EXPENSE', isActive: true },
  });

  const editForm = useForm<FormValues>();

  function openEdit(cat: TransactionCategory) {
    setEditTarget(cat);
    editForm.reset({
      code: cat.code,
      description: cat.description,
      operationType: cat.operationType,
      accountingNature: cat.accountingNature,
      isActive: cat.isActive,
    });
  }

  function handleCreate(values: FormValues) {
    const dto: CreateTransactionCategoryDto = { ...values };
    createMutation.mutate(dto, {
      onSuccess: () => {
        setCreateOpen(false);
        createForm.reset();
      },
    });
  }

  function handleEdit(values: FormValues) {
    if (!editTarget) return;
    updateMutation.mutate(
      { id: editTarget.id, dto: values },
      { onSuccess: () => setEditTarget(null) },
    );
  }

  function handleDelete() {
    if (!deleteTarget) return;
    deleteMutation.mutate(deleteTarget.id, { onSuccess: () => setDeleteTarget(null) });
  }

  const columns = [
    { key: 'code', header: 'Code', mono: true, width: '120px' },
    { key: 'description', header: 'Description' },
    { key: 'operationType', header: 'Operation Type' },
    {
      key: 'accountingNature',
      header: 'Nature',
      render: (row: TransactionCategory) => (
        <Badge variant={row.accountingNature === 'REVENUE' ? 'positive' : row.accountingNature === 'EXPENSE' ? 'negative' : 'info'}>
          {row.accountingNature}
        </Badge>
      ),
    },
    {
      key: 'isActive',
      header: 'Status',
      render: (row: TransactionCategory) => (
        <Badge variant={row.isActive ? 'positive' : 'default'}>{row.isActive ? 'Active' : 'Inactive'}</Badge>
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '140px',
      render: (row: TransactionCategory) => (
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
        title="Transaction Categories"
        subtitle="Manage income, expense and transfer categories"
        actions={<Button variant="primary" onClick={() => setCreateOpen(true)}>New Category</Button>}
      />

      <Table
        columns={columns}
        data={data}
        keyFn={row => row.id}
        loading={isLoading}
        emptyMessage="No transaction categories found."
      />

      {/* Create Modal */}
      <Modal open={createOpen} onClose={() => setCreateOpen(false)} title="NEW CATEGORY">
        <form onSubmit={createForm.handleSubmit(handleCreate)} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <Input label="Code" {...createForm.register('code', { required: 'Code is required' })} error={createForm.formState.errors.code?.message} />
          <Input label="Description" {...createForm.register('description', { required: 'Description is required' })} error={createForm.formState.errors.description?.message} />
          <Input label="Operation Type" {...createForm.register('operationType', { required: 'Operation type is required' })} error={createForm.formState.errors.operationType?.message} />
          <div>
            <label style={{ display: 'block', fontSize: 'var(--text-sm)', color: 'var(--text-secondary)', marginBottom: '6px' }}>Accounting Nature</label>
            <select {...createForm.register('accountingNature')} style={{ width: '100%', padding: '8px 12px', background: 'var(--bg-elevated)', border: '1px solid var(--border)', borderRadius: 'var(--radius-md)', color: 'var(--text-primary)', fontSize: 'var(--text-sm)' }}>
              {NATURE_OPTIONS.map(n => <option key={n} value={n}>{n}</option>)}
            </select>
          </div>
          <label style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: 'var(--text-sm)', cursor: 'pointer' }}>
            <input type="checkbox" {...createForm.register('isActive')} defaultChecked />
            Active
          </label>
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
      <Modal open={!!editTarget} onClose={() => setEditTarget(null)} title="EDIT CATEGORY">
        <form onSubmit={editForm.handleSubmit(handleEdit)} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <Input label="Code" {...editForm.register('code', { required: 'Code is required' })} error={editForm.formState.errors.code?.message} />
          <Input label="Description" {...editForm.register('description', { required: 'Description is required' })} error={editForm.formState.errors.description?.message} />
          <Input label="Operation Type" {...editForm.register('operationType', { required: 'Operation type is required' })} error={editForm.formState.errors.operationType?.message} />
          <div>
            <label style={{ display: 'block', fontSize: 'var(--text-sm)', color: 'var(--text-secondary)', marginBottom: '6px' }}>Accounting Nature</label>
            <select {...editForm.register('accountingNature')} style={{ width: '100%', padding: '8px 12px', background: 'var(--bg-elevated)', border: '1px solid var(--border)', borderRadius: 'var(--radius-md)', color: 'var(--text-primary)', fontSize: 'var(--text-sm)' }}>
              {NATURE_OPTIONS.map(n => <option key={n} value={n}>{n}</option>)}
            </select>
          </div>
          <label style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: 'var(--text-sm)', cursor: 'pointer' }}>
            <input type="checkbox" {...editForm.register('isActive')} />
            Active
          </label>
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

      {/* Delete Confirm Modal */}
      <Modal open={!!deleteTarget} onClose={() => setDeleteTarget(null)} title="CONFIRM DELETE" width={400}>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '20px' }}>
          Delete category <strong style={{ color: 'var(--text-primary)' }}>{deleteTarget?.code}</strong>? This action cannot be undone.
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
