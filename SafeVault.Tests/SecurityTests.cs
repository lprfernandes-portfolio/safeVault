using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SafeVault.Web.Controllers;
using SafeVault.Web.Data;
using SafeVault.Web.Models;
using SafeVault.Web.Services;

namespace SafeVault.Tests;
[TestFixture]
public class SecurityTests
{
    private string _databasePath = "";
    private UserRepository _repository = null!;
    private AuthService _auth = null!;
    [SetUp]
    public async Task SetUp()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"safevault-test-{Guid.NewGuid():N}.db");
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
        { ["ConnectionStrings:SafeVault"] = $"Data Source={_databasePath}" }).Build();
        _repository = new UserRepository(configuration);
        _auth = new AuthService(_repository);
        await _repository.InitializeAsync();
    }
    [TearDown]
    public void Cleanup() { if (File.Exists(_databasePath)) File.Delete(_databasePath); }

    private static bool Valid(object model) => Validator.TryValidateObject(model, new ValidationContext(model), new List<ValidationResult>(), true);
    [Test]
    public void RejectsInvalidRegistrationData()
    {
        Assert.That(Valid(new RegisterForm { Username = "", Email = "invalid", Password = "short" }), Is.False);
        Assert.That(Valid(new RegisterForm { Username = "valid_name", Email = "test@example.com", Password = "LongPassword123!" }), Is.True);
    }
    [Test]
    public async Task SqlInjectionPayloadIsDataNotExecutableSql()
    {
        await _auth.RegisterAsync(new RegisterForm { Username="alice",Email="alice@example.com",Password="LongPassword123!" });
        var malicious = "alice' OR 1=1 --";
        Assert.That(await _repository.FindByUsernameAsync(malicious), Is.Null);
        Assert.That(await _repository.FindByUsernameAsync("alice"), Is.Not.Null);
        Assert.That((await _repository.ListAsync()).Count, Is.EqualTo(1));
    }
    [Test]
    public void HtmlEncoderEscapesXssPayload()
    {
        var html = HtmlEncoder.Default.Encode("<script>alert('xss')</script>");
        Assert.That(html, Does.Not.Contain("<script>"));
        Assert.That(html, Does.Contain("&lt;"));
    }
    [Test]
    public async Task LoginAcceptsCorrectPasswordAndRejectsIncorrectPassword()
    {
        await _auth.RegisterAsync(new RegisterForm { Username="alice",Email="alice@example.com",Password="LongPassword123!" });
        Assert.That(await _auth.AuthenticateAsync("alice","wrong"), Is.Null);
        Assert.That(await _auth.AuthenticateAsync("alice","LongPassword123!"), Is.Not.Null);
        var user = await _repository.FindByUsernameAsync("alice");
        Assert.That(user!.PasswordHash, Is.Not.EqualTo("LongPassword123!"));
        Assert.That(new PasswordHasher<VaultUser>().VerifyHashedPassword(user,user.PasswordHash,"LongPassword123!"), Is.Not.EqualTo(PasswordVerificationResult.Failed));
    }
    [Test]
    public async Task RegistrationCannotSetAdminRole()
    {
        await _auth.RegisterAsync(new RegisterForm { Username="alice",Email="alice@example.com",Password="LongPassword123!" });
        Assert.That((await _repository.FindByUsernameAsync("alice"))!.Role, Is.EqualTo("User"));
    }
    [Test]
    public void AdminControllerRequiresAdminRole()
    {
        var attribute = (AuthorizeAttribute?)Attribute.GetCustomAttribute(typeof(AdminController),typeof(AuthorizeAttribute));
        Assert.That(attribute?.Roles, Is.EqualTo("Admin"));
    }
    [Test]
    public async Task DuplicateUsernameIsRejected()
    {
        var a=new RegisterForm {Username="alice",Email="alice@example.com",Password="LongPassword123!"};
        var b=new RegisterForm {Username="ALICE",Email="another@example.com",Password="LongPassword123!"};
        Assert.That(await _auth.RegisterAsync(a), Is.True);
        Assert.That(await _auth.RegisterAsync(b), Is.False);
    }
}
