using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Error;

namespace MHAuthorWebsite.Core.Contracts;

public interface IErrorService
{
    Task<ServiceResult> HandleErrorAsync(HandleErrorDto errorDto);
}