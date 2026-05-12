using Pixtract.Application.DTOs;

namespace Pixtract.Application.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Error)> RegisterAsync(RegisterDto dto);
    Task<(bool Success, string Token, string Error)> LoginAsync(string email, string password);
}
