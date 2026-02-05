using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Configuration.EcontApi;
using MHAuthorWebsite.Core.Dtos.Order;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using static MHAuthorWebsite.GCommon.ApplicationRules.Econt;

namespace MHAuthorWebsite.Core.Admin;

public class AdminEcontService : EcontService, IAdminEcontService
{
    private readonly ILogger<AdminEcontService> _logger;

    public AdminEcontService(HttpClient http, IOptions<EcontApiSettings> econtSettings, ILogger<AdminEcontService> logger, ILogger<EcontService> baseLogger)
        : base(http, econtSettings, baseLogger)
    {
        _logger = logger;
    }

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

        _logger.LogInformation("Successfully created AWB for Econt order with Order number {OrderNumber}", order.OrderNumber);
        return ServiceResult<EcontShipmentStatusDto>.Ok(responseDto);
    }

    public async Task<ServiceResult> DeleteLabelAsync(EcontOrderDto order)
    {
        HttpResponseMessage response = await SendRequestAsync(DeleteLabelEndpoint, order);
        if (!response.IsSuccessStatusCode) return ServiceResult<EcontOrderDto>.Failure();

        _logger.LogInformation("Successfully deleted Econt label for order with Order number {OrderNumber}", order.OrderNumber);
        return ServiceResult.Ok();
    }
}