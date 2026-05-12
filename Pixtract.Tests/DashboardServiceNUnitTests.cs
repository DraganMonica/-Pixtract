using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Pixtract.Application.Interfaces;
using Pixtract.Domain.Entities;
using Pixtract.Domain.Enums;
using Pixtract.Infrastructure.Services;
using Pixtract.Tests.Helpers;

namespace Pixtract.Tests;

[TestFixture]
public class DashboardServiceNUnitTests
{
    private DashboardService _service = null!;
    private Pixtract.Infrastructure.Data.ApplicationDbContext _db = null!;
    private Mock<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>> _userManagerMock = null!;
    private Mock<IUsageService> _usageMock = null!;

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb(Guid.NewGuid().ToString());
        _userManagerMock = TestHelpers.CreateMockUserManager();
        _usageMock = new Mock<IUsageService>();
        _service = new DashboardService(_db, _usageMock.Object, _userManagerMock.Object, NullLogger<DashboardService>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public async Task GetDailyLimitAsync_ReturneazaLimitaDinPlan()
    {
        var user = TestHelpers.CreateUser(plan: TestHelpers.ProPlan);
        _userManagerMock.Setup(m => m.Users)
            .Returns(new List<ApplicationUser> { user }.AsQueryable().BuildMock());

        var limit = await _service.GetDailyLimitAsync(user.Id);

        Assert.That(limit, Is.EqualTo(50));
    }

    [Test]
    public async Task GetPlanNameAsync_ReturneazaNumelePlanului()
    {
        var user = TestHelpers.CreateUser(plan: TestHelpers.ProPlan);
        _userManagerMock.Setup(m => m.Users)
            .Returns(new List<ApplicationUser> { user }.AsQueryable().BuildMock());

        var name = await _service.GetPlanNameAsync(user.Id);

        Assert.That(name, Is.EqualTo("Pro"));
    }

    [Test]
    public async Task GetRecentExtractionsAsync_ReturneazaDoarUltimeleN()
    {
        for (int i = 1; i <= 15; i++)
        {
            _db.ExtractionRequests.Add(new ExtractionRequest
            {
                UserId = "user-1",
                ProductId = $"P{i:D3}",
                Category = "Tricouri dama",
                ResultJson = "{}",
                Status = ExtractionStatus.Success,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _db.SaveChangesAsync();

        var result = await _service.GetRecentExtractionsAsync("user-1", count: 10);

        Assert.That(result.Count, Is.EqualTo(10));
    }

    [Test]
    public async Task GetRecentExtractionsAsync_MapeazaCorectAtributele()
    {
        _db.ExtractionRequests.Add(new ExtractionRequest
        {
            UserId = "user-1",
            ProductId = "P001",
            Category = "Tricouri dama",
            ResultJson = """{"Culoare de baza": "Rosu"}""",
            Status = ExtractionStatus.Success,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var result = await _service.GetRecentExtractionsAsync("user-1");

        Assert.That(result[0].Attributes["Culoare de baza"], Is.EqualTo("Rosu"));
    }
}