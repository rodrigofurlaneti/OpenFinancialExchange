import { ReactNode } from 'react';

interface PageHeaderProps {
  title:    string;
  subtitle?: string;
  actions?: ReactNode;
}

export const PageHeader = ({ title, subtitle, actions }: PageHeaderProps) => (
  <div style={{
    display: 'flex',
    alignItems: 'flex-start',
    justifyContent: 'space-between',
    marginBottom: 'var(--sp-6)',
    paddingBottom: 'var(--sp-5)',
    borderBottom: '1px solid var(--border)',
  }}>
    <div>
      <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '3px' }}>
        <span style={{
          fontFamily: 'var(--font-mono)',
          fontSize: 'var(--text-xs)',
          color: 'var(--accent)',
          letterSpacing: '0.1em',
          textTransform: 'uppercase',
        }}>
          ◆
        </span>
        <h1 style={{ fontSize: 'var(--text-xl)', fontWeight: 600, letterSpacing: '-0.01em' }}>
          {title}
        </h1>
      </div>
      {subtitle && (
        <p style={{ fontSize: 'var(--text-sm)', color: 'var(--text-secondary)', marginLeft: '20px' }}>
          {subtitle}
        </p>
      )}
    </div>
    {actions && <div style={{ display: 'flex', gap: 'var(--sp-2)' }}>{actions}</div>}
  </div>
);
