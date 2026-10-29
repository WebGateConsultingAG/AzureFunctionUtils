# Local Entra JWT validation

Enabled **only** when `FUNCTIONS_UTILS_LOCAL_DEVELOPMENT=true` in `local.settings.json` (never in Azure App Settings).

## Environment variables

| Variable | Required for | Notes |
|----------|----------------|-------|
| `FUNCTIONS_UTILS_LOCAL_DEVELOPMENT` | Local JWT at all | Must be literal `true` (case-insensitive). Do not use `AZURE_FUNCTIONS_ENVIRONMENT` — Core Tools overwrites it. |
| `FUNCTIONS_UTILS_AAD_TENANT_ID` | API + SPA JWT | Entra tenant GUID |
| `FUNCTIONS_UTILS_AAD_AUDIENCE` | API access token | Exposed API (`api://{id}` or bare app id); both forms accepted via `BuildValidAudiences` |
| `FUNCTIONS_UTILS_AAD_SPA_CLIENT_ID` | SPA ID token | Entra application (client) id of the SPA — must match the token `aud` claim |

If tenant is missing, Bearer parsers return `null` (fail closed). API parser also requires audience; SPA parser requires SPA client id.

## Validation steps (EntraJwtClaimsPrincipalParser)

1. Read `Authorization: Bearer {token}`.
2. Load OpenID configuration for `https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid-configuration` (cached).
3. Validate signature, lifetime (2 min clock skew), issuer (`login.microsoftonline.com/.../v2.0` or `sts.windows.net/{tenant}/`), audience (one of built valid audiences).
4. On any failure return `null` — no exception propagated to callers (logged only if host logs handler failures).

## SPA vs API token order

`ClaimsPrincipalResolver` tries **API Bearer first**, then **SPA ID token**. A request carrying a valid API token is resolved as API; the SPA path applies when the caller sends an ID token (typical for local SPA → Functions without API resource scope).

## Operational notes

- **First request** after cold start may pay OIDC metadata latency (sync fetch in-process, cached per tenant). This affects availability/latency, not token trust — validation still uses signing keys from metadata. Keep the worker warm during local JWT testing.
- **Roles in ID token:** often empty or incomplete; host apps may supply roles from their own local configuration (outside this library).
- **Cloud:** disable local flag; rely on Easy Auth + APIM `validate-jwt` in front of the app.

## Troubleshooting

| Symptom | Check |
|---------|--------|
| `IsAuthenticated()` false locally | Bearer present? Tenant + SPA client id set? Token audience matches SPA id? |
| API token works, SPA not | `FUNCTIONS_UTILS_AAD_SPA_CLIENT_ID` missing or wrong |
| Easy Auth works in Azure, local empty | Expected without Easy Auth; enable local flag + JWT env vars |
| Invalid Easy Auth header locally | Header must be Base64(JSON) or Base64url — parser tries both |
