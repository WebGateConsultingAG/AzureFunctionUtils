# Security Audit — Detail-Checkliste (FunctionRunContext)

## A. Trust boundaries

| # | Prüfung | Erwartung |
|---|---------|-----------|
| A1 | Cloud ohne `FUNCTIONS_UTILS_LOCAL_DEVELOPMENT` | Nur `EasyAuthClaimsPrincipalParser`; Bearer-Parsers return `null` |
| A2 | Local flag | Nur `local.settings.json` / Process env — nie Azure Production App Settings in Docs/Code-Beispielen |
| A3 | Public Function App | README: APIM `validate-jwt` + Easy Auth Required — Library ersetzt das nicht |
| A4 | `AppFunctionRunContext` | Caller-supplied id/roles — Host muss nicht-HTTP-Caller absichern (out of library scope, aber dokumentieren wenn API erweitert wird) |

## B. Easy Auth (`EasyAuthClaimsPrincipalParser`)

| # | Prüfung | Erwartung |
|---|---------|-----------|
| B1 | Header | Nur `x-ms-client-principal`; fehlt/ leer → `null` |
| B2 | Decoding | Base64url mit Fallback Standard-Base64; Decode-Fehler → `null` (fail closed) |
| B3 | JSON | Deserialisierung fehlgeschlagen → `null`; keine partiellen Principals aus kaputtem JSON |
| B4 | Claims | `typ`/`val` aus JSON — kein zusätzliches Vertrauen in andere Header |
| B5 | Identity provider | `auth_typ` als Identity.Name — kein Escalation-Pfad durch leere Provider |

## C. Local Entra JWT (`EntraJwtClaimsPrincipalParser`)

| # | Prüfung | Erwartung |
|---|---------|-----------|
| C1 | Gate | Nur wenn `AzureFunctionsEnvironment.IsLocalDevelopment()` |
| C2 | Tenant | `FUNCTIONS_UTILS_AAD_TENANT_ID` required; fehlt → kein JWT |
| C3 | Audience | API: `FUNCTIONS_UTILS_AAD_AUDIENCE`; SPA: `FUNCTIONS_UTILS_AAD_SPA_CLIENT_ID`; `BuildValidAudiences` für `api://` + bare id |
| C4 | Validation params | Issuer (v2.0 + sts.windows.net), signing keys from OIDC metadata, lifetime, 2 min clock skew |
| C5 | Token extraction | Strikt `Authorization: Bearer {token}`; kein Token aus Query/Cookie |
| C6 | Failure handling | Validate failure → `null`; keine Roh-Exception an Caller |
| C7 | Claims mapping | `MapInboundClaims = false` — konsistent mit Easy Auth short names |
| C8 | OIDC fetch | Sync metadata on first use — kein Security-Bypass, aber Availability; keine user-controlled metadata URL |

## D. Resolver order (`ClaimsPrincipalResolver`)

| # | Prüfung | Erwartung |
|---|---------|-----------|
| D1 | Order | Easy Auth → (local) API Bearer → (local) SPA ID token |
| D2 | No merge | Erster erfolgreicher Principal gewinnt; kein Merge untrusted + trusted |
| D3 | UserId | `IsAuthenticated` iff non-whitespace UserId from mapped claims |
| D4 | Roles | Union distinct `roles` + `ClaimTypes.Role`; keine impliziten Admin-Rollen |

## E. Claim → identity mapping

| # | Prüfung | Erwartung |
|---|---------|-----------|
| E1 | User ID precedence | oid → objectidentifier URI → sub → nameidentifier URI |
| E2 | UPN precedence | upn → UPN URI → preferred_username → unique_name → email |
| E3 | Spoofing | Kein separater Code-Pfad, der UserId aus Query/Route ohne Principal setzt |

## F. Caching & side channels

| # | Prüfung | Erwartung |
|---|---------|-----------|
| F1 | `ForRequest` | Cache in `HttpContext.Items` — kein Cross-Request-Leak (pro Request neu) |
| F2 | OIDC manager cache | Keyed by metadata URL tenant — kein Cross-Tenant reuse bug bei Key-Änderung |
| F3 | Logging | Keine Token-Inhalte in neuem Code loggen |

## G. Public API surface

| # | Prüfung | Erwartung |
|---|---------|-----------|
| G1 | `GetPayLoad<T>()` | Nur JSON body; kein Auth — Host muss vorher autorisieren |
| G2 | `GetEnvironmentVariable` | Process scope — keine Secrets in Library hardcoden |
| G3 | `IsDev()` | Spiegelt nur local flag — nicht für Authorization allein verwenden (Consumer-Antipattern in Audit erwähnen) |

## H. Tests & supply chain

| # | Prüfung | Erwartung |
|---|---------|-----------|
| H1 | Unit tests | Easy Auth decode, empty resolver, `ForRequest` cache — erweitern bei neuen Zweigen |
| H2 | Dependencies | IdentityModel/JWT packages — keine bekannten downgrade auf unsichere Versionen im csproj |
| H3 | InternalsVisibleTo | Nur Test-Assembly |

## I. Documentation accuracy

| # | Prüfung | Erwartung |
|---|---------|-----------|
| I1 | README Security model | Stimmt mit Code-Pfaden überein |
| I2 | Env var table | Vollständig; keine veralteten Variablennamen |
| I3 | XML docs | Security-relevante `<remarks>` auf Parsern (fail closed, local-only) |

## Consumer misuse (nur als Audit-Hinweis, nicht Library-Fix)

Host apps sollen dokumentiert werden, wenn der Audit Integrationskontext sieht:

- Authorization nur mit `IsDev()` statt `IsAuthenticated()` + roles
- `new UserFunctionRunContext(req)` mehrfach pro Request (Performance, doppelte Validation)
- Local JWT env vars in Azure deployed
