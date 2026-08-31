export type ApiError = {
  code: string
  message: string
}

export type ApiResponse<T> = {
  success: boolean
  data: T
  error?: ApiError
}

export type RequirementStatus =
  | 'Draft'
  | 'Submitted'
  | 'Processing'
  | 'AwaitingApproval'
  | 'Approved'
  | 'Rejected'
  | 'Completed'
  | 'Failed'

export type AgentRunStatus =
  | 'Created'
  | 'Running'
  | 'WaitingForApproval'
  | 'Approved'
  | 'Rejected'
  | 'Completed'
  | 'Failed'

export type AgentStage =
  | 'None'
  | 'RequirementAnalysis'
  | 'UserStoryGeneration'
  | 'AwaitingApproval'
  | 'Development'
  | 'Testing'
  | 'CodeReview'
  | 'Deployment'
  | 'Completed'

export type ApprovalStatus = 'Pending' | 'Approved' | 'Rejected'
export type ApprovalType = 'UserStory' | 'Code' | 'Deployment' | 'Production'

export type Dashboard = {
  totalRequirements: number
  activeAgentRuns: number
  pendingApprovals: number
  completedRuns: number
  failedRuns: number
}

export type RequirementSummary = {
  id: string
  title: string
  status: RequirementStatus
  createdAtUtc: string
  updatedAtUtc: string
}

export type UserStory = {
  id: string
  title: string
  description: string
  acceptanceCriteria: string[]
  technicalAreas: string[]
  developmentTasks: string[]
  assumptions: string[]
  createdAtUtc: string
}

export type AgentRunSummary = {
  id: string
  status: AgentRunStatus
  currentStage: AgentStage
  startedAtUtc: string
  completedAtUtc: string | null
}

export type RequirementDetails = {
  id: string
  title: string
  description: string
  status: RequirementStatus
  createdAtUtc: string
  updatedAtUtc: string
  userStories: UserStory[]
  agentRuns: AgentRunSummary[]
}

export type Approval = {
  id: string
  type: ApprovalType
  status: ApprovalStatus
  comment: string | null
  createdAtUtc: string
  decidedAtUtc: string | null
}

export type WorkflowEvent = {
  id: string
  fromStage: AgentStage
  toStage: AgentStage
  eventType: string
  message: string
  createdAtUtc: string
}

export type AgentRunDetails = {
  id: string
  requirementId: string
  status: AgentRunStatus
  currentStage: AgentStage
  provider: string | null
  model: string | null
  promptVersion: string | null
  retryCount: number
  startedAtUtc: string
  completedAtUtc: string | null
  errorMessage: string | null
  approvals: Approval[]
  workflowEvents: WorkflowEvent[]
}

export type RequirementAnalysisResult = {
  userStoryId: string
  title: string
  description: string
  acceptanceCriteria: string[]
  technicalAreas: string[]
  developmentTasks: string[]
  assumptions: string[]
  createdAtUtc: string
}

export type RequirementAnalysis = {
  requirementId: string
  analysis: RequirementAnalysisResult | null
  latestRun: AgentRunDetails | null
}

export type PagedResult<T> = {
  items: T[]
  totalCount: number
}
