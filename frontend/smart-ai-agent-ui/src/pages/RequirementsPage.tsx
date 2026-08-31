import { EmptyState } from '../components/EmptyState'
import { ErrorState } from '../components/ErrorState'
import { LoadingState } from '../components/LoadingState'
import { StatusBadge } from '../components/StatusBadge'
import { useAsyncData } from '../hooks/useAsyncData'
import { api } from '../services/api'

type RequirementsPageProps = {
  onSelectRequirement: (id: string) => void
}

export const RequirementsPage = ({ onSelectRequirement }: RequirementsPageProps) => {
  const { data, error, loading } = useAsyncData(() => api.getRequirements(), [])

  return (
    <section className="page">
      <div className="page__header">
        <div>
          <p className="page__eyebrow">Requirements</p>
          <h2>Requirement backlog</h2>
        </div>
      </div>

      {loading && <LoadingState />}
      {error && <ErrorState message={error} />}

      {data && data.items.length === 0 && <EmptyState title="No requirements yet" message="Create the first business requirement to begin the workflow." />}

      {data && data.items.length > 0 && (
        <div className="table-panel">
          <table className="table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Status</th>
                <th>Updated</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((item) => (
                <tr key={item.id}>
                  <td>{item.title}</td>
                  <td>
                    <StatusBadge value={item.status} />
                  </td>
                  <td>{new Date(item.updatedAtUtc).toLocaleString()}</td>
                  <td>
                    <button type="button" className="button button--secondary" onClick={() => onSelectRequirement(item.id)}>
                      View
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
