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
  useGetAllBanks,
  useCreateBank,
  useUpdateBank,
  useDeleteBank,
} from '../hooks/useBanks';
import { Bank, CreateBankDto } from '../types';

type FormValues = {
  compeCode: string;
  bankName: string;
  ispb: string;
  isActive: boolean;
};

export default function BanksPage() {
  const { data = [], isLoading, error } = useGetAllBanks();
  const createMutation = useCreateBank();
  const updateMutation = useUpdateBank();
  const deleteMutation = useDeleteBank();

  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<Bank | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Bank | null>(null);

  const createForm = useForm<FormValues>({
    defaultValues: { compeCode: '', bankName: '', ispb: '', isActive: true },
  });
  const editForm = useForm<FormValues>();

  function openEdit(bank: Bank) {
    setEditTarget(bank);
    editForm.reset({
      compeCode: bank.compeCode,
      bankName: bank.bankName,
      ispb: bank.ispb,
      isActive: bank.isActive,
    });
  }

  function handleCreate(values: FormValues) {
    const dto: CreateBankDto = { ...values };
    createMutation.mutate(dto, {
      onSuccess: () => { setCreateOpen(false); createForm.reset(); },
    });
  }

  function handleEdit(values: FormValues) {
    if (!editTarget) return;
    updateMutation.mutate({ id: editTarget.id, dto: values }, { onSuccess: () => setEditTarget(null) });
  }

  function handleDelete() {
    if (!deleteTarget) return;
    deleteMutation.mutate(deleteTarget.id, { onSuccess: () => setDeleteTarget(null) });
  }

  const columns = [
    { key: 'compeCode', header: 'COMPE', mono: true, width: '100px' },
    { key: 'bankName', header: 'Bank Name' },
    { key: 'ispb', header: 'ISPB', mono: true, width: '120px' },
    {
      key: 'isActive',
      header: 'Status',
      width: '100px',
      render: (row: Bank) => (
        <Badge variant={row.isActive ? 'positive' : 'default'}>{row.isActive ? 'Active' : 'Inactive'}</Badge>
      ),
    },
    {
      key: 'createdAt',
      header: 'Created',
      mono: true,
      width: '120px',
      render: (row: Bank) => new Date(row.createdAt).toLocaleDateString(),
    },
    {
      key: 'actions',
      header: '',
      width: '140px',
      render: (row: Bank) => (
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
        title="Banks"
        subtitle="Manage financial institutions"
        actions={<Button variant="primary" onClick={() => setCreateOpen(true)}>New Bank</Button>}
      />

      <Table columns={columns} data={data} keyFn={row => row.id} loading={isLoading} emptyMessage="No banks found." />

      {/* Create Modal */}
      <Modal open={createOpen} onClose={() => setCreateOpen(false)} title="NEW BANK">
        <form onSubmit={createForm.handleSubmit(handleCreate)} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <Input label="COMPE Code" {...createForm.register('compeCode', { required: 'COMPE code is required' })} error={createForm.formState.errors.compeCode?.message} />
          <Input label="Bank Name" {...createForm.register('bankName', { required: 'Bank name is required' })} error={createForm.formState.errors.bankName?.message} />
          <Input label="ISPB" {...createForm.register('ispb', { required: 'ISPB is required' })} error={createForm.formState.errors.ispb?.message} />
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
      <Modal open={!!editTarget} onClose={() => setEditTarget(null)} title="EDIT BANK">
        <form onSubmit={editForm.handleSubmit(handleEdit)} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <Input label="COMPE Code" {...editForm.register('compeCode', { required: 'COMPE code is required' })} error={editForm.formState.errors.compeCode?.message} />
          <Input label="Bank Name" {...editForm.register('bankName', { required: 'Bank name is required' })} error={editForm.formState.errors.bankName?.message} />
          <Input label="ISPB" {...editForm.register('ispb', { required: 'ISPB is required' })} error={editForm.formState.errors.ispb?.message} />
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

      {/* Delete Modal */}
      <Modal open={!!deleteTarget} onClose={() => setDeleteTarget(null)} title="CONFIRM DELETE" width={400}>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '20px' }}>
          Delete bank <strong style={{ color: 'var(--text-primary)' }}>{deleteTarget?.bankName}</strong>? This action cannot be undone.
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
