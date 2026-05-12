using Pixtract.Application.DTOs;

namespace Pixtract.Web.VMs;

public class AdminVM
{
    public List<UserDto> Users { get; set; } = new();
    public int TotalExtractions { get; set; }
    public int TodayExtractions { get; set; }
    public int ActiveUsersToday { get; set; }
    public int ExpiringSoon { get; set; }
    public List<CategoryStatDto> TopCategories { get; set; } = new();
}
