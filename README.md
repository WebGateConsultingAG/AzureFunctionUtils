# WebGate.Azure.FunctionsUtils

Helpers for Azure Functions. The library centers on `FunctionRunContext`, which extracts Easy Auth / `x-ms-client-principal` claims from an `HttpRequest` and exposes user identity, UPN, roles, authentication state, and local-dev overrides.

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

## FunctionRunContext

The FunctionRunContext encapsulates the extraction of the PrincipalClaims token. The context delivers functionality to determine the user and also its roles.

### Create a new UserFunctionRunContext

The UserFunctionRunContext will be created by a HttpRequest. All information related to the current user are extracted.

```c#
[Function("MyCoolFunction")]
    public async Task<IActionResult> MyCoolFunction([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/me")] HttpRequest req)
    {
        var userFunctionRunContext = new UserFunctionRunContext(req);
        //Do some stuff with the graphAPI to find the User by its UserID
        var myGraphUser = await doSomeCoolStuff(userFunctionRunContext.GetUserId());
        return new OkObjectResult(myGraphUser);
    }
```
