using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminEcontService : IEcontService
{
    Task<ServiceResult<EcontShipmentStatusDto>> CreateAwbAsync(EcontOrderDto order);

    Task<ServiceResult> DeleteLabelAsync(EcontOrderDto order);
}