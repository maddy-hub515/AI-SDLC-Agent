import { ErrorState } from '../components/ErrorState'
import { LoadingState } from '../components/LoadingState'
import { StatCard } from '../components/StatCard'
import { useAsyncData } from '../hooks/useAsyncData'
import { api } from '../services/api'

export const DashboardPage = () => {
  const { data, error, loading } = useAsyncData(() => api.getDashboard(), [])

  return (
    <section className="page">
      <div className="page__header">
        <div>
          <p className="page__eyebrow">Overview</p>
          <h2>Operations dashboard</h2>
        </div>
      </div>

      {loading && <LoadingState />}
      {error && <ErrorState message={error} />}

      {data && (
        <div className="stats-grid">
          <StatCard label="Total requirements" value={data.totalRequirements} />
          <StatCard label="Active agent runs" value={data.activeAgentRuns} />
          <StatCard label="Pending approvals" value={data.pendingApprovals} />
          <StatCard label="Completed runs" value={data.completedRuns} />
          <StatCard label="Failed or rejected runs" value={data.failedRuns} />
        </div>
      )}
    </section>
  )
}
