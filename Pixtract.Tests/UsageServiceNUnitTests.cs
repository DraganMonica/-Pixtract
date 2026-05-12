using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Pixtract.Domain.Entities;
using Pixtract.Infrastructure.Services;
using Pixtract.Tests.Helpers;

namespace Pixtract.Tests;

[TestFixture]
public class UsageServiceNUnitTests
{
    private UsageService _service = null!;
    private Pixtract.Infrastructure.Data.ApplicationDbContext _db = null!;
    private Moq.Mock<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>> _userManagerMock = null!;

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb(Guid.NewGuid().ToString());
        _userManagerMock = TestHelpers.CreateMockUserManager();
        _service = new UsageService(_db, _userManagerMock.Object, NullLogger<UsageService>.Instance);
    }


    // pentru a elibera resursele (se foloseste automat dupa fiecare test)
    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public async Task CanProcessAsync_UtilizatorNegasit_ReturneazaFalse()
    {
        _userManagerMock.Setup(m => m.Users)
            .Returns(new List<ApplicationUser>().AsQueryable().BuildMock());

        var result = await _service.CanProcessAsync("user-inexistent");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CanProcessAsync_SubLimita_ReturneazaTrue()
    {
        var user = TestHelpers.CreateUser();
        _userManagerMock.Setup(m => m.Users)
            .Returns(new List<ApplicationUser> { user }.AsQueryable().BuildMock());

        var result = await _service.CanProcessAsync(user.Id, 1);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CanProcessAsync_PesteLimita_ReturneazaFalse()
    {
        var user = TestHelpers.CreateUser();
        // FreePlan are 5 imagini/zi — simulam ca a folosit deja 5
        _db.UserDailyUsages.Add(new UserDailyUsage
        {
            UserId = user.Id,
            Date = DateTime.UtcNow.Date,
            ImagesUsed = 5
        });
        await _db.SaveChangesAsync();

        _userManagerMock.Setup(m => m.Users)
            .Returns(new List<ApplicationUser> { user }.AsQueryable().BuildMock());

        var result = await _service.CanProcessAsync(user.Id, 1);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IncrementAsync_FaraInregistrareAnterioara_CreeazaRecord()
    {
        await _service.IncrementAsync("user-1", 1);

        var record = _db.UserDailyUsages.FirstOrDefault(u => u.UserId == "user-1");
        Assert.That(record, Is.Not.Null);
        Assert.That(record!.ImagesUsed, Is.EqualTo(1));
    }

    [Test]
    public async Task IncrementAsync_CuInregistrareExistenta_Actualizeaza()
    {
        _db.UserDailyUsages.Add(new UserDailyUsage
        {
            UserId = "user-1",
            Date = DateTime.UtcNow.Date,
            ImagesUsed = 3
        });
        await _db.SaveChangesAsync();

        await _service.IncrementAsync("user-1", 2);

        var record = _db.UserDailyUsages.First(u => u.UserId == "user-1");
        Assert.That(record.ImagesUsed, Is.EqualTo(5));
    }

    [Test]
    public async Task GetTodayUsageAsync_FaraExtrageri_ReturneazaZero()
    {
        var result = await _service.GetTodayUsageAsync("user-fara-activitate");

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public async Task GetNextProductIdAsync_PrimaImagine_ReturneazaP001()
    {
        var result = await _service.GetNextProductIdAsync("user-nou");

        Assert.That(result, Is.EqualTo("P001"));
    }

    [Test]
    public async Task GetNextProductIdAsync_DupaDouaImagini_ReturneazaP003()
    {
        _db.UserDailyUsages.Add(new UserDailyUsage
        {
            UserId = "user-1",
            Date = DateTime.UtcNow.Date,
            ImagesUsed = 2
        });
        await _db.SaveChangesAsync();

        var result = await _service.GetNextProductIdAsync("user-1");

        Assert.That(result, Is.EqualTo("P003"));
    }
}