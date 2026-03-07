# Client Architecture

Date: YYYY-MM-DD  
Project: GameServerStudyAspNet / Unity Client

## 1. Purpose
- Define client architecture and module boundaries.
- Keep request/response flow consistent with server contracts.
- Provide clear rules for maintainability and testability.

## 2. Architecture Summary
- Flow: `UI -> Application (UseCase) -> Network (RPC) -> Server`

## 3. Layers

### 3.1 UI Layer
- Responsibility: Input handling, rendering, user feedback.
- Contains: Screens, popups, widgets.
- Rule: Do not call RPC directly from UI scripts.

### 3.2 Application Layer
- Responsibility: Use cases, orchestration, state update rules.
- Contains: Feature-level use case classes and facades.
- Rule: All feature flows must pass through this layer.

### 3.3 Network Layer
- Responsibility: RPC request/response, serialization, retry/timeout.
- Contains: API clients, transport wrappers, DTO mapping.
- Rule: No gameplay logic in network code.

### 3.4 DevTools Layer
- Responsibility: Debug panels, scenario runners, diagnostic logs.
- Contains: Internal tools for QA and local validation.
- Rule: Gate by build flag and exclude from release behavior.

## 4. Suggested Folder Layout
```txt
Assets/Scripts/
  Network/
  Application/
  UI/
  DevTools/
  Foundation/
```

## 5. Request Processing Flow
1. UI event occurs.
2. Use case validates input and builds request.
3. Network layer sends RPC request.
4. Response is validated and mapped.
5. Shared context/state is updated.
6. UI reacts to new state.

## 6. Error Handling Policy
- Map server error codes to user-facing messages.
- Define retriable vs non-retriable errors.
- Standardize timeout and duplicate-request handling.
- Use a common log schema for requests, responses, and errors.

## 7. State Management Policy
- Define authoritative fields from server.
- Define client-cached fields and invalidation rules.
- Define conflict resolution when local and server state differ.

## 8. Quality Gates
- Baseline runtime target (FPS/GC budget).
- API response visibility in debug panel.
- Minimum PlayMode test coverage for critical flows.

## 9. Security and Operations
- No secrets in client assets or source.
- Restrict debug and cheat functionality by build type.
- Separate config for local/dev/release.

## 10. Open Issues
- [ ] Issue 1
- [ ] Issue 2
- [ ] Issue 3

## 11. Change Log
- YYYY-MM-DD: Initial draft
