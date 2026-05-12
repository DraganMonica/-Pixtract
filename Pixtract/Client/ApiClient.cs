
using Pixtract.Application.DTOs;
using Pixtract.Web.VMs;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Pixtract.Web.Client;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string TokenKey = "jwt_token";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    // TOKEN
    private void AttachToken()
    {
        // incearca mai intai session
        var token = _httpContextAccessor.HttpContext?.Session.GetString(TokenKey);

        // Fallback: citeste din claims ul cookie ului de autentificare
        if (string.IsNullOrEmpty(token))
            token = _httpContextAccessor.HttpContext?.User.FindFirstValue("jwt_token");

        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }

    public void SaveToken(string token)
        => _httpContextAccessor.HttpContext?.Session.SetString(TokenKey, token);

    public void ClearToken()
        => _httpContextAccessor.HttpContext?.Session.Remove(TokenKey);

    public bool IsLoggedIn()
        => !string.IsNullOrEmpty(
            _httpContextAccessor.HttpContext?.Session.GetString(TokenKey));

    //AUTH
    public async Task<(bool Success, string Error)> RegisterAsync(RegisterDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var response = await _httpClient.PostAsync("api/auth/register",
            new StringContent(json, Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
            return (true, string.Empty);

        var body = await response.Content.ReadAsStringAsync();
        return (false, body);
    }

    public async Task<(bool Success, string Error)> LoginAsync(string email, string password)
    {
        var payload = JsonSerializer.Serialize(new { email, password });
        var response = await _httpClient.PostAsync("api/auth/login",
            new StringContent(payload, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
            return (false, "Email sau parola incorecte.");

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var token = doc.RootElement.GetProperty("token").GetString();

        SaveToken(token!);
        return (true, string.Empty);
    }

    //  EXTRACTION 
    public async Task<List<ExtractionResultDto>?> ExtractMultipleAsync(List<IFormFile> files, string category)
    {
        AttachToken();

        using var content = new MultipartFormDataContent();
        var streams = new List<Stream>();

        foreach (var file in files)
        {
            var stream = file.OpenReadStream();
            streams.Add(stream);
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "files", file.FileName);
        }
        content.Add(new StringContent(category), "category");

        var response = await _httpClient.PostAsync("api/extraction", content);

        foreach (var s in streams) s.Dispose();

        if (!response.IsSuccessStatusCode) return null;

        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<ExtractionResultDto>>(body, JsonOptions);
    }

    public async Task<List<ExtractionDto>> GetHistoryAsync()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("api/extraction/history");
        if (!response.IsSuccessStatusCode) return new();

        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<ExtractionDto>>(body, JsonOptions) ?? new();
    }

    // DASHBOARD 
    public async Task<DashboardVM?> GetDashboardAsync()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("api/dashboard");
        if (!response.IsSuccessStatusCode) return null;

        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DashboardVM>(body, JsonOptions);
    }

    // ADMIN
    public async Task<AdminVM?> GetAdminDataAsync()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("api/admin");
        if (!response.IsSuccessStatusCode) return null;

        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AdminVM>(body, JsonOptions);
    }

    //PLANS
    public async Task<List<PlanDto>> GetAllPlansAsync()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("api/plans");
        if (!response.IsSuccessStatusCode) return new();

        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<PlanDto>>(body, JsonOptions) ?? new();
    }

    public async Task<(string PlanName, int? DaysRemaining, int PlanId)> GetUserPlanInfoAsync()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("api/plans/user");
        if (!response.IsSuccessStatusCode) return ("Free", null, 1);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var planName = doc.RootElement.GetProperty("planName").GetString() ?? "Free";
        var planId = doc.RootElement.GetProperty("planId").GetInt32();
        int? daysRemaining = null;
        if (doc.RootElement.TryGetProperty("daysRemaining", out var dr) && dr.ValueKind != JsonValueKind.Null)
            daysRemaining = dr.GetInt32();

        return (planName, daysRemaining, planId);
    }

    // SUBSCRIPTION 
    public async Task<string?> UpgradePlanAsync(int planId)
    {
        AttachToken();
        var response = await _httpClient.PostAsync("api/subscription/upgrade",
            new StringContent(planId.ToString(), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode) return null;

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("checkoutUrl").GetString();
    }

    public async Task<(bool Success, string Error)> ConfirmUpgradeAsync(string sessionId, int planId)
    {
        AttachToken();
        var payload = JsonSerializer.Serialize(new { sessionId, planId });
        var response = await _httpClient.PostAsync("api/subscription/confirm",
            new StringContent(payload, Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
            return (true, string.Empty);

        return (false, "Eroare la actualizarea planului.");
    }

    // EXPORT 
    public async Task<byte[]?> ExportCurrentAsync(int id)
    {
        AttachToken();
        var response = await _httpClient.GetAsync($"api/export/current/{id}");
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<byte[]?> ExportHistoryAsync()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("api/export/history");
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadAsByteArrayAsync();
    }

    // USAGE 
    public async Task<(int TodayUsage, bool CanProcess, int ImagesPerRequest)> GetUsageAsync()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("api/usage");
        if (!response.IsSuccessStatusCode) return (0, false, 1);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var usage = doc.RootElement.GetProperty("todayUsage").GetInt32();
        var canProcess = doc.RootElement.GetProperty("canProcess").GetBoolean();
        var imagesPerRequest = doc.RootElement.GetProperty("imagesPerRequest").GetInt32();

        return (usage, canProcess, imagesPerRequest);
    }
}