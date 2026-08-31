type EmptyStateProps = {
  title: string
  message: string
}

export const EmptyState = ({ title, message }: EmptyStateProps) => {
  return (
    <div className="empty-state">
      <h3>{title}</h3>
      <p>{message}</p>
    </div>
  )
}
