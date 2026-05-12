using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pixtract.Application.DTOs;
using Pixtract.Application.Interfaces;
using Pixtract.Domain.Entities;
using Pixtract.Domain.Enums;
using Pixtract.Infrastructure.Data;

namespace Pixtract.Infrastructure.Services;

public class ExtractionService : IExtractionService
{
    private readonly ApplicationDbContext _db;
    private readonly IAiService _aiService;
    // pt limitele user ului
    private readonly IUsageService _usageService;
    private readonly ILogger<ExtractionService> _logger;

    //unde salvez imaginile date -in uploads din pixtract-partea de web(frontend ul)
    private readonly string _uploadsPath;

    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    //limita 10MB
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public ExtractionService(ApplicationDbContext db, IAiService aiService,
                            IUsageService usageService, ILogger<ExtractionService> logger,
                            IConfiguration configuration)
    {
        _db = db;
        _aiService = aiService;
        _usageService = usageService;
        _logger = logger;
        // din appsettings.json -pixtract ia uploadpath 
        _uploadsPath = configuration["UploadPath"] ?? throw new Exception("UploadPath lipseste din configuratie.");
    }

    //proceseaza o singura imagine
    public async Task<ExtractionResultDto> ProcessAsync(string userId, ExtractionRequestDto dto)
    {
        // verificare extensie- nu permite alt tip de extensie fata de ce am mentionat
        var extension = Path.GetExtension(dto.FileName).ToLower();
        if (!_allowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Format de fisier invalid pentru utilizatorul {UserId}. Fisier={FileName}, Extensie={Extension}",
                userId, dto.FileName, extension);
            return new ExtractionResultDto { Success = false, Error = "Format invalid. Acceptam: jpg, jpeg, png, webp." };
        }

        // dimenisunea max 10MB
        if (dto.ImageBytes.Length > MaxFileSizeBytes)
        {
            _logger.LogWarning("Fisierul depaseste limita de 10MB pentru utilizatorul: {UserId}. Fisier={FileName}, Dimensiune={Size}MB",
                userId, dto.FileName, dto.ImageBytes.Length / 1024 / 1024);
            return new ExtractionResultDto { Success = false, Error = "Imaginea depaseste limita de 10MB." };
        }


        //verificare de limita
        var canProcess = await _usageService.CanProcessAsync(userId);
        if (!canProcess)
        {
            _logger.LogWarning("Limita zilnica atinsa pentru utilizatorul: {UserId}. Cererea a fost respinsa.", userId);
            return new ExtractionResultDto { Success = false, Error = "LIMIT_REACHED" };
        }

        // generarea id ului pt produse-P0001
        var productId = await _usageService.GetNextProductIdAsync(userId);
        _logger.LogInformation("Incepe extragerea pentru utilizatorul: {UserId}. Produs={ProductId}, Categorie={Category}, Fisier={FileName}",
            userId, productId, dto.Category, dto.FileName);

        // incperea extragerii-se foloseste de ce are  in componenta
        var result = await _aiService.ExtractAsync(
            dto.ImageBytes, dto.FileName, dto.ContentType, dto.Category, productId);

        if (!result.Success)
        {
            _logger.LogWarning("Extragerea a esuat pentru utilizatorul: {UserId}. Produs={ProductId}, Eroare={Error}",
                userId, productId, result.Error);
            return result;
        }

        _logger.LogInformation("Extragere reusita pentru utilizatorul: {UserId}. Produs={ProductId}, Atribute extrase={Count}",
            userId, productId, result.Attributes?.Count ?? 0);

        var extractionRequest = new ExtractionRequest
        {
            UserId = userId,
            ProductId = productId,
            Category = dto.Category,
            // transf din obiect in string JSON
            ResultJson = System.Text.Json.JsonSerializer.Serialize(result.Attributes),
            Status = ExtractionStatus.Success,
            CreatedAt = DateTime.UtcNow
        };
        //adauga in db, apoi salveza si in final creste usage ul user ului.
        _db.ExtractionRequests.Add(extractionRequest);
        await _db.SaveChangesAsync();
        await _usageService.IncrementAsync(userId);

        //returneaza extractia
        result.Id = extractionRequest.Id;
        return result;
    }

    // procesez mai multe imagini pentru acelasi utilizator
    public async Task<List<ExtractionResultDto>> ProcessMultipleAsync(string userId, List<ExtractionRequestDto> dtos)
    {
        // PAS 1  caut planul utilizatorului din baza de date
        var plan = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Plan)
            .FirstOrDefaultAsync();

        // PAS 2  iau limita maxima de imagini per request din plan
        // daca utilizatorul nu are plan, folosesc automat valoarea 1
        var imagesPerRequest = plan?.ImagesPerRequest ?? 1;

        // PAS 3  verific daca utilizatorul a trimis prea multe imagini intr un singur request
        if (dtos.Count > imagesPerRequest)
        {
            _logger.LogWarning("Utilizatorul {UserId} a depasit limita de imagini per request. Trimise={Sent}, Permise={Allowed}",
                userId, dtos.Count, imagesPerRequest);

            // returnez eroare daca depaseste limita planului
            return new List<ExtractionResultDto>
        {
            new() { Success = false, Error = $"Planul tau permite maxim {imagesPerRequest} imagine(i) per request." }
        };
        }

        // PAS 4  verific daca utilizatorul mai are imagini disponibile pentru ziua curenta
        var canProcess = await _usageService.CanProcessAsync(userId, dtos.Count);

        if (!canProcess)
        {
            _logger.LogWarning("Limita zilnica atinsa pentru utilizatorul: {UserId}. Cerere pentru {Count} imagini respinsa.",
                userId, dtos.Count);

            // returnez eroare daca limita zilnica este depasita
            return new List<ExtractionResultDto>
        {
            new() { Success = false, Error = "LIMIT_REACHED" }
        };
        }

        // PAS 5  loghez inceputul procesarii multiple
        _logger.LogInformation("Incepe procesarea a {Count} imagini pentru utilizatorul {UserId}. Categoria={Category}",
            dtos.Count, userId, dtos.FirstOrDefault()?.Category);

        // PAS 6  creez lista in care voi salva toate rezultatele
        var results = new List<ExtractionResultDto>();

        // PAS 7  procesez fiecare imagine pe rand
        foreach (var dto in dtos)
        {

            var extension = Path.GetExtension(dto.FileName).ToLower();

            if (!_allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Format invalid ignorat in procesare multipla. Fisier={FileName}", dto.FileName);

                // adaug eroare in lista de rezultate
                results.Add(new ExtractionResultDto
                {
                    Success = false,
                    Error = $"Format invalid: {dto.FileName}"
                });

                // trec la urmatoarea imagine
                continue;
            }

            if (dto.ImageBytes.Length > MaxFileSizeBytes)
            {
                _logger.LogWarning("Fisier prea mare ignorat in procesare multipla. Fisier={FileName}, Dimensiune={Size}MB",
                    dto.FileName, dto.ImageBytes.Length / 1024 / 1024);

                // adaug eroare daca fisierul este prea mare
                results.Add(new ExtractionResultDto
                {
                    Success = false,
                    Error = $"Fisier prea mare: {dto.FileName}"
                });

                continue;
            }

            var productId = await _usageService.GetNextProductIdAsync(userId);

            _logger.LogInformation("Extragere AI pornita pentru utilizatorul {UserId}. Produs={ProductId}, Fisier={FileName}",
                userId, productId, dto.FileName);

            // trimit imaginea catre AI pentru extragerea atributelor
            var result = await _aiService.ExtractAsync(
                dto.ImageBytes,
                dto.FileName,
                dto.ContentType,
                dto.Category,
                productId);

            // verific daca AI ul a returnat eroare
            if (!result.Success)
            {
                _logger.LogWarning("Extragerea a esuat pentru utilizatorul {UserId}. Produs={ProductId}, Eroare={Error}",
                    userId, productId, result.Error);

                // adaug rezultatul esuat in lista
                results.Add(result);

                // trec la urmatoarea imagine
                continue;
            }

            _logger.LogInformation("Extragere reusita pentru utilizatorul {UserId}. Produs={ProductId}, Atribute={Count}",
                userId, productId, result.Attributes?.Count ?? 0);

            var uniqueName = $"{Guid.NewGuid()}{extension}";

            try
            {
                //creez folderul uploads daca nu exista
                Directory.CreateDirectory(_uploadsPath);
                var filePath = Path.Combine(_uploadsPath, uniqueName);
                await File.WriteAllBytesAsync(filePath, dto.ImageBytes);

                _logger.LogInformation("Imaginea a fost salvata pe disc. Fisier={FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la salvarea imaginii pe disc. Fisier={FileName}", dto.FileName);
            }

            // PAS 7.11  creez entitatea care va fi salvata in baza de date
            var extractionRequest = new ExtractionRequest
            {
                UserId = userId,
                ProductId = productId,
                Category = dto.Category,

                // transform obiectul cu atribute in string JSON
                ResultJson = System.Text.Json.JsonSerializer.Serialize(result.Attributes),

                Status = ExtractionStatus.Success,
                CreatedAt = DateTime.UtcNow,
                ImagePath = $"/uploads/{uniqueName}"
            };

            _db.ExtractionRequests.Add(extractionRequest);
            await _db.SaveChangesAsync();

            // cresc numarul de imagini folosite de utilizator
            await _usageService.IncrementAsync(userId, 1);

            result.Id = extractionRequest.Id;

            // adaug rezultatul final in lista
            results.Add(result);
        }

        _logger.LogInformation("Procesare multipla finalizata pentru utilizatorul {UserId}. Reusita={Success}, Esuat={Failed}",
            userId,
            results.Count(r => r.Success),
            results.Count(r => !r.Success));

        return results;
    }

    // returnarea  istoricului tuturor extragerilor pentru utilizatorul curent
    public async Task<List<ExtractionRequest>> GetHistoryAsync(string userId)
    {
        var history = await _db.ExtractionRequests
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        _logger.LogInformation("Istoric pentru utilizatorul: {UserId}. Total extrageri={Count}", userId, history.Count);
        return history;
    }

    // returnez o extragere specifica dupa Id
    public async Task<ExtractionRequest?> GetByIdAsync(int id, string userId)
    {
        var request = await _db.ExtractionRequests
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (request == null)
            _logger.LogWarning("Extragerea cu Id={Id} nu a fost gasita pentru utilizatorul {UserId}.", id, userId);

        return request;
    }
}