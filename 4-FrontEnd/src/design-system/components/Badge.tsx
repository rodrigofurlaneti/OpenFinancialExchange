type BadgeVariant = 'default' | 'positive' | 'negative' | 'warning' | 'info' | 'accent';

interface BadgeProps {
  children: React.ReactNode;
  variant?: BadgeVariant;
}

const colors: Record<BadgeVariant, { bg: string; color: string }> = {
  default:  { bg: 'var(--bg-elevated)', color: 'var(--text-secondary)' },
  positive: { bg: 'var(--positive-dim)', color: 'var(--positive)' },
  negative:  { bg: 'var(--negative-dim)',  color: 'var(--negative)' },
  warning:  { bg: 'var(--warning-dim)', color: 'var(--warning)' },
  info:     { bg: 'var(--info-dim)',    color: 'var(--info)' },
  accent:   { bg: 'var(--accent-dim)',  color: 'var(--accent)' },
};

export const Badge = ({ children, variant = 'default' }: BadgeProps) => {
  const c = colors[variant];
  return (
    <span style={{
      display: 'inline-flex',
      alignItems: 'center',
      padding: '1px 7px',
      borderRadius: '2px',
      fontSize: 'var(--text-xs)',
      fontFamily: 'var(--font-mono)',
      fontWeight: 500,
      letterSpacing: '0.06em',
      textTransform: 'uppercase',
      background: c.bg,
      color: c.color,
      border: `1px solid ${c.color}22`,
    }}>
      {children}
    </span>
  );
};
