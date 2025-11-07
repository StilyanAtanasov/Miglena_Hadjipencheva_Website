using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Web.ViewModels.Contacts;

namespace MHAuthorWebsite.Core.Contracts;

public interface IContactsService
{
    Task<ServiceResult> SendContactMessageAsync(ContactFormViewModel model, string? userId);
}