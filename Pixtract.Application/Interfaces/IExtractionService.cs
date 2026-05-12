using Pixtract.Application.DTOs;
using Pixtract.Domain.Entities;

namespace Pixtract.Application.Interfaces;

public interface IExtractionService
{
    Task<ExtractionResultDto> ProcessAsync(string userId, ExtractionRequestDto dto);
    Task<List<ExtractionResultDto>> ProcessMultipleAsync(string userId, List<ExtractionRequestDto> dtos);
    Task<List<ExtractionRequest>> GetHistoryAsync(string userId);
    Task<ExtractionRequest?> GetByIdAsync(int id, string userId);
}