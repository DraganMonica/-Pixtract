using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Pixtract.Application.DTOs;

public class BatchUploadDto : IValidatableObject
{
    [Required(ErrorMessage = "Te rugam sa selectezi o categorie.")]
    public string Category { get; set; }

    [Required(ErrorMessage = "Te rugam sa incarci cel putin o imagine.")]
    public List<IFormFile> Images { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Images == null || Images.Count == 0)
        {
            yield return new ValidationResult(
                "Te rugam sa incarci cel putin o imagine.",
                new[] { nameof(Images) });
            yield break;
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        const long maxSize = 10 * 1024 * 1024; // 10MB

        foreach (var image in Images)
        {
            var ext = Path.GetExtension(image.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
                yield return new ValidationResult(
                    $"Fisierul '{image.FileName}' nu este valid. Formate acceptate: jpg, jpeg, png, webp.",
                    new[] { nameof(Images) });

            if (image.Length > maxSize)
                yield return new ValidationResult(
                    $"Fisierul '{image.FileName}' depaseste limita de 10MB.",
                    new[] { nameof(Images) });
        }
    }
}
