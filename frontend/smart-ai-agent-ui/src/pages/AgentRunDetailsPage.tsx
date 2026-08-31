import { StatusBadge } from '../components/StatusBadge'
import { useAsyncData } from '../hooks/useAsyncData'
import { api } from '../services/api'
import { EmptyState } from '../components/EmptyState'
import { ErrorState } from '../components/ErrorState'
import { LoadingState } from '../components/LoadingState'

type AgentRunDetailsPageProps = {
  runId: string
  onOpenApproval: (id: string) => void
}

export const AgentRunDetailsPage = ({ runId, onOpenApproval }: AgentRunDetailsPageProps) => {
  const { data, error, loading } = useAsyncData(() => api.getRun(runId), [runId])

  return (
    <section className="page">
      {loading && <LoadingState />}
      {error && <ErrorState message={error} />}

      {data && (
        <>
          <div className="page__header">
            <div>
              <p className="page__eyebrow">Agent Run</p>
              <h2>{data.id}</h2>
            </div>

            {data.status === 'WaitingForApproval' && (
              <button type="button" className="button" onClick={() => onOpenApproval(data.id)}>
                Open Approval
              </button>
            )}
          </div>

          <div className="detail-grid">
            <article className="panel">
              <h3>Run status</h3>
              <div className="stack">
                <div>
                  <span className="meta-label">Status</span>
                  <StatusBadge value={data.status} />
                </div>
                <div>
                  <span className="meta-label">Current stage</span>
                  <p>{data.currentStage}</p>
                </div>
                <div>
                  <span className="meta-label">Started</span>
                  <p>{new Date(data.startedAtUtc).toLocaleString()}</p>
                </div>
                {data.errorMessage && (
                  <div>
                    <span className="meta-label">Error</span>
                    <p>{data.errorMessage}</p>
                  </div>
                )}
              </div>
            </article>

            <article className="panel">
              <h3>Approvals</h3>
              {data.approvals.length === 0 && <EmptyState title="No approvals found" message="Approval records will appear here once the workflow reaches a decision point." />}
              {data.approvals.map((approval) => (
                <div key={approval.id} className="audit-item">
                  <div className="audit-item__header">
                    <strong>{approval.type}</strong>
                    <StatusBadge value={approval.status} />
                  </div>
                  <p>{approval.comment ?? 'No comment recorded.'}</p>
                </div>
              ))}
            </article>
          </div>

          <article className="panel">
            <h3>Workflow history</h3>
            {data.workflowEvents.length === 0 && <EmptyState title="No workflow events" message="Run events will appear here as the workflow advances." />}
            {data.workflowEvents.map((event) => (
              <div key={event.id} className="audit-item">
                <div className="audit-item__header">
                  <strong>{event.eventType}</strong>
                  <span>{new Date(event.createdAtUtc).toLocaleString()}</span>
                </div>
                <p>
                  {event.fromStage} to {event.toStage}
                </p>
                <p>{event.message}</p>
              </div>
            ))}
          </article>
        </>
      )}
    </section>
  )
}
