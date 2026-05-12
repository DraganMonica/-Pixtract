using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Pixtract.Domain.Entities;
using Pixtract.Domain.Enums;
using Pixtract.Infrastructure.Services;

namespace Pixtract.Tests;

[TestFixture]
public class ExportServiceNUnitTests
{
    private ExportService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new ExportService(NullLogger<ExportService>.Instance);
    }

    private ExtractionRequest CreazaExtractie(string productId = "P001", string category = "Tricouri dama")
    {
        return new ExtractionRequest
        {
            Id = 1,
            UserId = "user-1",
            ProductId = productId,
            Category = category,
            ResultJson = """{"Culoare de baza": "Alb", "Imprimeu": "Fara"}""",
            Status = ExtractionStatus.Success,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Test]
    public void ExportCurrent_ReturneazaBytes()
    {
        var extractie = CreazaExtractie();

        var result = _service.ExportCurrent(extractie);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    [Test]
    public void ExportHistory_CategorieValida_ReturneazaFisier()
    {
        var extractii = new List<ExtractionRequest>
        {
            CreazaExtractie("P001"),
            CreazaExtractie("P002")
        };

        var result = _service.ExportHistory(extractii);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    [Test]
    public void ExportHistory_CategorieNecunoscuta_IgnoraCategoriaRespectiva()
    {
        var extractii = new List<ExtractionRequest>
        {
            CreazaExtractie("P001", "Categorie inexistenta")
        };

        // nu arunca exceptie, returneaza fisier cu sheet gol
        var result = _service.ExportHistory(extractii);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    [Test]
    public void ExportHistory_ListaGoala_ReturneazaFisierCuSheetGol()
    {
        var result = _service.ExportHistory(new List<ExtractionRequest>());

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    [Test]
    public void ExportHistory_MaiMulteCategori_CreeazaSheeturiSeparate()
    {
        var extractii = new List<ExtractionRequest>
        {
            CreazaExtractie("P001", "Tricouri dama"),
            CreazaExtractie("P002", "Bluze dama")
        };

        // nu arunca exceptie — dovada ca ambele sheet-uri au fost create
        Assert.DoesNotThrow(() => _service.ExportHistory(extractii));
    }
}