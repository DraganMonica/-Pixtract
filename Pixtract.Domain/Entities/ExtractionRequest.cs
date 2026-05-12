using Pixtract.Domain.Enums;

namespace Pixtract.Domain.Entities;

// fiecare apel AI este un request
public class ExtractionRequest
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    // P001, P002, etc...
    public string ProductId { get; set; }

  
    public string Category { get; set; }

    // JSON cu atributele extrase
    public string ResultJson { get; set; }

    //tine minte UNDE este salvata imaginea
    public string? ImagePath { get; set; }

    // Success, Failed
    public ExtractionStatus Status { get; set; }        
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
