import { useQuery } from '@tanstack/react-query'
import { dashboardApi } from '../api/dashboardApi'

export function useDashboardSummary(from: string, to: string) {
  return useQuery({
    queryKey: ['dashboard-summary', from, to],
    queryFn: () => dashboardApi.getSummary(from, to),
    enabled: !!from && !!to,
  })
}
