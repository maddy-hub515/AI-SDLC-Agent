import { AxiosError } from 'axios'
import { apiClient } from '../api/client'
import type {
  AgentRunDetails,
  ApiResponse,
  Dashboard,
  PagedResult,
  RequirementAnalysis,
  RequirementDetails,
  RequirementSummary,
} from '../types/api'

type CreateRequirementRequest = {
  title: string
  description: string
}

const unwrap = async <T>(promise: Promise<{ data: ApiResponse<T> }>): Promise<T> => {
  try {
    const response = await promise
    if (!response.data.success) {
      throw new Error(response.data.error?.message ?? 'Request failed.')
    }

    return response.data.data
  } catch (error) {
    if (error instanceof AxiosError) {
      throw new Error(error.response?.data?.error?.message ?? error.message)
    }

    throw error
  }
}

export const api = {
  getDashboard: () => unwrap<Dashboard>(apiClient.get('/api/dashboard')),
  getRequirements: () => unwrap<PagedResult<RequirementSummary>>(apiClient.get('/api/requirements')),
  getRequirement: (id: string) => unwrap<RequirementDetails>(apiClient.get(`/api/requirements/${id}`)),
  getRequirementAnalysis: (id: string) => unwrap<RequirementAnalysis>(apiClient.get(`/api/requirements/${id}/analysis`)),
  createRequirement: (request: CreateRequirementRequest) => unwrap<RequirementDetails | RequirementSummary>(apiClient.post('/api/requirements', request)),
  analyzeRequirement: (requirementId: string) => unwrap<AgentRunDetails>(apiClient.post(`/api/requirements/${requirementId}/analyze`)),
  startRun: (requirementId: string) => unwrap<AgentRunDetails>(apiClient.post(`/api/requirements/${requirementId}/runs`)),
  getRun: (id: string) => unwrap<AgentRunDetails>(apiClient.get(`/api/agent-runs/${id}`)),
  approveRun: (id: string, comment: string) => unwrap<AgentRunDetails>(apiClient.post(`/api/agent-runs/${id}/approve`, { comment })),
  rejectRun: (id: string, reason: string) => unwrap<AgentRunDetails>(apiClient.post(`/api/agent-runs/${id}/reject`, { reason })),
}
