
- **HttpContext** contains all the information about the http request from start to the begining .

- **🍪 Cookies**
Cookies are small pieces of data stored on the user's browser by a website. They are used to remember information between requests, such as login sessions, user preferences, or tracking data. Each cookie consists of a name-value pair and may include attributes like expiration, path, domain, and security flags.


### 🔐 `/login` Endpoint – Setting a Cookie in ASP.NET Core

```csharp
app.MapGet("/login", (HttpContext ctx) =>
{
    ctx.Response.Headers["set-cookie"] = "auth=usr:zakaria";
    return "ok";
});
```
#### 🔐 What It Does
- Creates a `/login` route  
- Sets a cookie `auth=usr:zakaria` manually in the response header  
- Returns `"ok"` as plain text  

#### ⏳ Cookie Lifetime
- **Session cookie** (no expiration set)  
- Deleted when the browser is closed  

#### ⚠️ Security Notes
- Cookie is accessible to JavaScript (no `HttpOnly`)  
- Sent over HTTP and HTTPS (no `Secure`)  

- The first implementation is kind of work and we can simply set the user name cookie and getting it back .

#### ⚠️ Issues with Simple Cookie-Based Login (`auth=usr:zakaria`)

- No Authentication Validation
    - Anyone can forge the cookie (e.g., `auth=usr:admin`)
- No Encryption or Signature
    - Cookie value is plain text → easy to tamper with
    - ✅ Use JWTs or encrypted cookies
- Missing Security Flags
    - No `HttpOnly` → vulnerable to XSS
    - No `Secure` → vulnerable to MITM
    - ✅ Use:
        ```csharp
        new CookieOptions {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        }
        ```

### 🔐 What is `"auth-cookie"` in `CreateProtector("auth-cookie")`?

- `"auth-cookie"` is a **purpose string** used when creating a data protector with `IDataProtectionProvider`.
- It defines a **unique encryption scope**, ensuring that only code using the *same* purpose can decrypt the data.
- This adds a layer of **security isolation** within your application.

#### ✅ Key Points:
- **Not** the actual name of the cookie.
- Acts like a **namespace** for encryption.
- Ensures that data encrypted with `"auth-cookie"` can **only be decrypted** using the same purpose.

#### 🔄 Example:
```csharp
var protector1 = idp.CreateProtector("auth-cookie");
var protector2 = idp.CreateProtector("reset-token");

var encrypted = protector1.Protect("hello");
var decrypted = protector1.Unprotect(encrypted); // ✅ Works
var fail = protector2.Unprotect(encrypted);      // ❌ Throws exception
```

#### Using the protector idea

- This can solve some issues of the previous implementation
  - The cookie is protected
- But we still need to do this everywhere
- What about picking a different cookie for a different endpoint
- A clean up will microsoft implementation .
- Edge cases and security and browser specific cookie implementations


### 🔐 ClaimsPrincipal & ClaimsIdentity Summary

- `HttpContext.User` is a `ClaimsPrincipal` representing the current user.
- A `ClaimsPrincipal` contains one or more `ClaimsIdentity` instances.
- `User.Identity` refers to the first authenticated identity.

#### ✅ Common Access
- `User.Identity.Name` → returns value from `ClaimTypes.Name`
- `User.Identity.IsAuthenticated` → true if the identity is authenticated
- `User.FindFirst(ClaimTypes.Role)?.Value` → direct access to claims

#### 👥 Multiple Identities
- `ClaimsPrincipal` can hold multiple identities (e.g., from Google and local login).
- Useful in federated authentication or multi-factor scenarios.

- By default, `User.Identity` uses the first authenticated identity.



### Using microsoft cookie schema based authentication

- We can use the authentication cookie based and replace the following
  - Store the cookie and use the Authentication middlware
  - Protect and Unprotect the user information
  - Read back the claims and convert them to a ClaimsPrincipal generating an identity propery of the User .
  - Possibility of adding the Authorize attribute to verify the IsAuthenticated property .
  - Check the commit for implementation details .



## Policy based Authorization

- In order to authorize an endpoint (/**marocain** one) you can do some manual checks like 
  ```csharp
  if (!ctx.User.Identities.Any(...))
  if (!ctx.User.HasClaim("nationality", "marocain"))
  ```
  this will work but you will have to do the same if you need to authorize /**marrakech**.

- A cleaner way is to use policy based authorization 
    ```csharp
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("MarocainOnly", policy =>
            policy.RequireClaim("nationality", "marocain"));
    });
    app.MapGet("/marocain", [Authorize(Policy = "MarocainOnly")] () =>
    {
        return Results.Ok("marocain");
    });
    ```


