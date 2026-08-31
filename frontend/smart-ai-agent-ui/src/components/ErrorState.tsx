type ErrorStateProps = {
  message: string
}

export const ErrorState = ({ message }: ErrorStateProps) => <div className="panel panel--error">{message}</div>
