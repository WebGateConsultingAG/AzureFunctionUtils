# WebGate.Azure.FunctionsUtils — Agent-Leitfaden

NuGet **`WebGate.Azure.FunctionsUtils`** (TFM `net10.0`, Paket-Major = .NET-Major). Kern: **`UserFunctionRunContext`** / **`AppFunctionRunContext`** — Identität, Rollen, Payload, Local-Dev-Erkennung für Azure Functions (isolated worker, ASP.NET Core `HttpRequest`).

**README:** [README.md](./README.md) · **Architektur / Resolver-Kette:** [docs/architecture-function-run-context.md](./docs/architecture-function-run-context.md) · **Local JWT / SPA ID-Token:** [docs/local-entra-jwt-validation.md](./docs/local-entra-jwt-validation.md)

## Skills

| Skill | Zweck |
|-------|--------|
| `@security-audit-function-utils` | Security-Audit (Auth, JWT, Easy Auth, fail-closed, Env-Flags, Docs/Tests) |

## Build & Test

```bash
dotnet build AzureFunctionUtils.sln -c Release
dotnet test AzureFunctionUtils.sln -c Release
```

CI (`.github/workflows/dotnet.yml`): Build, Test, NuGet push to nuget.org on push to **`main`** (`--skip-duplicate`).

## Versioning

- Package version in `AzureFunctionsUtils/AzureFunctionsUtils.csproj` (`Version`).
- Breaking public API → Major bump (aligned with TFM major).
- **10.0.4+:** `UserFunctionRunContext.ForRequest`, SPA ID token parser, `LocalEntraJwtEnvironment`, Easy Auth Base64url fallback, unit tests.

## Principles (bei Änderungen)

- **Fail closed:** Ungültige/missing Auth → kein Principal, `IsAuthenticated() == false` (kein Silent-Fallback auf anonyme Admin-Rechte).
- **Cloud:** Nur **Easy Auth** (`x-ms-client-principal`). Kein Bearer-JWT ohne `FUNCTIONS_UTILS_LOCAL_DEVELOPMENT=true`.
- **Local:** Easy Auth first, dann API Bearer (`FUNCTIONS_UTILS_AAD_AUDIENCE`), dann SPA ID token (`FUNCTIONS_UTILS_AAD_SPA_CLIENT_ID`).
- **Ein Request, ein Context:** Consumer sollen **`UserFunctionRunContext.ForRequest(req)`** nutzen (JWT/OIDC nicht mehrfach pro Request).
- **XML-Docs:** `public` und `internal` Kern-APIs ausführlich (`///`): Intent, Nebenwirkungen, Skip/Fehler, `<remarks>` für Idempotenz/Cache/Security. Beim Split Docs **mitverschieben**, nicht kürzen.

## Integration (host apps)

- **`local.settings.json`:** `FUNCTIONS_UTILS_LOCAL_DEVELOPMENT`, `FUNCTIONS_UTILS_AAD_TENANT_ID`, `FUNCTIONS_UTILS_AAD_SPA_CLIENT_ID` (Entra SPA application/client id); optional `FUNCTIONS_UTILS_AAD_AUDIENCE` for API access tokens.
- **SPA frontends locally:** when the UI sends an **ID token** instead of an API access token to the Functions host, set the SPA client id variable; audience on the token must match that id.
- **Repeated identity reads:** use **`UserFunctionRunContext.ForRequest(req)`** (or expose `GetClaimsPrincipal()` once per request in the host) so JWT validation and claim mapping run once.

## Datei-Landkarte

| Bereich | Ort |
|--------|-----|
| Public API | `FunctionRunContext/*.cs` |
| Resolver / JWT / Easy Auth | `FunctionRunContext/Internal/` |
| Tests | `AzureFunctionsUtils.Tests/` (`InternalsVisibleTo`) |

God-Datei vermeiden: neue Auth-Zweige als **named parser** + Eintrag in `ClaimsPrincipalResolver`, nicht inline in `UserFunctionRunContext`.

## Commit-Gate

- `dotnet test AzureFunctionUtils.sln -c Release` grün.
- README + `docs/*` bei Verhaltens-/Env-Änderungen mitziehen.
- Kein `FUNCTIONS_UTILS_LOCAL_DEVELOPMENT` in Cloud-Dokumentation als empfohlene App Setting.
