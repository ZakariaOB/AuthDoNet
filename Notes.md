
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

