using MHAuthorWebsite.Core.Contracts;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MHAuthorWebsite.Infrastructure.Caching;

public class RedisCacheService : IFastCacheService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };

    public RedisCacheService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _baseUrl = config["Redis:Upstash:RestUrl"]!.TrimEnd('/');

        string? token = config["Redis:Upstash:RestToken"];

        if (string.IsNullOrEmpty(_baseUrl)) throw new InvalidOperationException("Redis RestUrl is not configured.");
        if (string.IsNullOrEmpty(token)) throw new InvalidOperationException("Redis token is not configured.");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}/get/{key}");
        if (!response.IsSuccessStatusCode) return default;

        string jsonResponse = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
        JsonElement result = doc.RootElement.GetProperty("result");

        if (result.ValueKind == JsonValueKind.Null) return default;

        // CRITICAL: If Upstash returns a string, we must get the string content 
        // to remove the extra escaping/quotes before deserializing to T.
        string rawJson = result.ValueKind == JsonValueKind.String
            ? result.GetString()!
            : result.GetRawText();

        return JsonSerializer.Deserialize<T>(rawJson, JsonOptions);
    }

    public async Task<IEnumerable<T?>> GetBatchAsync<T>(IEnumerable<string> keys)
    {
        IEnumerable<string> enumerable = keys as string[] ?? keys.ToArray();
        if (!enumerable.Any()) return Enumerable.Empty<T?>();

        List<string> command = new List<string> { "MGET" };
        command.AddRange(enumerable);

        string jsonCommand = JsonSerializer.Serialize(command);
        StringContent content = new StringContent(jsonCommand, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(_baseUrl, content);
        if (!response.IsSuccessStatusCode) return enumerable.Select(_ => default(T));

        string jsonResponse = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
        List<T?> resultList = doc.RootElement.GetProperty("result").EnumerateArray()
            .Select(v =>
            {
                if (v.ValueKind == JsonValueKind.Null) return default(T);

                string rawJson = v.ValueKind == JsonValueKind.String
                    ? v.GetString()!
                    : v.GetRawText();

                return JsonSerializer.Deserialize<T>(rawJson, JsonOptions);
            })
            .ToList();

        return resultList;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        string jsonValue = JsonSerializer.Serialize(value, JsonOptions);
        object[] command = { "SET", key, jsonValue, "EX", (int)ttl.TotalSeconds };
        await SendCommandAsync(command);
    }

    public async Task SetBatchAsync<T>(IDictionary<string, T> values, TimeSpan ttl, bool fireAndForget = false)
    {
        // For batches via REST, we use a Pipeline
        IEnumerable<object> pipeline = values.Select(kv => new object[]
        {
            "SET", kv.Key, JsonSerializer.Serialize(kv.Value, JsonOptions), "EX", (int)ttl.TotalSeconds
        });

        string json = JsonSerializer.Serialize(pipeline);
        await _httpClient.PostAsync($"{_baseUrl}/pipeline", new StringContent(json, Encoding.UTF8, "application/json"));
    }

    public async Task RemoveAsync(string key) => await _httpClient.GetAsync($"{_baseUrl}/del/{key}");

    public void SetFireAndForget<T>(string key, T value, TimeSpan ttl)
    {
        string jsonValue = JsonSerializer.Serialize(value, JsonOptions);
        object[] command = { "SET", key, jsonValue, "EX", (int)ttl.TotalSeconds };
        string json = JsonSerializer.Serialize(command);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // WE DO NOT AWAIT THIS. (A bit of a simulated 'Fire and Forget' through UDP)
        _ = _httpClient.PostAsync(_baseUrl, content);
    }

    private async Task SendCommandAsync(object[] command)
    {
        string json = JsonSerializer.Serialize(command);
        await _httpClient.PostAsync(_baseUrl, new StringContent(json, Encoding.UTF8, "application/json"));
    }
}