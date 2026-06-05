import { ReactNode } from 'react';

interface Column<T> {
  key:       string;
  header:    string;
  render?:   (row: T) => ReactNode;
  mono?:     boolean;
  align?:    'left' | 'right' | 'center';
  width?:    string;
}

interface TableProps<T> {
  columns: Column<T>[];
  data:    T[];
  keyFn:   (row: T) => string | number;
  loading?: boolean;
  emptyMessage?: string;
  onRowClick?: (row: T) => void;
}

export const Table = <T,>({
  columns, data, keyFn, loading, emptyMessage = 'No records found.', onRowClick,
}: TableProps<T>) => (
  <div style={{
    border: '1px solid var(--border)',
    borderRadius: 'var(--radius-lg)',
    overflow: 'hidden',
  }}>
    <table style={{ width: '100%', borderCollapse: 'collapse' }}>
      <thead>
        <tr style={{ background: 'var(--bg-elevated)', borderBottom: '1px solid var(--border)' }}>
          {columns.map(col => (
            <th
              key={col.key}
              style={{
                padding: '8px 14px',
                fontSize: 'var(--text-xs)',
                fontFamily: 'var(--font-mono)',
                fontWeight: 500,
                color: 'var(--text-muted)',
                letterSpacing: '0.08em',
                textTransform: 'uppercase',
                textAlign: col.align ?? 'left',
                width: col.width,
                whiteSpace: 'nowrap',
              }}
            >
              {col.header}
            </th>
          ))}
        </tr>
      </thead>
      <tbody>
        {loading ? (
          <tr>
            <td colSpan={columns.length} style={{ padding: '32px', textAlign: 'center' }}>
              <LoadingRows />
            </td>
          </tr>
        ) : data.length === 0 ? (
          <tr>
            <td colSpan={columns.length} style={{
              padding: '40px',
              textAlign: 'center',
              color: 'var(--text-muted)',
              fontFamily: 'var(--font-mono)',
              fontSize: 'var(--text-sm)',
            }}>
              {emptyMessage}
            </td>
          </tr>
        ) : (
          data.map(row => (
            <tr
              key={keyFn(row)}
              onClick={onRowClick ? () => onRowClick(row) : undefined}
              style={{
                borderBottom: '1px solid var(--border)',
                cursor: onRowClick ? 'pointer' : 'default',
                transition: 'background var(--duration-fast)',
              }}
              onMouseEnter={e => { if (onRowClick) (e.currentTarget as HTMLElement).style.background = 'var(--bg-elevated)'; }}
              onMouseLeave={e => { (e.currentTarget as HTMLElement).style.background = 'transparent'; }}
            >
              {columns.map(col => (
                <td
                  key={col.key}
                  style={{
                    padding: '8px 14px',
                    fontSize: col.mono ? 'var(--text-sm)' : 'var(--text-base)',
                    fontFamily: col.mono ? 'var(--font-mono)' : 'var(--font-ui)',
                    color: 'var(--text-primary)',
                    textAlign: col.align ?? 'left',
                    verticalAlign: 'middle',
                  }}
                >
                  {col.render ? col.render(row) : String((row as Record<string, unknown>)[col.key] ?? '—')}
                </td>
              ))}
            </tr>
          ))
        )}
      </tbody>
    </table>
  </div>
);

const LoadingRows = () => (
  <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', padding: '8px' }}>
    {[80, 95, 70].map((w, i) => (
      <div key={i} style={{
        height: '12px',
        width: `${w}%`,
        background: 'var(--bg-elevated)',
        borderRadius: '2px',
        animation: `pulse 1.5s ${i * 0.2}s ease-in-out infinite`,
      }} />
    ))}
  </div>
);
