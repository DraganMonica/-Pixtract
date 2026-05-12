using System.ComponentModel.DataAnnotations;

namespace Pixtract.Application.DTOs;

public class ExtractionRequestDto
{
    [Required]
    public string Category { get; set; }
    public byte[] ImageBytes { get; set; }
    public string FileName { get; set; }
    //tipul de fisier-ong, jpeg?
    public string ContentType { get; set; }
}
