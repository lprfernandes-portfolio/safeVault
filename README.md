# SafeVault

SafeVault is a small ASP.NET Core MVC application I built as part of a security and authentication exercise. It demonstrates input validation, parameterized SQLite queries, password hashing, authentication cookies, role-based access, and tests for common web vulnerabilities.

It is a learning project, **not an audited system for real credentials or financial records**.

## Requirements

- .NET 8 SDK
- Internet access for first-time NuGet package restore
- VS Code with C# Dev Kit (optional)

## Run locally

From the extracted `SafeVault` folder:

```bash
dotnet restore SafeVault.Web/SafeVault.Web.csproj
dotnet run --project SafeVault.Web --launch-profile https
```

Open `https://localhost:7162`. If localhost HTTPS is not trusted, run `dotnet dev-certs https --trust` (follow your operating system's prompts).

Register a normal user using a unique username and email and a password of at least 12 characters. No default password or admin credentials are committed.

### Create a local administrator (optional)

For PowerShell, set credentials **in the terminal only** before starting the server:

```powershell
$env:SAFEVAULT_ADMIN_USERNAME = "local_admin"
$env:SAFEVAULT_ADMIN_PASSWORD = "choose-a-unique-long-password-here"
dotnet run --project SafeVault.Web --launch-profile https
```

On Linux/macOS, use `export SAFEVAULT_ADMIN_USERNAME=local_admin` and `export SAFEVAULT_ADMIN_PASSWORD='...'`. This creates an admin on initial startup, if the username does not already exist. Clear environment variables afterward; do not commit credentials to GitHub. The sample email for seeded admins ends in `example.invalid` and is not a real inbox.

### Routes

- `/` — Home
- `/Account/Register` — Registration with validation and CSRF protection
- `/Account/Login` — Login
- `/Home/Profile` — Requires authentication
- `/Admin` — Requires the Admin role

## Run security tests

```bash
dotnet test SafeVault.Tests/SafeVault.Tests.csproj
```

Tests cover validation, SQL injection payload handling, HTML encoding, password hashing, unsuccessful login, role assignment, and admin authorization metadata. These tests focus on components, **not a full browser-based penetration test**. Also manually verify that an anonymous visitor is redirected from `/Home/Profile` to login and a signed-in regular user cannot open `/Admin`.

## Current limitations

This is intentionally simple. It does not implement account lockout, multi-factor authentication, password reset, email confirmation, centralized audit logs, data encryption at rest, or a production identity provider. It should be deployed behind HTTPS and hardened further before any real-world use.

See [SECURITY_REPORT.md](SECURITY_REPORT.md) for the security rationale, test matrix and a transparent note on AI assistance.
