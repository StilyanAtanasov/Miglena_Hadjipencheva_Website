using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dto;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using static MHAuthorWebsite.GCommon.ApplicationRules.Econt;

namespace MHAuthorWebsite.Core.Admin;

public class AdminEcontService : EcontService, IAdminEcontService
{
    public AdminEcontService(HttpClient http, IConfiguration config) : base(http, config) { }

    public async Task<ServiceResult<EcontShipmentStatusDto>> CreateAwbAsync(EcontOrderDto order)
    {
        HttpResponseMessage response = await SendRequestAsync(CreateAwbEndpoint, order);
        if (!response.IsSuccessStatusCode) return ServiceResult<EcontShipmentStatusDto>.Failure();

        string responseJson = await response.Content.ReadAsStringAsync();
        EcontShipmentStatusDto responseDto = JsonSerializer.Deserialize<EcontShipmentStatusDto>(responseJson, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        })!;

        return ServiceResult<EcontShipmentStatusDto>.Ok(responseDto);
    }

    public async Task<ServiceResult> DeleteLabelAsync(EcontOrderDto order)
    {
        HttpResponseMessage response = await SendRequestAsync(DeleteLabelEndpoint, order);
        if (!response.IsSuccessStatusCode) return ServiceResult<EcontOrderDto>.Failure();

        return ServiceResult.Ok();
    }
}