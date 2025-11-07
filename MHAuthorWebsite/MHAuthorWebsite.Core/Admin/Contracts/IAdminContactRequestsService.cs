using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Web.ViewModels.Admin.ContactRequests;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminContactRequestsService
{
    Task<ICollection<ContactRequestCardViewModel>> GetContactRequestsPagedReadonlyAsync(int page);

    Task<ServiceResult<ContactRequestDetailsViewModel>> GetContactRequestDetailsReadonlyAsync(Guid requestId);

    Task<ServiceResult> ReplyToContactRequestAsync(Guid requestId, string replyMessage, string adminId);
}