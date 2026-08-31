type StatusBadgeProps = {
  value: string
}

export const StatusBadge = ({ value }: StatusBadgeProps) => {
  const tone = value.toLowerCase()
  return <span className={`status-badge status-badge--${tone}`}>{value}</span>
}
