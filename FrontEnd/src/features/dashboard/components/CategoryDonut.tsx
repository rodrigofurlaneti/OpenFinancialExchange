export interface DonutSegment {
  name: string
  value: number
  color: string
}

interface CategoryDonutProps {
  segments: DonutSegment[]
  /** Big number rendered in the center (already formatted). */
  centerValue: string
  centerLabel: string
  size?: number
  thickness?: number
}

export function CategoryDonut({
  segments,
  centerValue,
  centerLabel,
  size = 200,
  thickness = 22,
}: CategoryDonutProps) {
  const r = (size - thickness) / 2
  const cx = size / 2
  const cy = size / 2
  const circumference = 2 * Math.PI * r
  const total = segments.reduce((s, x) => s + x.value, 0)

  let acc = 0

  return (
    <div className="relative" style={{ width: size, height: size }}>
      <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} className="-rotate-90">
        {/* Track */}
        <circle
          cx={cx}
          cy={cy}
          r={r}
          fill="none"
          stroke="rgb(51 65 85 / 0.5)"
          strokeWidth={thickness}
        />
        {total > 0 &&
          segments.map((seg, i) => {
            const len = (seg.value / total) * circumference
            const dash = `${len} ${circumference - len}`
            const offset = -acc
            acc += len
            return (
              <circle
                key={i}
                cx={cx}
                cy={cy}
                r={r}
                fill="none"
                stroke={seg.color}
                strokeWidth={thickness}
                strokeDasharray={dash}
                strokeDashoffset={offset}
                strokeLinecap="butt"
              >
                <title>{`${seg.name}: ${seg.value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}`}</title>
              </circle>
            )
          })}
      </svg>
      <div className="absolute inset-0 flex flex-col items-center justify-center text-center px-4">
        <span className="text-[10px] uppercase tracking-wider text-slate-500">{centerLabel}</span>
        <span className="text-xl font-bold text-slate-100 leading-tight mt-0.5">{centerValue}</span>
      </div>
    </div>
  )
}
