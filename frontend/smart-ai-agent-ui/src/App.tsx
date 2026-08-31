import { useState } from 'react'
import { AppLayout } from './layouts/AppLayout'
import { AgentRunDetailsPage } from './pages/AgentRunDetailsPage'
import { ApprovalPage } from './pages/ApprovalPage'
import { CreateRequirementPage } from './pages/CreateRequirementPage'
import { DashboardPage } from './pages/DashboardPage'
import { RequirementDetailsPage } from './pages/RequirementDetailsPage'
import { RequirementsPage } from './pages/RequirementsPage'

type ViewState =
  | { name: 'dashboard' }
  | { name: 'requirements' }
  | { name: 'create' }
  | { name: 'requirementDetails'; requirementId: string }
  | { name: 'agentRunDetails'; runId: string }
  | { name: 'approval'; runId: string }

function App() {
  const [view, setView] = useState<ViewState>({ name: 'dashboard' })

  const currentPage = view.name === 'requirementDetails' || view.name === 'agentRunDetails' || view.name === 'approval' ? 'requirements' : view.name

  return (
    <AppLayout
      currentPage={currentPage}
      onNavigate={(page) => {
        if (page === 'dashboard') setView({ name: 'dashboard' })
        if (page === 'requirements') setView({ name: 'requirements' })
        if (page === 'create') setView({ name: 'create' })
      }}
    >
      {view.name === 'dashboard' && <DashboardPage />}
      {view.name === 'requirements' && <RequirementsPage onSelectRequirement={(requirementId) => setView({ name: 'requirementDetails', requirementId })} />}
      {view.name === 'create' && <CreateRequirementPage onCreated={(requirementId) => setView({ name: 'requirementDetails', requirementId })} />}
      {view.name === 'requirementDetails' && (
        <RequirementDetailsPage
          requirementId={view.requirementId}
          onSelectRun={(runId) => setView({ name: 'agentRunDetails', runId })}
        />
      )}
      {view.name === 'agentRunDetails' && <AgentRunDetailsPage runId={view.runId} onOpenApproval={(runId) => setView({ name: 'approval', runId })} />}
      {view.name === 'approval' && <ApprovalPage runId={view.runId} onCompleted={(runId) => setView({ name: 'agentRunDetails', runId })} />}
    </AppLayout>
  )
}

export default App
