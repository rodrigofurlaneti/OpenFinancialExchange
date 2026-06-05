import { NavLink, Outlet } from 'react-router-dom';

const NAV_ITEMS = [
  { to: '/',                    label: 'Dashboard',            icon: '▦' },
  { to: '/transactions',        label: 'Transactions',         icon: '⇄' },
  { to: '/accounts',            label: 'Accounts',             icon: '◻' },
  { to: '/statements',          label: 'Statements',           icon: '≡' },
  { to: '/banks',               label: 'Banks',                icon: '⬡' },
  { to: '/imports',             label: 'Imports',              icon: '↓' },
  { to: '/ledger-balances',     label: 'Ledger Balances',      icon: '⊞' },
  { to: '/signon-sessions',     label: 'Signon Sessions',      icon: '⌁' },
  { to: '/transaction-categories', label: 'Categories',        icon: '⊕' },
];

export const Layout = () => (
  <>
    {/* Top bar */}
    <header style={{
      gridColumn: '1 / -1',
      gridRow: '1',
      display: 'flex',
      alignItems: 'center',
      padding: '0 20px',
      background: 'var(--bg-void)',
      borderBottom: '1px solid var(--border)',
      gap: '16px',
      zIndex: 100,
    }}>
      <div style={{
        fontFamily: 'var(--font-mono)',
        fontWeight: 700,
        fontSize: 'var(--text-md)',
        color: 'var(--accent)',
        letterSpacing: '0.06em',
        display: 'flex',
        alignItems: 'center',
        gap: '8px',
      }}>
        <span style={{ opacity: 0.5 }}>◆</span>
        OFX
        <span style={{ color: 'var(--text-muted)', fontWeight: 300 }}>|</span>
        <span style={{ color: 'var(--text-secondary)', fontSize: 'var(--text-sm)', fontWeight: 400 }}>
          Financial Exchange
        </span>
      </div>
      <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', gap: '12px' }}>
        <StatusDot />
        <span style={{ fontFamily: 'var(--font-mono)', fontSize: 'var(--text-xs)', color: 'var(--text-muted)' }}>
          API: localhost:5000
        </span>
      </div>
    </header>

    {/* Sidebar */}
    <aside style={{
      gridRow: '2',
      background: 'var(--bg-void)',
      borderRight: '1px solid var(--border)',
      padding: '16px 0',
      overflowY: 'auto',
    }}>
      <nav>
        {NAV_ITEMS.map(({ to, label, icon }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/'}
            style={({ isActive }) => ({
              display: 'flex',
              alignItems: 'center',
              gap: '10px',
              padding: '7px 16px',
              fontSize: 'var(--text-sm)',
              fontFamily: 'var(--font-ui)',
              color: isActive ? 'var(--accent)' : 'var(--text-secondary)',
              background: isActive ? 'var(--accent-dim)' : 'transparent',
              borderLeft: `2px solid ${isActive ? 'var(--accent)' : 'transparent'}`,
              transition: 'all var(--duration-fast)',
              textDecoration: 'none',
            })}
            onMouseEnter={e => {
              const el = e.currentTarget as HTMLElement;
              if (!el.style.background.includes('accent-dim'))
                el.style.background = 'var(--bg-elevated)';
            }}
            onMouseLeave={e => {
              const el = e.currentTarget as HTMLElement;
              if (!el.style.background.includes('accent-dim'))
                el.style.background = 'transparent';
            }}
          >
            <span style={{
              fontFamily: 'var(--font-mono)',
              fontSize: '12px',
              width: '16px',
              textAlign: 'center',
              opacity: 0.7,
            }}>
              {icon}
            </span>
            {label}
          </NavLink>
        ))}
      </nav>
    </aside>

    {/* Main content */}
    <main style={{
      gridRow: '2',
      overflowY: 'auto',
      background: 'var(--bg-base)',
      padding: '28px 32px',
    }}>
      <Outlet />
    </main>
  </>
);

const StatusDot = () => (
  <div style={{
    width: '7px', height: '7px',
    borderRadius: '50%',
    background: 'var(--positive)',
    boxShadow: '0 0 6px var(--positive)',
    animation: 'pulse 2s ease-in-out infinite',
  }} />
);
