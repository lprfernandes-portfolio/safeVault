using Microsoft.AspNetCore.Identity;
using SafeVault.Web.Data;
using SafeVault.Web.Models;

namespace SafeVault.Web.Services;
public class AuthService
{
    private readonly UserRepository _repository;
    private readonly PasswordHasher<VaultUser> _hasher = new();
    public AuthService(UserRepository repository) => _repository = repository;
    public async Task<bool> RegisterAsync(RegisterForm input)
    {
        // Role is never accepted from the public registration form.
        var user = new VaultUser { Username = input.Username, Email = input.Email, Role = "User" };
        user.PasswordHash = _hasher.HashPassword(user, input.Password);
        return await _repository.CreateAsync(user);
    }
    public async Task<VaultUser?> AuthenticateAsync(string username, string password)
    {
        var user = await _repository.FindByUsernameAsync(username);
        if (user is null) return null;
        var outcome = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return outcome == PasswordVerificationResult.Failed ? null : user;
    }
    public async Task SeedAdminAsync(string username, string password)
    {
        if (username.Length is < 3 or > 30 || password.Length < 12 ||
            !System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            throw new InvalidOperationException("Admin environment credentials do not meet validation requirements.");
        if (await _repository.FindByUsernameAsync(username) is not null) return;
        var admin = new VaultUser { Username = username, Email = $"{username}@example.invalid", Role = "Admin" };
        admin.PasswordHash = _hasher.HashPassword(admin, password);
        if (!await _repository.CreateAsync(admin))
            throw new InvalidOperationException("Unable to seed admin (possibly duplicate email).");
    }
}
