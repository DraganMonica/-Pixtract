using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Pixtract.Application.DTOs;
using Pixtract.Application.Interfaces;
using Pixtract.Domain.Entities;
using Pixtract.Infrastructure.Services;
using Pixtract.Tests.Helpers;

namespace Pixtract.Tests;

[TestFixture]
public class ExtractionServiceNUnitTests
{
    private ExtractionService _service = null!;
    private Pixtract.Infrastructure.Data.ApplicationDbContext _db = null!;
    private Mock<IAiService> _aiMock = null!;
    private Mock<IUsageService> _usageMock = null!;

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb(Guid.NewGuid().ToString());
        _aiMock = new Mock<IAiService>();
        _usageMock = new Mock<IUsageService>();

        var config = new Mock<IConfiguration>();
        config.Setup(c => c["UploadPath"]).Returns("uploads");

        _service = new ExtractionService(
            _db, _aiMock.Object, _usageMock.Object,
            NullLogger<ExtractionService>.Instance, config.Object);
    }

    private ExtractionRequestDto CreeazaRequest(string fileName = "test.jpg") => new()
    {
        Category = "Tricouri dama",
        FileName = fileName,
        ContentType = "image/jpeg",
        ImageBytes = new byte[100]
    };

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }


    [Test]
    public async Task ProcessAsync_ExtensieInvalida_ReturneazaEsec()
    {
        var dto = CreeazaRequest("poza.bmp");

        var result = await _service.ProcessAsync("user-1", dto);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Does.Contain("Format invalid"));
    }

    [Test]
    public async Task ProcessAsync_FisierPreaMarе_ReturneazaEsec()
    {
        var dto = CreeazaRequest();
        dto.ImageBytes = new byte[11 * 1024 * 1024]; // 11MB

        var result = await _service.ProcessAsync("user-1", dto);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Does.Contain("10MB"));
    }

    [Test]
    public async Task ProcessAsync_LimitaZilnicaAtinsa_ReturneazaEsec()
    {
        _usageMock.Setup(u => u.CanProcessAsync("user-1", 1)).ReturnsAsync(false);

        var result = await _service.ProcessAsync("user-1", CreeazaRequest());

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("LIMIT_REACHED"));
    }

    [Test]
    public async Task ProcessAsync_AIReusita_SalveazaInDB()
    {
        _usageMock.Setup(u => u.CanProcessAsync("user-1", 1)).ReturnsAsync(true);
        _usageMock.Setup(u => u.GetNextProductIdAsync("user-1")).ReturnsAsync("P001");
        _aiMock.Setup(a => a.ExtractAsync(
            It.IsAny<byte[]>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ExtractionResultDto
            {
                Success = true,
                Attributes = new Dictionary<string, string?> { { "Culoare", "Alb" } }
            });

        var result = await _service.ProcessAsync("user-1", CreeazaRequest());

        Assert.That(result.Success, Is.True);
        Assert.That(_db.ExtractionRequests.Count(), Is.EqualTo(1));
        _usageMock.Verify(u => u.IncrementAsync("user-1", It.IsAny<int>()), Times.Once);
    }

    [Test]
    public async Task ProcessAsync_AIEsuata_NuSalveazaInDB_NuIncrementeaza()
    {
        _usageMock.Setup(u => u.CanProcessAsync("user-1", 1)).ReturnsAsync(true);
        _usageMock.Setup(u => u.GetNextProductIdAsync("user-1")).ReturnsAsync("P001");
        _aiMock.Setup(a => a.ExtractAsync(
            It.IsAny<byte[]>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ExtractionResultDto { Success = false, Error = "Eroare API" });

        var result = await _service.ProcessAsync("user-1", CreeazaRequest());

        Assert.That(result.Success, Is.False);
        Assert.That(_db.ExtractionRequests.Count(), Is.EqualTo(0));
        _usageMock.Verify(u => u.IncrementAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task ProcessMultipleAsync_DepasesteLimitaRequestului_ReturneazaEsec()
    {
        var user = TestHelpers.CreateUser(plan: TestHelpers.FreePlan); // max 1 imagine/request
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var dtos = new List<ExtractionRequestDto>
        {
            CreeazaRequest("a.jpg"),
            CreeazaRequest("b.jpg")
        };

        var results = await _service.ProcessMultipleAsync(user.Id, dtos);

        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Success, Is.False);
        Assert.That(results[0].Error, Does.Contain("maxim"));
    }

    [Test]
    public async Task ProcessMultipleAsync_AIEsuata_NuIncrementeazaUsage()
    {
        var user = TestHelpers.CreateUser(plan: TestHelpers.ProPlan);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _usageMock.Setup(u => u.CanProcessAsync(user.Id, 1)).ReturnsAsync(true);
        _usageMock.Setup(u => u.GetNextProductIdAsync(user.Id)).ReturnsAsync("P001");
        _aiMock.Setup(a => a.ExtractAsync(
            It.IsAny<byte[]>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ExtractionResultDto { Success = false, Error = "Timeout" });

        await _service.ProcessMultipleAsync(user.Id, new List<ExtractionRequestDto> { CreeazaRequest() });

        _usageMock.Verify(u => u.IncrementAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        Assert.That(_db.ExtractionRequests.Count(), Is.EqualTo(0));
    }
}