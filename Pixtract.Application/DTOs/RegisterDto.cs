using System.ComponentModel.DataAnnotations;

namespace Pixtract.Application.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Email-ul este obligatoriu.")]
    [EmailAddress(ErrorMessage = "Email invalid.")]
    [StringLength(100, ErrorMessage = "Email-ul nu poate depasi 100 de caractere.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Parola este obligatorie.")]
    [StringLength(100, MinimumLength = 6,
        ErrorMessage = "Parola trebuie sa aiba minim 6 caractere.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirmarea parolei este obligatorie.")]
    [Compare("Password", ErrorMessage = "Parolele nu coincid.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }
}
