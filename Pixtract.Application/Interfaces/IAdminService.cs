using Pixtract.Application.DTOs;

namespace Pixtract.Application.Interfaces;

public interface IAdminService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<int> GetTotalExtractionsAsync();
    Task<int> GetTodayExtractionsAsync();
    Task<int> GetActiveUsersTodayAsync();
    Task<int> GetExpiringSoonAsync();
    Task<List<CategoryStatDto>> GetTopCategoriesAsync();
}
