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
  useGetAllAccounts,
  useCreateAccount,
  useUpdateAccount,
  useDeleteAccount,
} from '../hooks/useAccounts';
import { Account, AccountType, CreateAccountDto } from '../types';

type FormValues = {
  importId: string;
  bankId: string;
  branchNumber: string;
  accountNumber: string;
  accountType: AccountType;
  defaultCurrency: string;
};

const ACCOUNT_TYPES: AccountType[] = ['CHECKING', 'SAVINGS', 'MONEYMRKT', 'CREDITLINE'];

function accountTypeBadge(type: AccountType) {
  const map: Record<AccountType, 'info' | 'positive' | 'accent' | 'warning'> = {
    CHECKING: 'info',
    SAVINGS: 'positive',
    MONEYMRKT: 'accent',
    CREDITLINE: 'warning',
  };
  return map[type];
}

export default function AccountsPage() {
  const { data = [], isLoading, error } = useGetAllAccounts();
  const createMutation = useCreateAccount();
  const updateMutation = useUpdateAccount();
  const deleteMutation = useDeleteAccount();

  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<Account | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Account | null>(null);

  const createForm = useForm<FormValues>({
    defaultValues: { importId: '', bankId: '', branchNumber: '', accountNumber: '', accountType: 'CHECKING', defaultCurrency: 'BRL' },
  });
  const editForm = useForm<FormValues>();

  function openEdit(account: Account) {
    setEditTarget(account);
    editForm.reset({
      importId: account.importId,
      bankId: account.bankId,
      branchNumber: account.branchNumber ?? '',
      accountNumber: account.accountNumber,
      accountType: account.accountType,
      defaultCurrency: account.defaultCurrency,
    });
  }

  function handleCreate(values: FormValues) {
    const dto: CreateAccountDto = {
      importId: values.importId,
      bankId: values.bankId,
      branchNumber: values.branchNumber || undefined,
      accountNumber: values.accountNumber,
      accountType: values.accountType,
      defaultCurrency: values.defaultCurrency,
    };
    createMutation.mutate(dto, {
      onSuccess: () => { setCreateOpen(false); createForm.reset(); },
    });
  }

  function handleEdit(values: FormValues) {
    if (!editTarget) return;
    updateMutation.mutate(
      { id: editTarget.id, dto: { ...values, branchNumber: values.branchNumber || undefined } },
      { onSuccess: () => setEditTarget(null) },
    );
  }

  function handleDelete() {
    if (!deleteTarget) return;
    deleteMutation.mutate(deleteTarget.id, { onSuccess: () => setDeleteTarget(null) });
  }

  const columns = [
    { key: 'accountNumber', header: 'Account Number', mono: true },
    { key: 'branchNumber', header: 'Branch', mono: true, width: '100px', render: (row: Account) => row.branchNumber ?? '—' },
    {
      key: 'accountType',
      header: 'Type',
      width: '130px',
      render: (row: Account) => <Badge variant={accountTypeBadge(row.accountType)}>{row.accountType}</Badge>,
    },
    { key: 'defaultCurrency', header: 'Currency', mono: true, width: '100px' },
    { key: 'bankId', header: 'Bank ID', mono: true },
    { key: 'importId', header: 'Import ID', mono: true },
    {
      key: 'createdAt',
      header: 'Created',
      mono: true,
      width: '120px',
      render: (row: Account) => new Date(row.createdAt).toLocaleDateString(),
    },
    {
      key: 'actions',
      header: '',
      width: '140px',
      render: (row: Account) => (
        <div style={{ display: 'flex', gap: '8px' }}>
          <Button variant="ghost" size="sm" onClick={e => { e.stopPropagation(); openEdit(row); }}>Edit</Button>
          <Button variant="danger" size="sm" onClick={e => { e.stopPropagation(); setDeleteTarget(row); }}>Delete</Button>
        </div>
      ),
    },
  ];

  const formFields = (form: ReturnType<typeof useForm<FormValues>>) => (
    <>
      <Input label="Import ID" {...form.register('importId', { required: 'Import ID is required' })} error={form.formState.errors.importId?.message} />
      <Input label="Bank ID" {...form.register('bankId', { required: 'Bank ID is required' })} error={form.formState.errors.bankId?.message} />
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <Input label="Branch Number" {...form.register('branchNumber')} />
        <Input label="Account Number" {...form.register('accountNumber', { required: 'Account number is required' })} error={form.formState.errors.accountNumber?.message} />
      </div>
      <div>
        <label style={{ display: 'block', fontSize: 'var(--text-sm)', color: 'var(--text-secondary)', marginBottom: '6px' }}>Account Type</label>
        <select {...form.register('accountType')} style={{ width: '100%', padding: '8px 12px', background: 'var(--bg-elevated)', border: '1px solid var(--border)', borderRadius: 'var(--radius-md)', color: 'var(--text-primary)', fontSize: 'var(--text-sm)' }}>
          {ACCOUNT_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
        </select>
      </div>
      <Input label="Default Currency" {...form.register('defaultCurrency', { required: 'Currency is required' })} error={form.formState.errors.defaultCurrency?.message} />
    </>
  );

  if (isLoading) return <Spinner fullPage />;
  if (error) return <div style={{ color: 'var(--error)', padding: '24px' }}>Error: {(error as Error).message}</div>;

  return (
    <div>
      <PageHeader
        title="Accounts"
        subtitle="Bank accounts from OFX imports"
        actions={<Button variant="primary" onClick={() => setCreateOpen(true)}>New Account</Button>}
      />

      <Table columns={columns} data={data} keyFn={row => row.id} loading={isLoading} emptyMessage="No accounts found." />

      {/* Create Modal */}
      <Modal open={createOpen} onClose={() => setCreateOpen(false)} title="NEW ACCOUNT" width={540}>
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
      <Modal open={!!editTarget} onClose={() => setEditTarget(null)} title="EDIT ACCOUNT" width={540}>
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
          Delete account <strong style={{ color: 'var(--text-primary)' }}>{deleteTarget?.accountNumber}</strong>? This action cannot be undone.
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
