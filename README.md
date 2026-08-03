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
- A future uplift to `net11.0` would start at package version `11.0.0`

Within a major line, use minor/patch for library changes that stay on the same TFM.

---

## Environments

`AZURE_FUNCTIONS_ENVIRONMENT` distinguishes **where** the functions run:

| Value | Meaning | Auth behavior | `IsDev()` |
|---|---|---|---|
| `LocalDevelopment` | Functions on a developer machine | Easy Auth if present, otherwise validated Bearer JWT | `true` |
| `Development` | Azure cloud **DEV** environment | Easy Auth only | `false` |
| `Staging` / `Production` / other | Azure cloud environments | Easy Auth only | `false` |

Important: Azure’s usual `Development` value means the cloud DEV slot, **not** local execution. For local runs set `AZURE_FUNCTIONS_ENVIRONMENT=LocalDevelopment` in `local.settings.json`.

---

## Security model

Headers alone are not trusted. Protect cloud Function Apps like this:

```text
Internet → Azure API Management (validate-jwt) → Function App (Easy Auth Required) → UserFunctionRunContext
```

1. **Easy Auth required (cloud environments)**  
   On the Function App, enable App Service Authentication / Easy Auth and set unauthenticated requests to **Return HTTP 401**. Easy Auth strips client-supplied `x-ms-client-principal*` headers and replaces them after a successful Entra ID login.  
   Whenever the environment is **not** `LocalDevelopment`, this library accepts **only** Easy Auth (`x-ms-client-principal`). Bearer fallback is disabled.

2. **Do not expose the Function App publicly**  
   Prefer private networking and put **Azure API Management** in front. Use an APIM `validate-jwt` policy against Entra ID (issuer, audience, signing keys) before traffic reaches the Function App.

   Example APIM fragment:

   ```xml
   <validate-jwt header-name="Authorization" failed-validation-httpcode="401">
     <openid-config url="https://login.microsoftonline.com/{tenant-id}/v2.0/.well-known/openid-configuration" />
     <audiences>
       <audience>{api-app-id-or-uri}</audience>
     </audiences>
   </validate-jwt>
   ```

3. **Bearer JWT cryptographic validation (`LocalDevelopment` only)**  
   On a developer machine (`AZURE_FUNCTIONS_ENVIRONMENT=LocalDevelopment`), if Easy Auth is absent, the library may fall back to `Authorization: Bearer`. That token is validated (signature, issuer, audience, lifetime) via Entra OpenID metadata — decode-only is not used.

   Example `local.settings.json`:

   ```json
   {
     "Values": {
       "AZURE_FUNCTIONS_ENVIRONMENT": "LocalDevelopment",
       "FUNCTIONS_UTILS_AAD_TENANT_ID": "{tenant-id}",
       "FUNCTIONS_UTILS_AAD_AUDIENCE": "{api-app-id-or-uri}"
     }
   }
   ```

   | Variable | Purpose |
   |---|---|
   | `FUNCTIONS_UTILS_AAD_TENANT_ID` | Entra tenant ID |
   | `FUNCTIONS_UTILS_AAD_AUDIENCE` | API audience (app ID or Application ID URI) |

   If either variable is missing, Bearer fallback fails closed (`IsAuthenticated() == false`).

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
`IsDev()` follows the [Environments](#environments) table (`LocalDevelopment` only).

### UserFunctionRunContext

Identity is resolved in this order:

1. Azure Easy Auth payload from `x-ms-client-principal` (all environments)
2. Validated JWT from `Authorization: Bearer <token>` (`LocalDevelopment` only; requires tenant/audience env vars)

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
