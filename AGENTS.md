# AGENTS.md

This file is the implementation contract for humans and coding agents working in this repository.

Use it as the source of truth for project rules until architecture tests are added.

## Project Context

- Main implementation project: `src/WeatherMonitor.Api`
- Local orchestration and entry point: `src/AppHost`
- Stack: .NET 10, Aspire, Keycloak, PostgreSQL, Redis, Vertical Slice Architecture, DDD, CQRS, MediatR

## Architecture Rules

### Domain Isolated

- The domain is isolated and is the DDD source-of-truth layer.
- No code from outside the domain may be imported into the domain.
- The domain must remain persistence-agnostic, framework-agnostic, and infrastructure-agnostic.
- Business rules and invariants belong in domain objects and domain methods.

### Dependency Direction

- Dependency arrows flow from the domain upward.
- Code at the same abstraction layer must not depend on peer implementations.
- Features must never import code from another feature, under any circumstances.
- If behavior or types are needed by more than one feature, move them to an explicit shared/common area or to the domain when appropriate.
- Do not reference another feature folder as a shortcut.

### Vertical Slice

- Feature endpoints live under `Features/<FeatureName>`.
- Each feature owns its own endpoint, command/query, handler, validator, and response types.
- Endpoints stay thin and only do HTTP orchestration:
  - bind request data
  - extract auth-derived data
  - dispatch through MediatR
  - define HTTP/OpenAPI metadata
- Business rules must not live in endpoints.
- Handlers may use `AppDbContext` directly.
- Do not introduce repository abstractions for this project.
- EF Core mappings must be implemented with `IEntityTypeConfiguration<T>`.

## API Rules

- All endpoints live under `/api/v{version}`.
- Protected endpoints must use `.RequireAuthorization()`.
- OpenAPI metadata must be explicit with `.Produces<>()`.
- `WithName` must follow this pattern:
  - `{http-verb}-v{version}-endpoint-path-with-hyphens`
- `WithDisplayName` must be human-friendly.
- `WithTags` must use the established resource tags such as `weather` and `monitors`.
- Success responses use:
  - `ApiResponse<T>`
  - `PagedApiResponse<T>`
- Error responses use `ProblemDetails`.
- Response record types must not include the HTTP verb in their names.

## Auth And Ownership

- `ClientId` always comes from `ClaimsPrincipal.Identity?.Name`.
- Never accept `ClientId` from route, query string, or request body.
- Cross-user monitor reads or writes must not expose resource existence.
- If a monitor belongs to another client, return `404`.
- Never return stored access tokens in API responses.

## Agreed Feature Rules

### Weather Conditions

- Weather-condition code endpoints read from the domain enumeration in memory.
- Do not query BrasilAPI for weather-condition code listing or lookup.
- Do not query the database for weather-condition code listing or lookup.

### Monitor Resource Shape

- The canonical non-secret monitor response shape is:
  - `MonitorId`
  - `CityCode`
  - `CityName`
  - `UF`
  - `WeatherConditionCode`
  - `WebhookUrl`
  - `Time`
  - `State`
- `MonitorId` is a GUID.

### Monitor Patch Contract

- `PATCH /monitors/{monitorId}` may only change:
  - `WebhookUrl`
  - `AccessToken`
  - `Time`
  - `Enabled`
- In PATCH requests:
  - `null` means "do not change this property"
  - `AccessToken = ""` means "clear the stored token"
- City and weather conditions are immutable after monitor creation.

### Duplicate Rule

- A duplicate monitor is:
  - same client
  - same resolved city
  - same scheduled time

## Integration Rules

### Brasil API

- BrasilAPI integration lives under `Infrastructure/Clients`.
- BrasilAPI response models live under `Infrastructure/Clients/Responses`.
- Delegating handlers live under `Infrastructure/Clients/Handlers`.
- BrasilAPI response models must use English property names.
- Map Portuguese JSON field names with `JsonPropertyName`.
- Keep the `IBrasilApiClient` name because it refers to the external BrasilAPI.
- Route path parts that mirror the external API may remain in Portuguese.
- HybridCache TTL for BrasilAPI GET caching is 1 day.

### Persistence

- Database shape and EF Core persistence live under `Infrastructure/Persistence`.
- Auto-migrations run only in Development.

## Guardrails

- Do not implement extra endpoints or abstractions outside the agreed issue list.
- Do not implement TickerQ jobs, outbox delivery, or Aspire Service Defaults unless a tracked issue explicitly covers that work.
- Do not add advanced architecture layers that are not required by the current POC scope.

## Working Style

- Prefer English names in code whenever possible.
- Keep external DTOs out of core layers.
- Keep handlers and endpoints aligned with the existing response and error contract.
- When in doubt, preserve the dependency direction:
  - domain at the core
  - application/API above it
  - infrastructure at the edges