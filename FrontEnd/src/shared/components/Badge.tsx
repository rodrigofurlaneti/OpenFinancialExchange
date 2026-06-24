interface BadgeProps {
  children: string
  variant?: 'green' | 'red' | 'blue' | 'yellow' | 'slate'
}

const variants = {
  green:  'bg-emerald-500/15 text-emerald-400 ring-emerald-500/30',
  red:    'bg-red-500/15 text-red-400 ring-red-500/30',
  blue:   'bg-blue-500/15 text-blue-400 ring-blue-500/30',
  yellow: 'bg-yellow-500/15 text-yellow-400 ring-yellow-500/30',
  slate:  'bg-slate-500/15 text-slate-400 ring-slate-500/30',
}

export function Badge({ children, variant = 'slate' }: BadgeProps) {
  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset ${variants[variant]}`}>
      {children}
    </span>
  )
}
