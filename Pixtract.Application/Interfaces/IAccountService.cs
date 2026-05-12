using Pixtract.Application.DTOs;

namespace Pixtract.Application.Interfaces;

public interface IAccountService
{
    Task<(bool Success, string Error)> RegisterAsync(RegisterDto dto);
    Task<(bool Success, string Error)> LoginAsync(LoginDto dto);
    Task LogoutAsync();
}