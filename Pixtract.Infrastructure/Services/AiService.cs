using Microsoft.Extensions.Logging;
using Pixtract.Application.DTOs;
using Pixtract.Application.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Pixtract.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AiService> _logger;

    public AiService(IHttpClientFactory httpClientFactory, ILogger<AiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ExtractionResultDto> ExtractAsync(
        byte[] imageBytes,
        string fileName,
        string contentType,
        string category,
        string productId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PythonApi");

            using var content = new MultipartFormDataContent();

            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(imageContent, "file", fileName);
            content.Add(new StringContent(category), "categorie");
            content.Add(new StringContent(productId), "product_id");

            _logger.LogInformation("Cerere trimisa catre Python API. Produs={ProductId}, Categorie={Category}, Fisier={FileName}, Dimensiune={Size}KB",
                productId, category, fileName, imageBytes.Length / 1024);

            var response = await client.PostAsync("/extract", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Python API a returnat un raspuns neasteptat. StatusCode={StatusCode}, Produs={ProductId}",
                    response.StatusCode, productId);
                return new ExtractionResultDto { Success = false, Error = $"Eroare API: {response.StatusCode}" };
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ExtractionResultDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Success == true)
                _logger.LogInformation("Raspuns valid primit de la Python API. Produs={ProductId}, Atribute extrase={Count}",
                    productId, result.Attributes?.Count ?? 0);
            else
                _logger.LogWarning("Python API a returnat un rezultat negativ. Produs={ProductId}, Eroare={Error}",
                    productId, result?.Error);

            return result ?? new ExtractionResultDto { Success = false, Error = "Raspuns invalid de la API." };
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Python API nu a raspuns in timpul alocat. Produs={ProductId}, Fisier={FileName}",
                productId, fileName);
            return new ExtractionResultDto { Success = false, Error = "Modelul nu a raspuns in timp util." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare neasteptata la apelul Python API. Produs={ProductId}, Fisier={FileName}",
                productId, fileName);
            return new ExtractionResultDto { Success = false, Error = "Eroare neasteptata." };
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PythonApi");
            var response = await client.GetAsync("/health");
            var available = response.IsSuccessStatusCode;
            _logger.LogInformation("Verificare disponibilitate Python API. Disponibil={Available}", available);
            return available;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Python API nu este disponibil. Verificare health check esuata.");
            return false;
        }
    }
}