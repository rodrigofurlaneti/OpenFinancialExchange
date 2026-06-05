import { lazy, Suspense } from 'react';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { Layout } from '../../design-system/components/Layout';
import { Spinner } from '../../design-system/components/Spinner';

const DashboardPage              = lazy(() => import('../../features/dashboard/pages/DashboardPage'));
const TransactionsPage           = lazy(() => import('../../features/transactions/pages/TransactionsPage'));
const AccountsPage               = lazy(() => import('../../features/accounts/pages/AccountsPage'));
const StatementsPage             = lazy(() => import('../../features/statements/pages/StatementsPage'));
const BanksPage                  = lazy(() => import('../../features/banks/pages/BanksPage'));
const ImportsPage                = lazy(() => import('../../features/imports/pages/ImportsPage'));
const LedgerBalancesPage         = lazy(() => import('../../features/ledgerBalances/pages/LedgerBalancesPage'));
const SignonSessionsPage         = lazy(() => import('../../features/signonSessions/pages/SignonSessionsPage'));
const TransactionCategoriesPage  = lazy(() => import('../../features/transactionCategories/pages/TransactionCategoriesPage'));

const Loading = () => <Spinner fullPage />;

const router = createBrowserRouter([
  {
    path: '/',
    element: <Layout />,
    children: [
      { index: true,                  element: <Suspense fallback={<Loading />}><DashboardPage /></Suspense> },
      { path: 'transactions',         element: <Suspense fallback={<Loading />}><TransactionsPage /></Suspense> },
      { path: 'accounts',             element: <Suspense fallback={<Loading />}><AccountsPage /></Suspense> },
      { path: 'statements',           element: <Suspense fallback={<Loading />}><StatementsPage /></Suspense> },
      { path: 'banks',                element: <Suspense fallback={<Loading />}><BanksPage /></Suspense> },
      { path: 'imports',              element: <Suspense fallback={<Loading />}><ImportsPage /></Suspense> },
      { path: 'ledger-balances',      element: <Suspense fallback={<Loading />}><LedgerBalancesPage /></Suspense> },
      { path: 'signon-sessions',      element: <Suspense fallback={<Loading />}><SignonSessionsPage /></Suspense> },
      { path: 'transaction-categories', element: <Suspense fallback={<Loading />}><TransactionCategoriesPage /></Suspense> },
    ],
  },
]);

export const AppRouter = () => <RouterProvider router={router} />;
