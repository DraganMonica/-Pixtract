using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pixtract.Domain.Entities;
using Pixtract.Infrastructure.Data;

namespace Pixtract.Tests.Helpers;

public static class TestHelpers
{
    public static ApplicationDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    public static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    public static Plan FreePlan => new Plan
    {
        Id = 1,
        Name = "Free",
        DailyImageLimit = 5,
        ImagesPerRequest = 1,
        Price = 0,
        CanExportHistory = false
    };

    public static Plan ProPlan => new Plan
    {
        Id = 2,
        Name = "Pro",
        DailyImageLimit = 50,
        ImagesPerRequest = 5,
        Price = 9.99m,
        CanExportHistory = true
    };

    public static ApplicationUser CreateUser(string id = "user-1", Plan? plan = null)
    {
        var p = plan ?? FreePlan;
        return new ApplicationUser
        {
            Id = id,
            Email = "test@test.com",
            UserName = "test@test.com",
            PlanId = p.Id,
            Plan = p
        };
    }
}