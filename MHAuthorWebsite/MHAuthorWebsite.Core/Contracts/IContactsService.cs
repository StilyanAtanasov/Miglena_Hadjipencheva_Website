using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Contacts;

namespace MHAuthorWebsite.Core.Contracts;

public interface IContactsService
{
    Task<ServiceResult> SendContactMessageAsync(SendContactMessageDto model, string? userId);
}