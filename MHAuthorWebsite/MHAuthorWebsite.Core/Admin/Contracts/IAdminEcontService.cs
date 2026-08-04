using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Order;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminEcontService : IEcontService
{
    Task<ServiceResult<EcontShipmentStatusDto>> CreateAwbAsync(EcontOrderDto order);

    Task<ServiceResult> DeleteLabelAsync(EcontOrderDto order);
}