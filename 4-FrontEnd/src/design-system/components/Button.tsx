import { ButtonHTMLAttributes, forwardRef } from 'react';

type Variant = 'primary' | 'secondary' | 'ghost' | 'danger';
type Size    = 'sm' | 'md' | 'lg';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  size?:    Size;
  loading?: boolean;
}

const styles: Record<string, string> = {
  base: `
    display: inline-flex; align-items: center; justify-content: center; gap: 6px;
    border-radius: var(--radius-md); font-family: var(--font-ui); font-weight: 500;
    cursor: pointer; transition: all var(--duration-fast) var(--ease-out);
    border: 1px solid transparent; white-space: nowrap; letter-spacing: 0.01em;
  `,
};

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'secondary', size = 'md', loading, children, style, ...props }, ref) => {
    const variantStyle = {
      primary: {
        background: 'var(--accent)',
        color: 'var(--text-inverse)',
        borderColor: 'var(--accent)',
      },
      secondary: {
        background: 'var(--bg-elevated)',
        color: 'var(--text-primary)',
        borderColor: 'var(--border)',
      },
      ghost: {
        background: 'transparent',
        color: 'var(--text-secondary)',
        borderColor: 'transparent',
      },
      danger: {
        background: 'var(--negative-dim)',
        color: 'var(--negative)',
        borderColor: 'var(--negative)',
      },
    }[variant];

    const sizeStyle = {
      sm: { padding: '3px 10px', fontSize: 'var(--text-sm)', height: '26px' },
      md: { padding: '5px 14px', fontSize: 'var(--text-base)', height: '32px' },
      lg: { padding: '8px 20px', fontSize: 'var(--text-md)', height: '38px' },
    }[size];

    return (
      <button
        ref={ref}
        style={{ ...variantStyle, ...sizeStyle, ...style }}
        disabled={loading || props.disabled}
        {...props}
      >
        {loading ? <Spinner size={14} /> : children}
      </button>
    );
  }
);
Button.displayName = 'Button';

const Spinner = ({ size = 16 }: { size?: number }) => (
  <svg
    width={size} height={size} viewBox="0 0 24 24" fill="none"
    style={{ animation: 'spin 0.8s linear infinite' }}
  >
    <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
    <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="2" strokeOpacity="0.25" />
    <path d="M12 2a10 10 0 0 1 10 10" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
  </svg>
);
