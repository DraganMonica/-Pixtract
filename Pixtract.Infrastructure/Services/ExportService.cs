using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using Pixtract.Application.Interfaces;
using Pixtract.Domain.Entities;
using System.Text.Json;

namespace Pixtract.Infrastructure.Services;

public class ExportService : IExportService
{
    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
    }

    // definesc coloanele care trebuie sa apara in Excel pentru fiecare categorie
    private readonly Dictionary<string, List<string>> _categoryColumns = new()
    {
        ["Tricouri dama"] = new()
            {
                "Imprimeu", "Tip decolteu", "Lungime maneca", "Culoare de baza", "Nuanta", "Croiala", "Pentru"
            },
        ["Tricouri barbati"] = new()
            {
                "Imprimeu", "Tip decolteu", "Lungime maneca", "Culoare de baza", "Nuanta", "Croiala", "Pentru"
            },
        ["Tricouri copii"] = new()
            {
               "Imprimeu", "Poveste/Personaj", "Culoare de baza", "Nuanta", "Pentru"
            },
        ["Bluze dama"] = new()
            {
                "Imprimeu", "Lungime maneca", "Culoare de baza", "Nuanta", "Croiala", "Sistem inchidere", "Pentru"
            },
        ["Sandale dama"] = new()
            {
                "Culoare de baza", "Nuanta", "Tip", "Tip toc", "Sistem inchidere", "Pentru"
            },
        ["Pantaloni sport barbati"] = new()
            {
                "Imprimeu", "Culoare de baza", "Nuanta", "Tip produs", "Lungime", "Pentru"
            }
    };

    // export produsul curent intr un fisier Excel
    public byte[] ExportCurrent(ExtractionRequest request)
    {
        _logger.LogInformation("Export Excel pentru produsul curent. Produs={ProductId}, Categorie={Category}",
            request.ProductId, request.Category);
        return GenerateExcel(new List<ExtractionRequest> { request }, request.Category);
    }

    
    public byte[] ExportHistory(List<ExtractionRequest> requests)
    {
        _logger.LogInformation("Export Excel istoric pornit. Total produse={Count}", requests.Count);

        using var workbook = new XLWorkbook();
        var grouped = requests.GroupBy(r => r.Category);

        foreach (var group in grouped)
        {
            var category = group.Key;
            var categoryRequests = group.ToList();

            if (!_categoryColumns.ContainsKey(category))
            {
                _logger.LogWarning("Categoria nu are coloane definite si va fi ignorata. Categorie={Category}", category);
                continue;
            }

            var columns = _categoryColumns[category];
            var sheetName = category.Length > 31 ? category.Substring(0, 31) : category;

            _logger.LogInformation("Sheet adaugat pentru categoria {Category}. Produse={Count}", category, categoryRequests.Count);
            AddSheet(workbook, sheetName, categoryRequests, columns);
        }

        if (!workbook.Worksheets.Any())
        {
            _logger.LogWarning("Nu exista date valide pentru export. S-a creat un sheet gol.");
            workbook.Worksheets.Add("Fara date");
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        _logger.LogInformation("Export istoric finalizat. Dimensiune fisier={Size}KB", stream.Length / 1024);
        return stream.ToArray();
    }

    // generez un fisier Excel pentru o lista de extrageri dintr o categorie
    private byte[] GenerateExcel(List<ExtractionRequest> requests, string category)
    {
        // PAS 1  creez workbook ul Excel(.xlsx)
        using var workbook = new XLWorkbook();

        // PAS 2  caut coloanele definite pentru categoria primita, ALTFEL: lista goala
        var columns = _categoryColumns.ContainsKey(category)
            ? _categoryColumns[category]
            : new List<string>();


        AddSheet(workbook, category, requests, columns);
        // Browser ul nu stie ce e XLWorkbook, asa ca se creeaza un recipient in ram pe care l recunoaste, unde pun excel
        using var stream = new MemoryStream();

        //transforma workbook ul intr un fisier Excel REAL
        workbook.SaveAs(stream);

        _logger.LogDebug("Fisier Excel generat pentru categoria {Category}. Dimensiune={Size}KB", category, stream.Length / 1024);

        // fisier ul excel se transforma in bytes, iar controller ul ulterior face return File(...), ca browser ul sa descarce .xlsx
        return stream.ToArray();
    }

    //adaug un sheet in Excel si completez randurile cu datele extrase
    private void AddSheet(
        IXLWorkbook workbook,
        string sheetName,
        List<ExtractionRequest> requests,
        List<string> attributeColumns)
    {
        var sheet = workbook.Worksheets.Add(sheetName);

        //definirea coloanelor comune pt toate categoriile
        var allColumns = new List<string> { "ProductId", "Category", "ProcessedAt" };
        allColumns.AddRange(attributeColumns);

        for (int i = 0; i < allColumns.Count; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = allColumns[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var req in requests)
        {
            var attrs = new Dictionary<string, string>();
            try
            {
                attrs = JsonSerializer.Deserialize<Dictionary<string, string>>(req.ResultJson) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Eroare la deserializarea atributelor pentru produsul {ProductId}.", req.ProductId);
            }

            sheet.Cell(row, 1).Value = req.ProductId;
            sheet.Cell(row, 2).Value = req.Category;
            sheet.Cell(row, 3).Value = req.CreatedAt.ToString("dd/MM/yyyy HH:mm");

            for (int i = 3; i < allColumns.Count; i++)
            {
                var key = allColumns[i];
                sheet.Cell(row, i + 1).Value = attrs.ContainsKey(key) ? attrs[key] ?? "" : "";
            }

            row++;
        }

        sheet.Columns().AdjustToContents();
        _logger.LogDebug("Sheet '{SheetName}' completat cu {RowCount} randuri.", sheetName, row - 2);
    }
}