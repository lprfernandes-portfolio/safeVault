using Microsoft.AspNetCore.Authentication.Cookies;
using SafeVault.Web.Data;
using SafeVault.Web.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Use HTTPS in deployment
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

// Create the SQLite database on first launch; seed admin only using explicit environment secrets.
using (var scope = app.Services.CreateScope())
{
    var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();
    await repository.InitializeAsync();
    var username = Environment.GetEnvironmentVariable("SAFEVAULT_ADMIN_USERNAME");
    var password = Environment.GetEnvironmentVariable("SAFEVAULT_ADMIN_PASSWORD");
    if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
    {
        var auth = scope.ServiceProvider.GetRequiredService<AuthService>();
        await auth.SeedAdminAsync(username, password);
    }
}
app.Run();
public partial class Program { }
