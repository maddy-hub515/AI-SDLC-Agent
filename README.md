# SmartAIAgent

Phase 2 of an AI-powered SDLC platform. The project now includes real LLM-powered requirement analysis using Ollama, while preserving the Phase 1 foundation for requirement intake, workflow tracking, approval handling, audit history, REST APIs, tests, health checks, Swagger, and the React dashboard.

## Phase 2 Scope

- Create and store business requirements
- Trigger requirement analysis with AI
- Generate structured user stories, acceptance criteria, technical areas, development tasks, and assumptions
- Move runs to approval, approve or reject them, and store approval history
- Expose REST APIs and provide a React UI
- Support unit tests, integration tests, health checks, structured logging, and OpenAPI

## Architecture

- `SmartAIAgent.Domain`: entities and enums
- `SmartAIAgent.Application`: use cases, DTOs, interfaces, workflow logic
- `SmartAIAgent.Infrastructure`: EF Core persistence, Ollama integration, prompt loading, Serilog bootstrapping
- `SmartAIAgent.Api`: controllers, middleware, Swagger, health checks
- `tests/*`: unit and integration tests
- `frontend/smart-ai-agent-ui`: React dashboard

## Prerequisites

- .NET SDK 8.0
- Node.js 20+
- npm 10+
- Ollama installed locally for interactive AI analysis

## Ollama Setup

Install and start Ollama, then pull the model you want to use.

```bash
ollama serve
ollama pull llama3.1
```

Configure the API through `src/SmartAIAgent.Api/appsettings.json` or `appsettings.Development.json`.

```json
"AI": {
  "Provider": "Ollama",
  "BaseUrl": "http://localhost:11434",
  "Model": "llama3.1",
  "TimeoutSeconds": 120,
  "Temperature": 0.2
}
```

Do not commit secrets. If sensitive AI configuration is added later, use environment variables or user secrets.

## Backend Run

```bash
dotnet run --project src/SmartAIAgent.Api
```

Swagger is available at `/swagger` in development.

On startup the API applies EF Core migrations automatically.

## Frontend Run

```bash
cd frontend/smart-ai-agent-ui
npm install
npm run dev
```

## Database Setup

The API uses SQL Server LocalDB by default.

## Migrations

```bash
dotnet ef migrations add <Name> --project src/SmartAIAgent.Infrastructure --startup-project src/SmartAIAgent.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/SmartAIAgent.Infrastructure --startup-project src/SmartAIAgent.Api
```

## Requirement Analysis Flow

1. Create a business requirement.
2. Open the requirement details page.
3. Select `Analyze with AI`.
4. Review the generated user story, acceptance criteria, technical areas, development tasks, and assumptions.
5. Approve or reject the result.

## API Examples

Create a requirement:

```bash
curl -X POST http://localhost:5227/api/requirements \
  -H "Content-Type: application/json" \
  -d '{"title":"Remove outcode restriction","description":"Linked case assignment should ignore officer outcode when selecting eligible officers."}'
```

Start AI analysis:

```bash
curl -X POST http://localhost:5227/api/requirements/{requirementId}/analyze
```

Get AI analysis:

```bash
curl http://localhost:5227/api/requirements/{requirementId}/analysis
```

Approve analysis:

```bash
curl -X POST http://localhost:5227/api/agent-runs/{agentRunId}/approve \
  -H "Content-Type: application/json" \
  -d '{"comment":"Approved"}'
```

Reject analysis:

```bash
curl -X POST http://localhost:5227/api/agent-runs/{agentRunId}/reject \
  -H "Content-Type: application/json" \
  -d '{"reason":"Acceptance criteria need to be more specific."}'
```

## Tests

```bash
dotnet test tests/SmartAIAgent.UnitTests/SmartAIAgent.UnitTests.csproj
dotnet test tests/SmartAIAgent.IntegrationTests/SmartAIAgent.IntegrationTests.csproj
cd frontend/smart-ai-agent-ui
npm run build
```

## Project Structure

```text
SmartAIAgent/
  src/
  tests/
  frontend/
  docs/architecture/
```

## Prompt Files

Requirement prompts are externalized under `src/SmartAIAgent.Api/Prompts/Requirement/`.

## Phase Boundary

This phase does not implement Git integration, Azure DevOps integration, code generation, deployment, test generation, or later-stage SDLC agents.
