import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../../core/api/client';
import { PageHeader } from '../../../design-system/components/PageHeader';
import { Badge } from '../../../design-system/components/Badge';
import { Spinner } from '../../../design-system/components/Spinner';
import type { Transaction } from '../../transactions/types';
import type { Account } from '../../accounts/types';
import type { Statement } from '../../statements/types';

const fmt = (n: number) =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(n);

export default function DashboardPage() {
  const { data: transactions = [], isLoading: txLoading } = useQuery<Transaction[]>({
    queryKey: ['transactions'],
    queryFn: async () => (await apiClient.get('/transactions')).data,
  });

  const { data: accounts = [] } = useQuery<Account[]>({
    queryKey: ['accounts'],
    queryFn: async () => (await apiClient.get('/accounts')).data,
  });

  const { data: statements = [] } = useQuery<Statement[]>({
    queryKey: ['statements'],
    queryFn: async () => (await apiClient.get('/statements')).data,
  });

  const { data: unreconciled = [] } = useQuery<Transaction[]>({
    queryKey: ['transactions', 'unreconciled'],
    queryFn: async () => (await apiClient.get('/transactions/unreconciled')).data,
  });

  const totalCredits  = transactions.filter(t => t.movementNature === 'CREDIT').reduce((s, t) => s + t.absoluteAmount, 0);
  const totalDebits   = transactions.filter(t => t.movementNature === 'DEBIT').reduce((s, t) => s + t.absoluteAmount, 0);
  const netFlow       = totalCredits - totalDebits;
  const recent        = [...transactions].sort((a, b) => (b.postedDate ?? '').localeCompare(a.postedDate ?? '')).slice(0, 8);

  return (
    <div className="page-enter">
      <PageHeader
        title="Dashboard"
        subtitle="Overview of your Open Financial Exchange data"
      />

      {txLoading ? (
        <Spinner fullPage />
      ) : (
        <>
          {/* KPI grid */}
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))',
            gap: '16px',
            marginBottom: '32px',
          }}>
            <KpiCard label="Total Transactions" value={transactions.length.toString()} accent="info" />
            <KpiCard label="Total Credits"       value={fmt(totalCredits)}  accent="positive" />
            <KpiCard label="Total Debits"        value={fmt(totalDebits)}   accent="negative" />
            <KpiCard label="Net Flow"            value={fmt(netFlow)}       accent={netFlow >= 0 ? 'positive' : 'negative'} />
            <KpiCard label="Accounts"            value={accounts.length.toString()}   accent="default" />
            <KpiCard label="Statements"          value={statements.length.toString()} accent="default" />
            <KpiCard label="Unreconciled"        value={unreconciled.length.toString()} accent={unreconciled.length > 0 ? 'warning' : 'positive'} />
          </div>

          {/* Recent transactions */}
          <section>
            <div style={{
              display: 'flex', alignItems: 'center', justifyContent: 'space-between',
              marginBottom: '14px',
            }}>
              <h2 style={{ fontSize: 'var(--text-md)', fontWeight: 600 }}>Recent Transactions</h2>
              <span style={{ fontSize: 'var(--text-xs)', color: 'var(--text-muted)', fontFamily: 'var(--font-mono)' }}>
                Last {recent.length} entries
              </span>
            </div>

            <div style={{
              border: '1px solid var(--border)',
              borderRadius: 'var(--radius-lg)',
              overflow: 'hidden',
            }}>
              {recent.length === 0 ? (
                <div style={{ padding: '32px', textAlign: 'center', color: 'var(--text-muted)', fontFamily: 'var(--font-mono)', fontSize: 'var(--text-sm)' }}>
                  No transactions loaded.
                </div>
              ) : (
                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                  <thead>
                    <tr style={{ background: 'var(--bg-elevated)', borderBottom: '1px solid var(--border)' }}>
                      {['Date', 'Type', 'Memo', 'Payee', 'Amount', 'Status'].map(h => (
                        <th key={h} style={{
                          padding: '8px 14px', textAlign: 'left',
                          fontSize: 'var(--text-xs)', fontFamily: 'var(--font-mono)',
                          color: 'var(--text-muted)', letterSpacing: '0.08em', textTransform: 'uppercase',
                        }}>{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {recent.map(tx => (
                      <tr key={tx.id} style={{ borderBottom: '1px solid var(--border)' }}>
                        <td style={{ padding: '8px 14px', fontFamily: 'var(--font-mono)', fontSize: 'var(--text-sm)', color: 'var(--text-secondary)' }}>
                          {tx.postedDate}
                        </td>
                        <td style={{ padding: '8px 14px' }}>
                          <Badge variant={tx.movementNature === 'CREDIT' ? 'positive' : 'negative'}>
                            {tx.movementNature}
                          </Badge>
                        </td>
                        <td style={{ padding: '8px 14px', fontSize: 'var(--text-sm)', maxWidth: '240px' }}>
                          <span style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', display: 'block' }}>
                            {tx.memo ?? tx.operationSubtype ?? '—'}
                          </span>
                        </td>
                        <td style={{ padding: '8px 14px', fontSize: 'var(--text-sm)', color: 'var(--text-secondary)' }}>
                          {tx.payeeName ?? '—'}
                        </td>
                        <td style={{
                          padding: '8px 14px',
                          fontFamily: 'var(--font-mono)', fontSize: 'var(--text-sm)',
                          color: tx.movementNature === 'CREDIT' ? 'var(--positive)' : 'var(--negative)',
                          textAlign: 'right',
                        }}>
                          {tx.movementNature === 'CREDIT' ? '+' : '−'} {fmt(tx.absoluteAmount)}
                        </td>
                        <td style={{ padding: '8px 14px' }}>
                          <Badge variant={tx.isReconciled ? 'positive' : 'warning'}>
                            {tx.isReconciled ? 'Reconciled' : 'Pending'}
                          </Badge>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          </section>
        </>
      )}
    </div>
  );
}

interface KpiCardProps {
  label:  string;
  value:  string;
  accent: 'positive' | 'negative' | 'info' | 'warning' | 'default';
}

function KpiCard({ label, value, accent }: KpiCardProps) {
  const accentColor: Record<string, string> = {
    positive: 'var(--positive)', negative: 'var(--negative)',
    info: 'var(--info)', warning: 'var(--warning)', default: 'var(--text-muted)',
  };

  return (
    <div style={{
      background: 'var(--bg-surface)',
      border: '1px solid var(--border)',
      borderRadius: 'var(--radius-lg)',
      padding: '18px 20px',
      display: 'flex', flexDirection: 'column', gap: '6px',
    }}>
      <span style={{
        fontSize: 'var(--text-xs)',
        fontFamily: 'var(--font-mono)',
        color: 'var(--text-muted)',
        letterSpacing: '0.08em',
        textTransform: 'uppercase',
      }}>
        {label}
      </span>
      <span style={{
        fontSize: 'var(--text-2xl)',
        fontFamily: 'var(--font-mono)',
        fontWeight: 600,
        color: accentColor[accent] ?? 'var(--text-primary)',
        letterSpacing: '-0.02em',
      }}>
        {value}
      </span>
    </div>
  );
}
