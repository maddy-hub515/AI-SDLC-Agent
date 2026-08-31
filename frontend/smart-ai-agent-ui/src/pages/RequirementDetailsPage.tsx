import { useState } from 'react'
import { EmptyState } from '../components/EmptyState'
import { ErrorState } from '../components/ErrorState'
import { LoadingState } from '../components/LoadingState'
import { StatusBadge } from '../components/StatusBadge'
import { useAsyncData } from '../hooks/useAsyncData'
import { api } from '../services/api'

type RequirementDetailsPageProps = {
  requirementId: string
  onSelectRun: (id: string) => void
}

export const RequirementDetailsPage = ({ requirementId, onSelectRun }: RequirementDetailsPageProps) => {
  const [refreshKey, setRefreshKey] = useState(0)
  const [analyzing, setAnalyzing] = useState(false)
  const [submittingDecision, setSubmittingDecision] = useState(false)
  const [rejectionReason, setRejectionReason] = useState('')
  const [actionError, setActionError] = useState<string | null>(null)
  const requirementState = useAsyncData(() => api.getRequirement(requirementId), [requirementId, refreshKey])
  const analysisState = useAsyncData(() => api.getRequirementAnalysis(requirementId), [requirementId, refreshKey])

  const refresh = () => setRefreshKey((current) => current + 1)

  const handleAnalyze = async () => {
    setAnalyzing(true)
    setActionError(null)

    try {
      const run = await api.analyzeRequirement(requirementId)
      onSelectRun(run.id)
      refresh()
    } catch (requestError) {
      setActionError(requestError instanceof Error ? requestError.message : 'Failed to analyze requirement.')
    } finally {
      setAnalyzing(false)
    }
  }

  const handleApprove = async () => {
    if (!analysisState.data?.latestRun) return

    setSubmittingDecision(true)
    setActionError(null)

    try {
      await api.approveRun(analysisState.data.latestRun.id, '')
      refresh()
    } catch (requestError) {
      setActionError(requestError instanceof Error ? requestError.message : 'Approval failed.')
    } finally {
      setSubmittingDecision(false)
    }
  }

  const handleReject = async () => {
    if (!analysisState.data?.latestRun) return

    setSubmittingDecision(true)
    setActionError(null)

    try {
      await api.rejectRun(analysisState.data.latestRun.id, rejectionReason)
      setRejectionReason('')
      refresh()
    } catch (requestError) {
      setActionError(requestError instanceof Error ? requestError.message : 'Rejection failed.')
    } finally {
      setSubmittingDecision(false)
    }
  }

  const loading = requirementState.loading || analysisState.loading
  const error = requirementState.error ?? analysisState.error
  const requirement = requirementState.data
  const analysis = analysisState.data
  const latestRun = analysis?.latestRun

  return (
    <section className="page">
      {loading && <LoadingState />}
      {error && <ErrorState message={error} />}

      {requirement && analysis && (
        <>
          <div className="page__header">
            <div>
              <p className="page__eyebrow">Requirement</p>
              <h2>{requirement.title}</h2>
            </div>

            <button type="button" className="button" onClick={handleAnalyze} disabled={analyzing || latestRun?.status === 'Running' || latestRun?.status === 'WaitingForApproval'}>
              {analyzing ? 'Analyzing requirement...' : 'Analyze with AI'}
            </button>
          </div>

          <div className="detail-grid">
            <article className="panel">
              <h3>Requirement information</h3>
              <p>{requirement.description}</p>
              <div className="stack">
                <div>
                  <span className="meta-label">Status</span>
                  <StatusBadge value={requirement.status} />
                </div>
                <div>
                  <span className="meta-label">Created</span>
                  <p>{new Date(requirement.createdAtUtc).toLocaleString()}</p>
                </div>
              </div>
            </article>

            <article className="panel">
              <h3>Agent run status</h3>
              {!latestRun && <EmptyState title="No analysis yet" message="Analyze this requirement to generate a structured AI result." />}
              {latestRun && (
                <div className="stack">
                  <div>
                    <span className="meta-label">Status</span>
                    <StatusBadge value={latestRun.status} />
                  </div>
                  <div>
                    <span className="meta-label">Current stage</span>
                    <p>{latestRun.currentStage}</p>
                  </div>
                  <div>
                    <span className="meta-label">Provider</span>
                    <p>{latestRun.provider ?? 'Not available'}</p>
                  </div>
                  <div>
                    <span className="meta-label">Model</span>
                    <p>{latestRun.model ?? 'Not available'}</p>
                  </div>
                  {latestRun.errorMessage && (
                    <div className="panel panel--error">
                      {latestRun.errorMessage}
                    </div>
                  )}
                  <div className="form-actions">
                    <button type="button" className="button button--secondary" onClick={() => onSelectRun(latestRun.id)}>
                      Open run details
                    </button>
                  </div>
                </div>
              )}
            </article>
          </div>

          {actionError && <div className="panel panel--error">{actionError}</div>}

          <article className="panel">
            <h3>AI generated user story</h3>
            {!analysis.analysis && latestRun?.status === 'Running' && <p>Analyzing requirement...</p>}
            {!analysis.analysis && !latestRun && <EmptyState title="No AI analysis yet" message="Run the Phase 2 requirement agent to generate the analysis." />}
            {!analysis.analysis && latestRun?.status === 'Failed' && <EmptyState title="Analysis failed" message="The latest AI run failed. Review the run status and try again." />}

            {analysis.analysis && (
              <div className="stack">
                <div className="story-card">
                  <h4>{analysis.analysis.title}</h4>
                  <p>{analysis.analysis.description}</p>
                </div>

                <div className="analysis-grid">
                  <div className="story-card">
                    <h4>Acceptance Criteria</h4>
                    <ul>
                      {analysis.analysis.acceptanceCriteria.map((item) => (
                        <li key={item}>{item}</li>
                      ))}
                    </ul>
                  </div>

                  <div className="story-card">
                    <h4>Technical Areas</h4>
                    <ul>
                      {analysis.analysis.technicalAreas.map((item) => (
                        <li key={item}>{item}</li>
                      ))}
                    </ul>
                  </div>

                  <div className="story-card">
                    <h4>Development Tasks</h4>
                    <ul>
                      {analysis.analysis.developmentTasks.map((item) => (
                        <li key={item}>{item}</li>
                      ))}
                    </ul>
                  </div>

                  <div className="story-card">
                    <h4>Assumptions</h4>
                    <ul>
                      {analysis.analysis.assumptions.map((item) => (
                        <li key={item}>{item}</li>
                      ))}
                    </ul>
                  </div>
                </div>
              </div>
            )}
          </article>

          {latestRun?.status === 'WaitingForApproval' && (
            <article className="form-panel">
              <h3>Approval</h3>
              <label className="field">
                <span>Reject reason</span>
                <textarea
                  value={rejectionReason}
                  onChange={(event) => setRejectionReason(event.target.value)}
                  rows={4}
                  placeholder="Provide a reason if you reject this analysis."
                />
              </label>

              <div className="form-actions">
                <button type="button" className="button" disabled={submittingDecision} onClick={handleApprove}>
                  Approve
                </button>
                <button type="button" className="button button--danger" disabled={submittingDecision || rejectionReason.trim().length === 0} onClick={handleReject}>
                  Reject
                </button>
              </div>
            </article>
          )}

          <article className="panel">
            <h3>Workflow history</h3>
            {!latestRun || latestRun.workflowEvents.length === 0 ? (
              <EmptyState title="No workflow events" message="Workflow history appears after analysis starts." />
            ) : (
              latestRun.workflowEvents.map((event) => (
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
              ))
            )}
          </article>
        </>
      )}
    </section>
  )
}
