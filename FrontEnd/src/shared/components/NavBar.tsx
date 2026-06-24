import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuthStore } from '../../core/auth/authStore'
import { Building2, CreditCard, FileUp, LogOut, BarChart3 } from 'lucide-react'

const NAV_ITEMS = [
  { to: '/institutions',  label: 'Instituições',  icon: Building2  },
  { to: '/bank-accounts', label: 'Contas',        icon: CreditCard },
  { to: '/ofx',          label: 'OFX',            icon: FileUp     },
]

export function NavBar() {
  const { pathname } = useLocation()
  const navigate = useNavigate()
  const logout = useAuthStore((s) => s.logout)

  function handleLogout() {
    logout()
    navigate('/login')
  }

  return (
    <nav className="fixed top-0 left-0 right-0 z-40 glass-card border-x-0 border-t-0 rounded-none px-4 h-16 flex items-center">
      <div className="max-w-7xl mx-auto w-full flex items-center justify-between">
        {/* Logo */}
        <Link to="/" className="flex items-center gap-2 font-bold text-emerald-400 hover:text-emerald-300 transition-colors">
          <BarChart3 size={22} />
          <span className="hidden sm:block">OpenFinancialExchange</span>
          <span className="sm:hidden">OFX</span>
        </Link>

        {/* Nav links */}
        <div className="flex items-center gap-1">
          {NAV_ITEMS.map(({ to, label, icon: Icon }) => {
            const active = pathname.startsWith(to)
            return (
              <Link
                key={to}
                to={to}
                className={`flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition-all duration-200 ${
                  active
                    ? 'bg-emerald-600/20 text-emerald-400'
                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-700/50'
                }`}
              >
                <Icon size={16} />
                <span className="hidden md:block">{label}</span>
              </Link>
            )
          })}
        </div>

        {/* Logout */}
        <button
          onClick={handleLogout}
          aria-label="Sair"
          className="flex items-center gap-2 px-3 py-2 rounded-lg text-sm text-slate-400 hover:text-red-400 hover:bg-red-500/10 transition-all duration-200"
        >
          <LogOut size={16} />
          <span className="hidden md:block">Sair</span>
        </button>
      </div>
    </nav>
  )
}
