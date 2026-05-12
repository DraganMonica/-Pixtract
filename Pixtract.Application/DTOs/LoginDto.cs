using System.ComponentModel.DataAnnotations;

namespace Pixtract.Application.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Email-ul este obligatoriu.")]
    [EmailAddress(ErrorMessage = "Email invalid.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Parola este obligatorie.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; } = false;
}
