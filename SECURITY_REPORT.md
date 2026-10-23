# SafeVault — Security report

## Risks addressed

| Risk | Change made | Verification |
| --- | --- | --- |
| SQL injection in user lookup | Used SQLite SQL parameters (`$username`, etc.) in all data operations | NUnit SQL injection payload test |
| Stored/reflected XSS | Razor automatically HTML-encodes ordinary `@value` output; no `Html.Raw` usage | HTML encoder unit test; inspect page manually |
| Invalid user input | Data annotations for username, email and password; controller checks ModelState | NUnit model validation test |
| Password disclosure | `PasswordHasher<VaultUser>` stores salted, one-way hashes | NUnit password verification test |
| Unauthorized admin access | `[Authorize(Roles = "Admin")]`; public registration always sets User role | Role unit tests; manual browser access check required |
| Cross-site form submission | POST forms use antiforgery validation | Verify missing CSRF token is rejected locally |
| Cookie theft / disclosure | HttpOnly, SameSite=Lax; Secure when using HTTPS | Inspect Set-Cookie in browser developer tools |

The SQL injection scenario is a representative test payload, not evidence that the finished application was exploitable before remediation. The HTML encoder unit test confirms encoding behavior, but does not substitute for end-to-end XSS testing.

## Test execution

Run `dotnet test SafeVault.Tests/SafeVault.Tests.csproj` and record results here after local execution. **Tests have not been executed in the creation environment** because the .NET SDK was not available.

## AI assistance and Copilot reflection

Copilot assisted with the initial implementation and documentation.


## Further improvements

For a production application use ASP.NET Core Identity or a trusted identity provider, add lockout and MFA, validate email ownership, enforce HTTPS at the proxy, add secure audit logging, secret rotation, account recovery, security headers (including a CSP), production data protection key storage, and integration/penetration tests.
