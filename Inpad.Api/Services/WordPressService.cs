using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Inpad.Api.Models;
using WordPressPCL;
using WordPressPCL.Models;

namespace Inpad.Api.Services;

public class WordPressService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _postType;
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public bool IsConfigured { get; }

    public WordPressService(IConfiguration config, HttpClient http)
    {
        _http = http;
        _baseUrl = (config["WordPress:Url"] ?? string.Empty).TrimEnd('/');
        _postType = config["WordPress:PostType"] ?? "objects";

        var username = config["WordPress:Username"] ?? string.Empty;
        var appPassword = config["WordPress:AppPassword"] ?? string.Empty;

        IsConfigured = !string.IsNullOrWhiteSpace(_baseUrl)
                    && !string.IsNullOrWhiteSpace(username)
                    && !string.IsNullOrWhiteSpace(appPassword);

        if (IsConfigured)
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{appPassword}"));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }
    }

    public async Task<int> PublishAsync(ArchObject obj, CancellationToken ct = default)
    {
        var payload = BuildPayload(obj, "publish");

        HttpResponseMessage response;

        if (obj.WordPressPostId.HasValue)
        {
            var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
            response = await _http.PostAsync($"{_baseUrl}/wp-json/wp/v2/{_postType}/{obj.WordPressPostId}", content, ct);
        }
        else
        {
            var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
            response = await _http.PostAsync($"{_baseUrl}/wp-json/wp/v2/{_postType}", content, ct);
        }

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("id").GetInt32();
    }

    public async Task UnpublishAsync(int wpPostId, CancellationToken ct = default)
    {
        var payload = new { status = "draft" };
        var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync($"{_baseUrl}/wp-json/wp/v2/{_postType}/{wpPostId}", content, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int wpPostId, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"{_baseUrl}/wp-json/wp/v2/{_postType}/{wpPostId}?force=true", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<int?> UploadMediaAsync(Stream fileStream, string fileName, string mimeType, CancellationToken ct = default)
    {
        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = fileName
        };

        var response = await _http.PostAsync($"{_baseUrl}/wp-json/wp/v2/media", content, ct);
        if (!response.IsSuccessStatusCode) return null;

        var body = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("id").GetInt32();
    }

    private static object BuildPayload(ArchObject obj, string status) => new
    {
        title = obj.Name,
        slug = obj.Slug,
        status,
        excerpt = obj.ShortDescription ?? string.Empty,
        content = obj.FullDescription ?? string.Empty,
        meta = new
        {
            inpad_city = obj.City ?? string.Empty,
            inpad_object_type = obj.ObjectType ?? string.Empty,
            inpad_year_start = obj.YearStart?.ToString() ?? string.Empty,
            inpad_year_end = obj.YearEnd?.ToString() ?? string.Empty,
            inpad_client = obj.Client ?? string.Empty,
            inpad_role = obj.InpadRole ?? string.Empty,
            inpad_seo_title = obj.SeoTitle ?? string.Empty,
            inpad_seo_description = obj.SeoDescription ?? string.Empty
        }
    };
}
