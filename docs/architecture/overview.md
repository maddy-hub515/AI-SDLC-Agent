# Architecture Overview

## System Architecture

SmartAIAgent Phase 2 continues using Clean Architecture. Domain remains persistence-agnostic. Application contains workflow logic, approval handling, requirement analysis orchestration, LLM abstractions, and DTOs. Infrastructure provides EF Core persistence, prompt loading, and the Ollama HTTP integration. API exposes REST endpoints and operational concerns. The React frontend consumes the API.

## Project Dependencies

- `Api -> Application -> Domain`
- `Api -> Infrastructure -> Application -> Domain`
- `Infrastructure -> Domain`
- Tests reference the layers they verify

## Domain Model

- `Project`
- `Requirement`
- `UserStory`
- `UserStoryAcceptanceCriterion`
- `AgentRun`
- `Approval`
- `WorkflowEvent`

## Workflow

Phase 2 workflow:

1. Requirement created
2. Agent run created
3. Requirement analysis stage recorded
4. User story generation stage recorded
5. AI processing stage recorded
6. Structured result persisted to the existing `UserStory` model
7. Awaiting approval stage recorded
8. Human approves or rejects
9. Audit history persists in `WorkflowEvents` and `Approvals`

Flow summary:

Requirement
-> RequirementAgent
-> PromptService
-> ILlmService
-> Ollama
-> Structured Result
-> Persistence
-> Approval

## API Structure

- `POST /api/requirements`
- `GET /api/requirements`
- `GET /api/requirements/{id}`
- `GET /api/requirements/{id}/analysis`
- `POST /api/requirements/{id}/analyze`
- `POST /api/requirements/{id}/runs`
- `GET /api/agent-runs/{id}`
- `POST /api/agent-runs/{id}/approve`
- `POST /api/agent-runs/{id}/reject`
- `GET /api/dashboard`
- `GET /health`

All API responses use a consistent `{ success, data | error }` envelope.

## AI Abstraction

`ILlmService` is defined in the Application layer so requirement analysis depends on an abstract capability instead of a provider-specific SDK or transport contract. `OllamaLlmService` lives in Infrastructure and uses `HttpClient`, allowing the provider to change later without rewriting application workflows.

## Future Phases

Later phases can add repository analysis, AI development agents, automated code review, test generation, Azure DevOps integration, deployment automation, and other post-approval SDLC stages. Those integrations remain intentionally excluded from Phase 2.
