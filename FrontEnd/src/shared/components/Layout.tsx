import { type ReactNode } from 'react'
import { NavBar } from './NavBar'

interface LayoutProps { children: ReactNode }

export function Layout({ children }: LayoutProps) {
  return (
    <div className="min-h-screen bg-slate-950">
      <NavBar />
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pt-24 pb-12">
        {children}
      </main>
    </div>
  )
}
