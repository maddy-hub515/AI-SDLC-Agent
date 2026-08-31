import type { ReactNode } from 'react'

type AppLayoutProps = {
  currentPage: string
  onNavigate: (page: string) => void
  children: ReactNode
}

const navigationItems = [
  { id: 'dashboard', label: 'Dashboard' },
  { id: 'requirements', label: 'Requirements' },
  { id: 'create', label: 'Create Requirement' },
]

export const AppLayout = ({ currentPage, onNavigate, children }: AppLayoutProps) => {
  return (
    <div className="shell">
      <aside className="sidebar">
        <div>
          <p className="sidebar__eyebrow">SmartAIAgent</p>
          <h1>Phase 1 Foundation</h1>
          <p className="sidebar__copy">Requirement intake, workflow state, approvals, and audit visibility.</p>
        </div>

        <nav className="sidebar__nav" aria-label="Primary navigation">
          {navigationItems.map((item) => (
            <button
              key={item.id}
              type="button"
              className={item.id === currentPage ? 'nav-link nav-link--active' : 'nav-link'}
              onClick={() => onNavigate(item.id)}
            >
              {item.label}
            </button>
          ))}
        </nav>
      </aside>

      <main className="content">{children}</main>
    </div>
  )
}
