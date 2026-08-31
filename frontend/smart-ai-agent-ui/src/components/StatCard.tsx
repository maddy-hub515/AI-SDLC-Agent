type StatCardProps = {
  label: string
  value: number
}

export const StatCard = ({ label, value }: StatCardProps) => {
  return (
    <article className="stat-card">
      <span className="stat-card__label">{label}</span>
      <strong className="stat-card__value">{value}</strong>
    </article>
  )
}
