import { InputHTMLAttributes, forwardRef } from 'react';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  hint?:  string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, hint, style, ...props }, ref) => (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
      {label && (
        <label style={{
          fontSize: 'var(--text-sm)',
          color: 'var(--text-secondary)',
          fontFamily: 'var(--font-mono)',
          letterSpacing: '0.05em',
          textTransform: 'uppercase',
        }}>
          {label}
        </label>
      )}
      <input
        ref={ref}
        style={{
          background: 'var(--bg-elevated)',
          border: `1px solid ${error ? 'var(--negative)' : 'var(--border)'}`,
          borderRadius: 'var(--radius-md)',
          color: 'var(--text-primary)',
          fontFamily: 'var(--font-mono)',
          fontSize: 'var(--text-sm)',
          height: '32px',
          padding: '0 10px',
          outline: 'none',
          transition: 'border-color var(--duration-fast)',
          width: '100%',
          ...style,
        }}
        onFocus={e => { e.currentTarget.style.borderColor = 'var(--border-focus)'; }}
        onBlur={e  => { e.currentTarget.style.borderColor = error ? 'var(--negative)' : 'var(--border)'; }}
        {...props}
      />
      {error && <span style={{ fontSize: 'var(--text-xs)', color: 'var(--negative)' }}>{error}</span>}
      {hint && !error && <span style={{ fontSize: 'var(--text-xs)', color: 'var(--text-muted)' }}>{hint}</span>}
    </div>
  )
);
Input.displayName = 'Input';
