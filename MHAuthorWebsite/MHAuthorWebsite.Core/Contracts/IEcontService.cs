using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Order;

namespace MHAuthorWebsite.Core.Contracts;

public interface IEcontService
{
    Task<ServiceResult<EcontOrderDto>> UpdateOrderAsync(EcontOrderDto order);

    Task<ServiceResult<EcontShipmentStatusDto>> GetTrackingInfo(EcontOrderDto order);
}