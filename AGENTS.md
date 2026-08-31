# AGENTS.md

## Project Purpose

SmartAIAgent is an AI SDLC platform. Phase 2 adds the first real AI capability: LLM-powered requirement analysis using Ollama, while preserving the existing requirement management, workflow state tracking, approval handling, audit history, REST APIs, tests, and the React dashboard.

## Architecture Rules

- Preserve Clean Architecture boundaries
- Do not make Domain depend on EF Core, ASP.NET Core, or Infrastructure
- Keep controllers thin
- Keep workflow and approval logic in Application services
- Do not expose EF entities directly from the API

## Coding Standards

- Use .NET 8 and TypeScript
- Use async APIs with `CancellationToken` where appropriate
- Persist timestamps in UTC
- Prefer minimal, maintainable abstractions
- Use structured logging
- Validate inputs and return controlled errors

## Build Commands

```bash
dotnet build
```

## Test Commands

```bash
dotnet test
```

## Database Migration Commands

```bash
dotnet ef migrations add <Name> --project src/SmartAIAgent.Infrastructure --startup-project src/SmartAIAgent.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/SmartAIAgent.Infrastructure --startup-project src/SmartAIAgent.Api
```

## Frontend Commands

```bash
cd frontend/smart-ai-agent-ui
npm install
npm run dev
npm run build
```

## Guardrails

- Do not bypass architecture layers for convenience
- Do not add dependencies unnecessarily
- Validate changes with builds and tests before claiming completion
- Ollama is the current development LLM provider
- LLM providers must remain abstracted behind `ILlmService`
- Prompts must remain externalized
- AI output must be structured and validated
- Do not introduce repository-specific hallucinations
- Do not implement future phases early
- Never bypass the approval workflow
- Never hard-code secrets
- Always run tests after changes
- Do not add Azure DevOps, Git automation, deployment automation, or later-phase SDLC agents in this phase
- Never commit secrets, API keys, or credentialed connection strings
