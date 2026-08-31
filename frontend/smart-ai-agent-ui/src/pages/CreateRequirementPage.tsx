import { useState } from 'react'
import { api } from '../services/api'

type CreateRequirementPageProps = {
  onCreated: (id: string) => void
}

export const CreateRequirementPage = ({ onCreated }: CreateRequirementPageProps) => {
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSaving(true)
    setError(null)

    try {
      const created = await api.createRequirement({ title, description })
      onCreated(created.id)
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Failed to create requirement.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <section className="page">
      <div className="page__header">
        <div>
          <p className="page__eyebrow">Create</p>
          <h2>New business requirement</h2>
        </div>
      </div>

      <form className="form-panel" onSubmit={handleSubmit}>
        <label className="field">
          <span>Title</span>
          <input value={title} onChange={(event) => setTitle(event.target.value)} maxLength={200} required />
        </label>

        <label className="field">
          <span>Description</span>
          <textarea value={description} onChange={(event) => setDescription(event.target.value)} rows={8} maxLength={4000} required />
        </label>

        {error && <div className="panel panel--error">{error}</div>}

        <div className="form-actions">
          <button type="submit" className="button" disabled={saving}>
            {saving ? 'Submitting...' : 'Submit Requirement'}
          </button>
        </div>
      </form>
    </section>
  )
}
