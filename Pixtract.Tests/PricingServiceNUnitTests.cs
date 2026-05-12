using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using MockQueryable.Moq;
using NUnit.Framework;
using Pixtract.Domain.Entities;
using Pixtract.Infrastructure.Services;
using Pixtract.Tests.Helpers;

namespace Pixtract.Tests;

[TestFixture]
public class PricingServiceNUnitTests
{
    private PricingService _service = null!;
    private Pixtract.Infrastructure.Data.ApplicationDbContext _db = null!;
    private Moq.Mock<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>> _userManagerMock = null!;

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb(Guid.NewGuid().ToString());
        _userManagerMock = TestHelpers.CreateMockUserManager();
        _service = new PricingService(_db, _userManagerMock.Object, NullLogger<PricingService>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }


    [Test]
    public async Task GetAllPlansAsync_ReturneazaPlanurileOrdonateDupaPrice()
    {
        _db.Plans.AddRange(TestHelpers.ProPlan, TestHelpers.FreePlan);
        await _db.SaveChangesAsync();

        var plans = await _service.GetAllPlansAsync();

        Assert.That(plans[0].Price, Is.LessThanOrEqualTo(plans[1].Price));
    }

    [Test]
    public async Task GetPlanByIdAsync_PlanExistent_ReturneazaPlan()
    {
        _db.Plans.Add(TestHelpers.FreePlan);
        await _db.SaveChangesAsync();

        var plan = await _service.GetPlanByIdAsync(1);

        Assert.That(plan, Is.Not.Null);
        Assert.That(plan!.Name, Is.EqualTo("Free"));
    }

    [Test]
    public async Task GetPlanByIdAsync_PlanInexistent_ReturneazaNull()
    {
        var plan = await _service.GetPlanByIdAsync(999);

        Assert.That(plan, Is.Null);
    }

    [Test]
    public async Task GetUserPlanAsync_UtilizatorCuPlan_ReturneazaPlanul()
    {
        var user = TestHelpers.CreateUser(plan: TestHelpers.ProPlan);
        _userManagerMock.Setup(m => m.Users)
            .Returns(new List<ApplicationUser> { user }.AsQueryable().BuildMock());

        var plan = await _service.GetUserPlanAsync(user.Id);

        Assert.That(plan, Is.Not.Null);
        Assert.That(plan!.Name, Is.EqualTo("Pro"));
    }
}