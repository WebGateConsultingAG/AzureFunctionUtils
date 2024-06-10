# WebGate.Azure.FunctionsUtils

WebGate.Azure.FunctionsUtils provides assets to support Azure Functions. The main purpose is to make the life of a developer easier.

---
## FunctionRunContext
The FunctionRunContext encapsulates the extraction of the PrincipalClaims token. The context delivers functionality to determinate the user and also its roles.

### Create a new UserFunctionRunContext
The UserFunctionRunContext will be created by a HttpRequest. All information related to the current user, are extracted.

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
