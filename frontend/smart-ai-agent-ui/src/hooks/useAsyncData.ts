import { useEffect, useState } from 'react'

type AsyncState<T> = {
  data: T | null
  error: string | null
  loading: boolean
}

export const useAsyncData = <T>(loader: () => Promise<T>, dependencies: readonly unknown[]): AsyncState<T> => {
  const [state, setState] = useState<AsyncState<T>>({ data: null, error: null, loading: true })

  useEffect(() => {
    let active = true
    setState({ data: null, error: null, loading: true })

    loader()
      .then((data) => {
        if (active) {
          setState({ data, error: null, loading: false })
        }
      })
      .catch((error: Error) => {
        if (active) {
          setState({ data: null, error: error.message, loading: false })
        }
      })

    return () => {
      active = false
    }
  }, dependencies)

  return state
}
