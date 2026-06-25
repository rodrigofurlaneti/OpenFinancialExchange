export type Period = 'day' | 'week' | 'month' | 'bimonth' | 'quarter' | 'semester' | 'year'

export interface PeriodOption {
  key: Period
  label: string
}

export const PERIODS: PeriodOption[] = [
  { key: 'day', label: 'Dia' },
  { key: 'week', label: 'Semana' },
  { key: 'month', label: 'Mês' },
  { key: 'bimonth', label: 'Bimestre' },
  { key: 'quarter', label: 'Trimestre' },
  { key: 'semester', label: 'Semestre' },
  { key: 'year', label: 'Ano' },
]

export interface PeriodRange {
  from: Date
  to: Date
  label: string
}

const startOfDay = (d: Date) => {
  const x = new Date(d)
  x.setHours(0, 0, 0, 0)
  return x
}
const endOfDay = (d: Date) => {
  const x = new Date(d)
  x.setHours(23, 59, 59, 999)
  return x
}
const monthsRange = (startYear: number, startMonth: number, lengthMonths: number) => ({
  from: startOfDay(new Date(startYear, startMonth, 1)),
  to: endOfDay(new Date(startYear, startMonth + lengthMonths, 0)), // day 0 of next month = last day
})
const cap = (s: string) => s.charAt(0).toUpperCase() + s.slice(1)
const monShort = (yy: number, mm: number) =>
  new Date(yy, mm, 1).toLocaleDateString('pt-BR', { month: 'short' }).replace('.', '')

/**
 * Calendar-aligned range for the given granularity.
 * offset = 0 is the current period, -1 the previous one, etc.
 */
export function getPeriodRange(period: Period, offset: number): PeriodRange {
  const now = new Date()
  const y = now.getFullYear()
  const m = now.getMonth()

  switch (period) {
    case 'day': {
      const d = new Date(y, m, now.getDate() + offset)
      return {
        from: startOfDay(d),
        to: endOfDay(d),
        label: cap(d.toLocaleDateString('pt-BR', { weekday: 'short', day: '2-digit', month: 'short' })),
      }
    }
    case 'week': {
      const base = new Date(y, m, now.getDate() + offset * 7)
      const dow = (base.getDay() + 6) % 7 // Monday = 0
      const mon = new Date(base)
      mon.setDate(base.getDate() - dow)
      const sun = new Date(mon)
      sun.setDate(mon.getDate() + 6)
      const from = startOfDay(mon)
      const to = endOfDay(sun)
      const f = from.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })
      const t = to.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })
      return { from, to, label: `${f} – ${t}` }
    }
    case 'month': {
      const total = y * 12 + m + offset
      const yy = Math.floor(total / 12)
      const mm = ((total % 12) + 12) % 12
      return {
        ...monthsRange(yy, mm, 1),
        label: cap(new Date(yy, mm, 1).toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' })),
      }
    }
    case 'bimonth': {
      const idx = y * 6 + Math.floor(m / 2) + offset
      const yy = Math.floor(idx / 6)
      const bi = ((idx % 6) + 6) % 6
      const sm = bi * 2
      return { ...monthsRange(yy, sm, 2), label: `${monShort(yy, sm)}–${monShort(yy, sm + 1)} ${yy}` }
    }
    case 'quarter': {
      const idx = y * 4 + Math.floor(m / 3) + offset
      const yy = Math.floor(idx / 4)
      const q = ((idx % 4) + 4) % 4
      return { ...monthsRange(yy, q * 3, 3), label: `T${q + 1} ${yy}` }
    }
    case 'semester': {
      const idx = y * 2 + Math.floor(m / 6) + offset
      const yy = Math.floor(idx / 2)
      const s = ((idx % 2) + 2) % 2
      return { ...monthsRange(yy, s * 6, 6), label: `S${s + 1} ${yy}` }
    }
    case 'year': {
      const yy = y + offset
      return { ...monthsRange(yy, 0, 12), label: `${yy}` }
    }
  }
}

/** Local wall-clock ISO (no timezone) so the API matches OFX DtPosted semantics. */
export function toLocalIso(d: Date): string {
  const p = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}:${p(d.getSeconds())}`
}
