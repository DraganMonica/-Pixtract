using Pixtract.Application.DTOs;

namespace Pixtract.Application.Interfaces;

public interface IAiService
{
    Task<ExtractionResultDto> ExtractAsync(byte[] imageBytes, string fileName, string contentType, string category, string productId);
    Task<bool> IsAvailableAsync();
}
