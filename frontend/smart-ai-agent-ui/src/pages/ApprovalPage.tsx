import { useState } from 'react'
import { ErrorState } from '../components/ErrorState'
import { LoadingState } from '../components/LoadingState'
import { StatusBadge } from '../components/StatusBadge'
import { useAsyncData } from '../hooks/useAsyncData'
import { api } from '../services/api'

type ApprovalPageProps = {
  runId: string
  onCompleted: (id: string) => void
}

export const ApprovalPage = ({ runId, onCompleted }: ApprovalPageProps) => {
  const { data, error, loading } = useAsyncData(() => api.getRun(runId), [runId])
  const [comment, setComment] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  const handleApprove = async () => {
    setSubmitting(true)
    setActionError(null)

    try {
      const result = await api.approveRun(runId, comment)
      onCompleted(result.id)
    } catch (requestError) {
      setActionError(requestError instanceof Error ? requestError.message : 'Approval failed.')
    } finally {
      setSubmitting(false)
    }
  }

  const handleReject = async () => {
    if (!window.confirm('Reject this workflow run?')) {
      return
    }

    setSubmitting(true)
    setActionError(null)

    try {
      const result = await api.rejectRun(runId, comment)
      onCompleted(result.id)
    } catch (requestError) {
      setActionError(requestError instanceof Error ? requestError.message : 'Rejection failed.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <section className="page">
      {loading && <LoadingState />}
      {error && <ErrorState message={error} />}

      {data && (
        <div className="approval-layout">
          <article className="panel">
            <p className="page__eyebrow">Approval</p>
            <h2>Review user story output</h2>
            <div className="stack">
              <div>
                <span className="meta-label">Current status</span>
                <StatusBadge value={data.status} />
              </div>
              <div>
                <span className="meta-label">Stage</span>
                <p>{data.currentStage}</p>
              </div>
            </div>
          </article>

          <article className="form-panel">
            <label className="field">
              <span>Comment</span>
              <textarea
                value={comment}
                onChange={(event) => setComment(event.target.value)}
                rows={6}
                placeholder="Add approval notes or a rejection reason."
              />
            </label>

            {actionError && <div className="panel panel--error">{actionError}</div>}

            <div className="form-actions">
              <button type="button" className="button" disabled={submitting} onClick={handleApprove}>
                Approve
              </button>
              <button type="button" className="button button--danger" disabled={submitting || comment.trim().length === 0} onClick={handleReject}>
                Reject
              </button>
            </div>
          </article>
        </div>
      )}
    </section>
  )
}
