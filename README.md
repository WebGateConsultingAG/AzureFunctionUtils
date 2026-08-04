# WebGate.Azure.FunctionsUtils

Helpers for Azure Functions. The library provides `FunctionRunContext` types that expose user or application identity, UPN, roles, authentication state, request payload parsing, and local development detection.

**Package:** `WebGate.Azure.FunctionsUtils`  
**License:** [Apache-2.0](LICENSE)

```bash
dotnet add package WebGate.Azure.FunctionsUtils
```

## Target Framework & Versioning

| | |
|---|---|
| Target Framework | `net10.0` |
| Package version | `10.x.x` |

The **NuGet package major version matches the .NET target framework major version**.

- `net10.0` → package version `10.x.x`
- Future uplifts follow **.NET LTS** releases only (e.g. the next LTS after .NET 10); the package major then matches that TFM major.

Within a major line, use minor/patch for library changes that stay on the same TFM.

---

## Local development flag

Local vs cloud is controlled by one setting:

| Variable | Value | Effect |
|---|---|---|
| `FUNCTIONS_UTILS_LOCAL_DEVELOPMENT` | `true` | `IsDev() == true`, Bearer JWT allowed |
| unset / other | — | Cloud mode: Easy Auth only, no Bearer |

Set it **only** in `local.settings.json`. Do not set it in Azure App Settings.

```json
{
  "Values": {
    "FUNCTIONS_UTILS_LOCAL_DEVELOPMENT": "true",
    "FUNCTIONS_UTILS_AAD_TENANT_ID": "{tenant-id}",
    "FUNCTIONS_UTILS_AAD_AUDIENCE": "api://{api-app-id}"
  }
}
```

Do not use `AZURE_FUNCTIONS_ENVIRONMENT` for this — Core Tools overwrites it to `Development`.

---

## Security model

Headers alone are not trusted. Protect cloud Function Apps like this:

```text
Internet → Azure API Management (validate-jwt) → Function App (Easy Auth Required) → UserFunctionRunContext
```

1. **Easy Auth required (cloud)**  
   Enable App Service Authentication / Easy Auth and set unauthenticated requests to **Return HTTP 401**. Without `FUNCTIONS_UTILS_LOCAL_DEVELOPMENT`, this library accepts **only** Easy Auth (`x-ms-client-principal`).

2. **Do not expose the Function App publicly**  
   Prefer private networking and put **Azure API Management** in front with `validate-jwt`.

3. **Bearer JWT (local only)**  
   When `FUNCTIONS_UTILS_LOCAL_DEVELOPMENT=true` and Easy Auth is absent, `Authorization: Bearer` is validated (signature, issuer, audience, lifetime) via Entra OpenID metadata.

   | Variable | Purpose |
   |---|---|
   | `FUNCTIONS_UTILS_AAD_TENANT_ID` | Entra tenant ID |
   | `FUNCTIONS_UTILS_AAD_AUDIENCE` | API audience (GUID or `api://{app-id}`; both accepted) |

   If tenant/audience are missing, Bearer fails closed. The SPA must send an API access token (Expose an API), e.g. MSAL scope `api://{clientId}/access_as_user`.

---

## FunctionRunContext

Public entry points:

| Type | Purpose |
|---|---|
| `UserFunctionRunContext` | User identity from HTTP request headers |
| `AppFunctionRunContext` | Application identity from explicit id and roles |
| `IFunctionRunContext` | Shared API for identity, roles, payload, and environment |

Shared API (`IFunctionRunContext`):

- `GetUserId()`, `GetUPN()`, `IsAuthenticated()`, `IsDev()`
- `IsInAtLeastOneRole(...)`, `IsInAllRoles(...)`
- `GetPayLoad<T>()` — reads JSON body from the associated `HttpRequest` (user context only)
- `GetEnvironmentVariable(name)`

`UserFunctionRunContext` also exposes `GetClaimsPrincipal()`.  
`IsDev()` is `true` when `FUNCTIONS_UTILS_LOCAL_DEVELOPMENT` is enabled.

### UserFunctionRunContext

Identity is resolved in this order:

1. Azure Easy Auth payload from `x-ms-client-principal` (all environments)
2. Validated JWT from `Authorization: Bearer <token>` (only when `FUNCTIONS_UTILS_LOCAL_DEVELOPMENT=true`)

Claim mapping:

| Field | Claims (first match wins) |
|---|---|
| User ID | `oid`, Easy Auth object identifier URI, `sub`, name identifier URI |
| UPN | `upn`, Easy Auth UPN URI, `preferred_username` |
| Roles | `roles` |

Authenticated is `true` when a user ID was resolved.

```csharp
[Function("MyCoolFunction")]
public async Task<IActionResult> MyCoolFunction(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/me")] HttpRequest req)
{
    var context = new UserFunctionRunContext(req);
    if (!context.IsAuthenticated())
    {
        return new UnauthorizedResult();
    }

    var userId = context.GetUserId();
    return new OkObjectResult(userId);
}
```

### AppFunctionRunContext

Use for non-user / application callers. Pass application id and roles explicitly; no HTTP headers are parsed.

```csharp
var context = new AppFunctionRunContext(applicationId, roles: ["reader"]);
var appId = context.GetUserId();
```
