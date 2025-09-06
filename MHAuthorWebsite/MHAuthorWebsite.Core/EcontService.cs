using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using static MHAuthorWebsite.GCommon.ApplicationRules.Econt;

namespace MHAuthorWebsite.Core;

public class EcontService : IEcontService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public EcontService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<ServiceResult<EcontOrderDto>> PlaceOrderAsync(EcontOrderDto order)
    {
        string url = UpdateOrderEndpoint;
        string privateKey = _config["EcontApiSecret"]!;
        int shopId = ShopId;

        string json = JsonSerializer.Serialize(order, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        });

        using HttpRequestMessage request = new(HttpMethod.Post, url);
        request.Headers.TryAddWithoutValidation("Authorization", privateKey);
        request.Headers.Add("X-ID-Shop", shopId.ToString());
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode) return ServiceResult<EcontOrderDto>.Failure();

        string responseJson = await response.Content.ReadAsStringAsync();
        EcontOrderDto responseDto = JsonSerializer.Deserialize<EcontOrderDto>(responseJson, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        })!;

        return ServiceResult<EcontOrderDto>.Ok(responseDto);
    }
}