using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dto;

namespace MHAuthorWebsite.Core.Contracts;

public interface IEcontService
{
    Task<ServiceResult<EcontOrderDto>> PlaceOrderAsync(EcontOrderDto order);
}