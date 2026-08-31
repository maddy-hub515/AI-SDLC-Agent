# Phase 2 Architecture

## Purpose

Phase 2 replaces the Phase 1 placeholder user story generation with a real LLM-powered requirement analysis flow.

## Main Components

- `IRequirementAgent` in Application coordinates analysis.
- `IPromptService` loads external prompt files.
- `ILlmService` provides provider-agnostic structured generation.
- `OllamaLlmService` in Infrastructure calls Ollama over `HttpClient`.
- `RequirementService` exposes analysis read models.
- `WorkflowService` continues to own approval and rejection transitions.

## Flow

1. `POST /api/requirements/{id}/analyze` triggers analysis.
2. `RequirementAgent` validates the request, prevents duplicate active runs, and creates an `AgentRun`.
3. Prompt files are loaded from `Prompts/Requirement/`.
4. `ILlmService.GenerateStructuredAsync<T>` requests structured JSON.
5. The structured response is validated.
6. The result is persisted using the existing `UserStory`, `UserStoryAcceptanceCriterion`, `AgentRun`, `Approval`, and `WorkflowEvent` models.
7. The run moves to `AwaitingApproval`.
8. A human approves or rejects the result.

## Persistence Notes

- `UserStory` stores title, description, acceptance criteria, and Phase 2 structured lists.
- `AgentRun` stores provider, model, prompt version, retry count, timestamps, status, and safe error messages.
- `WorkflowEvent` stores all workflow transitions.
- `Approval` stores the approval decision.

## Why `ILlmService` Is Provider-Agnostic

Requirement analysis should depend on a stable application contract, not a specific provider API. This keeps Phase 2 focused on workflow behavior and allows the Infrastructure layer to swap Ollama for another provider later with minimal change.

## Prompt Architecture

- `src/SmartAIAgent.Api/Prompts/Requirement/SystemPrompt.txt`
- `src/SmartAIAgent.Api/Prompts/Requirement/UserStoryPrompt.txt`

Prompts are externalized so they can be versioned and updated without embedding large prompt bodies in C# classes.
