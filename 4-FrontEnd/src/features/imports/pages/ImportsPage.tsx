import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { PageHeader } from '../../../design-system/components/PageHeader';
import { Button } from '../../../design-system/components/Button';
import { Table } from '../../../design-system/components/Table';
import { Modal } from '../../../design-system/components/Modal';
import { Input } from '../../../design-system/components/Input';
import { Spinner } from '../../../design-system/components/Spinner';
import {
  useGetAllImports,
  useCreateImport,
  useUpdateImport,
  useDeleteImport,
} from '../hooks/useImports';
import { Import, CreateImportDto } from '../types';

type CreateFormValues = {
  fileName: string;
  importedAt: string;
  ofxVersion: string;
  ofxSecurity: string;
  ofxEncoding: string;
  ofxCharset: string;
  notes: string;
  importedBy: string;
};

type EditFormValues = {
  notes: string;
};

export default function ImportsPage() {
  const { data = [], isLoading, error } = useGetAllImports();
  const createMutation = useCreateImport();
  const updateMutation = useUpdateImport();
  const deleteMutation = useDeleteImport();

  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<Import | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Import | null>(null);
  const [detailTarget, setDetailTarget] = useState<Import | null>(null);

  const createForm = useForm<CreateFormValues>({
    defaultValues: { fileName: '', importedAt: new Date().toISOString().slice(0, 16), ofxVersion: '', ofxSecurity: '', ofxEncoding: '', ofxCharset: '', notes: '', importedBy: '' },
  });
  const editForm = useForm<EditFormValues>({ defaultValues: { notes: '' } });

  function openEdit(imp: Import) {
    setEditTarget(imp);
    editForm.reset({ notes: imp.notes ?? '' });
  }

  function handleCreate(values: CreateFormValues) {
    const dto: CreateImportDto = {
      fileName: values.fileName,
      importedAt: values.importedAt,
      ofxVersion: values.ofxVersion || undefined,
      ofxSecurity: values.ofxSecurity || undefined,
      ofxEncoding: values.ofxEncoding || undefined,
      ofxCharset: values.ofxCharset || undefined,
      notes: values.notes || undefined,
      importedBy: values.importedBy || undefined,
    };
    createMutation.mutate(dto, {
      onSuccess: () => { setCreateOpen(false); createForm.reset(); },
    });
  }

  function handleEdit(values: EditFormValues) {
    if (!editTarget) return;
    updateMutation.mutate(
      { id: editTarget.id, dto: { notes: values.notes || null } },
      { onSuccess: () => setEditTarget(null) },
    );
  }

  function handleDelete() {
    if (!deleteTarget) return;
    deleteMutation.mutate(deleteTarget.id, { onSuccess: () => setDeleteTarget(null) });
  }

  const columns = [
    { key: 'fileName', header: 'File Name', mono: true },
    {
      key: 'importedAt',
      header: 'Imported At',
      mono: true,
      width: '160px',
      render: (row: Import) => new Date(row.importedAt).toLocaleString(),
    },
    { key: 'ofxVersion', header: 'OFX Version', width: '110px', render: (row: Import) => row.ofxVersion ?? '—' },
    { key: 'importedBy', header: 'Imported By', render: (row: Import) => row.importedBy ?? '—' },
    { key: 'notes', header: 'Notes', render: (row: Import) => row.notes ?? '—' },
    {
      key: 'actions',
      header: '',
      width: '200px',
      render: (row: Import) => (
        <div style={{ display: 'flex', gap: '8px' }}>
          <Button variant="ghost" size="sm" onClick={e => { e.stopPropagation(); setDetailTarget(row); }}>View</Button>
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
        title="Imports"
        subtitle="OFX file import records"
        actions={<Button variant="primary" onClick={() => setCreateOpen(true)}>New Import</Button>}
      />

      <Table columns={columns} data={data} keyFn={row => row.id} loading={isLoading} emptyMessage="No imports found." />

      {/* Create Modal */}
      <Modal open={createOpen} onClose={() => setCreateOpen(false)} title="NEW IMPORT" width={560}>
        <form onSubmit={createForm.handleSubmit(handleCreate)} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <Input label="File Name" {...createForm.register('fileName', { required: 'File name is required' })} error={createForm.formState.errors.fileName?.message} />
          <Input label="Imported At" type="datetime-local" {...createForm.register('importedAt', { required: 'Import date is required' })} error={createForm.formState.errors.importedAt?.message} />
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
            <Input label="OFX Version" {...createForm.register('ofxVersion')} />
            <Input label="OFX Security" {...createForm.register('ofxSecurity')} />
            <Input label="OFX Encoding" {...createForm.register('ofxEncoding')} />
            <Input label="OFX Charset" {...createForm.register('ofxCharset')} />
          </div>
          <Input label="Imported By" {...createForm.register('importedBy')} />
          <Input label="Notes" {...createForm.register('notes')} />
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

      {/* Edit Modal — notes only */}
      <Modal open={!!editTarget} onClose={() => setEditTarget(null)} title="EDIT IMPORT NOTES" width={400}>
        <form onSubmit={editForm.handleSubmit(handleEdit)} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <p style={{ fontSize: 'var(--text-sm)', color: 'var(--text-secondary)' }}>
            Only the Notes field can be updated after import.
          </p>
          <Input label="Notes" {...editForm.register('notes')} />
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

      {/* Detail Modal */}
      <Modal open={!!detailTarget} onClose={() => setDetailTarget(null)} title="IMPORT DETAILS" width={600}>
        {detailTarget && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '12px', fontSize: 'var(--text-sm)' }}>
            {([
              ['File Name', detailTarget.fileName],
              ['Imported At', new Date(detailTarget.importedAt).toLocaleString()],
              ['OFX Version', detailTarget.ofxVersion],
              ['OFX Security', detailTarget.ofxSecurity],
              ['OFX Encoding', detailTarget.ofxEncoding],
              ['OFX Charset', detailTarget.ofxCharset],
              ['OFX Compression', detailTarget.ofxCompression],
              ['OFX Old File UID', detailTarget.ofxOldFileUID],
              ['OFX New File UID', detailTarget.ofxNewFileUID],
              ['Imported By', detailTarget.importedBy],
              ['Notes', detailTarget.notes],
              ['Created At', new Date(detailTarget.createdAt).toLocaleString()],
            ] as [string, string | null][]).map(([label, value]) => (
              <div key={label} style={{ display: 'flex', gap: '12px' }}>
                <span style={{ color: 'var(--text-muted)', fontFamily: 'var(--font-mono)', minWidth: '160px' }}>{label}</span>
                <span style={{ color: 'var(--text-primary)' }}>{value ?? '—'}</span>
              </div>
            ))}
            <div style={{ marginTop: '8px', display: 'flex', justifyContent: 'flex-end' }}>
              <Button variant="ghost" onClick={() => setDetailTarget(null)}>Close</Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Delete Modal */}
      <Modal open={!!deleteTarget} onClose={() => setDeleteTarget(null)} title="CONFIRM DELETE" width={400}>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '20px' }}>
          Delete import <strong style={{ color: 'var(--text-primary)' }}>{deleteTarget?.fileName}</strong>? This action cannot be undone.
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
