import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom'
import { ProtectedRoute } from '../../shared/components/ProtectedRoute'
import { Layout } from '../../shared/components/Layout'
import { LoginPage } from '../../features/auth/LoginPage'
import { FinancialInstitutionsPage } from '../../features/financial-institutions/FinancialInstitutionsPage'
import { BankAccountsPage } from '../../features/bank-accounts/BankAccountsPage'
import { OfxPage } from '../../features/ofx/OfxPage'

const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <Layout><Navigate to="/institutions" replace /></Layout>,
        index: true,
        path: '/',
      },
      {
        path: '/institutions',
        element: <Layout><FinancialInstitutionsPage /></Layout>,
      },
      {
        path: '/bank-accounts',
        element: <Layout><BankAccountsPage /></Layout>,
      },
      {
        path: '/ofx',
        element: <Layout><OfxPage /></Layout>,
      },
    ],
  },
  { path: '*', element: <Navigate to="/" replace /> },
])

export function AppRouter() {
  return <RouterProvider router={router} />
}
