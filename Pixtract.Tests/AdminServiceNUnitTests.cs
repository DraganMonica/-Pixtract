using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using MockQueryable.Moq;
using NUnit.Framework;
using Pixtract.Domain.Entities;
using Pixtract.Domain.Enums;
using Pixtract.Infrastructure.Services;
using Pixtract.Tests.Helpers;

namespace Pixtract.Tests;

[TestFixture]
public class AdminServiceNUnitTests
{
    private AdminService _service = null!;
    private Pixtract.Infrastructure.Data.ApplicationDbContext _db = null!;
    private Moq.Mock<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>> _userManagerMock = null!;

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb(Guid.NewGuid().ToString());
        _userManagerMock = TestHelpers.CreateMockUserManager();
        _service = new AdminService(_db, _userManagerMock.Object, NullLogger<AdminService>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public async Task GetTotalExtractionsAsync_ReturneazaNumarulCorect()
    {
        _db.ExtractionRequests.AddRange(
            new ExtractionRequest { UserId = "u1", ProductId = "P001", Category = "X", ResultJson = "{}", Status = ExtractionStatus.Success },
            new ExtractionRequest { UserId = "u1", ProductId = "P002", Category = "X", ResultJson = "{}", Status = ExtractionStatus.Success }
        );
        await _db.SaveChangesAsync();

        var total = await _service.GetTotalExtractionsAsync();

        Assert.That(total, Is.EqualTo(2));
    }

    [Test]
    public async Task GetTodayExtractionsAsync_ReturneazaDoarExtractiiDinZiuaDeAzi()
    {
        _db.ExtractionRequests.AddRange(
            new ExtractionRequest
            {
                UserId = "u1",
                ProductId = "P001",
                Category = "X",
                ResultJson = "{}",
                Status = ExtractionStatus.Success,
                CreatedAt = DateTime.UtcNow // azi
            },
            new ExtractionRequest
            {
                UserId = "u1",
                ProductId = "P002",
                Category = "X",
                ResultJson = "{}",
                Status = ExtractionStatus.Success,
                CreatedAt = DateTime.UtcNow.AddDays(-1) // ieri
            }
        );
        await _db.SaveChangesAsync();

        var count = await _service.GetTodayExtractionsAsync();

        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllUsersAsync_ReturneazaUtilizatoriiCuPlan()
    {
        var user = TestHelpers.CreateUser();
        _userManagerMock.Setup(m => m.Users)
            .Returns(new List<ApplicationUser> { user }.AsQueryable().BuildMock());

        var result = await _service.GetAllUsersAsync();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Email, Is.EqualTo(user.Email));
        Assert.That(result[0].PlanName, Is.EqualTo("Free"));
    }
}