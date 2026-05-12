namespace Pixtract.Application.DTOs;

public class ExtractionResultDto
{
    public int Id { get; set; }
    public bool Success { get; set; }
    public string? ProductId { get; set; }
    public string? Category { get; set; }
    public string? ProcessedAt { get; set; }
    public Dictionary<string, string?> Attributes { get; set; } = new();
    public string? Error { get; set; }
}
