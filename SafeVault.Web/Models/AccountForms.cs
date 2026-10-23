using System.ComponentModel.DataAnnotations;
namespace SafeVault.Web.Models;
public class RegisterForm
{
    [Required, RegularExpression(@"^[a-zA-Z0-9_]{3,30}$", ErrorMessage = "Use 3–30 letters, numbers or underscores.")]
    public string Username { get; set; } = "";
    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = "";
    [Required, MinLength(12), MaxLength(128), DataType(DataType.Password)]
    public string Password { get; set; } = "";
}
public class LoginForm
{
    [Required] public string Username { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
}
