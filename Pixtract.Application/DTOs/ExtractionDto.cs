namespace Pixtract.Application.DTOs;

public class ExtractionDto
{
    public int Id { get; set; }
    public string ProductId { get; set; }
    public string Category { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
    public string? ImagePath { get; set; }
}
