using MHAuthorWebsite.Core.Admin.Dto;
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
    protected readonly HttpClient Http;
    protected readonly IConfiguration Config;

    public EcontService(HttpClient http, IConfiguration config)
    {
        Http = http;
        Config = config;
    }

    public async Task<ServiceResult<EcontOrderDto>> UpdateOrderAsync(EcontOrderDto order)
    {
        HttpResponseMessage response = await SendRequestAsync(UpdateOrderEndpoint, order);
        string responseJson = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return ServiceResult<EcontOrderDto>.Failure();

        EcontOrderDto responseDto = JsonSerializer.Deserialize<EcontOrderDto>(responseJson, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        })!;

        return ServiceResult<EcontOrderDto>.Ok(responseDto);
    }

    public async Task<ServiceResult<EcontShipmentStatusDto>> GetTrackingInfo(EcontOrderDto order)
    {
        HttpResponseMessage response = await SendRequestAsync(GetTraceEndpoint, order);
        string responseJson = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return ServiceResult<EcontShipmentStatusDto>.Failure();

        EcontShipmentStatusDto responseDto = JsonSerializer.Deserialize<EcontShipmentStatusDto>(responseJson, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        })!;

        return ServiceResult<EcontShipmentStatusDto>.Ok(responseDto);
    }

    protected async Task<HttpResponseMessage> SendRequestAsync(string endpoint, object payload)
    {
        string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        });

        using HttpRequestMessage request = new(HttpMethod.Post, endpoint);
        request.Headers.TryAddWithoutValidation("Authorization", Config["EcontApiSecret"]!);
        request.Headers.Add("X-ID-Shop", ShopId.ToString());
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await Http.SendAsync(request);

        return response;
    }
}