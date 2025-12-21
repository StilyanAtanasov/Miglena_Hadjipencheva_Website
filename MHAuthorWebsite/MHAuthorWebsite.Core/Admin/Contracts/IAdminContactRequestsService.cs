using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.ContactRequests;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminContactRequestsService
{
    Task<ICollection<ContactRequestCardDto>> GetContactRequestsPagedReadonlyAsync(int page);

    Task<ServiceResult<ContactRequestDetailsDto>> GetContactRequestDetailsReadonlyAsync(Guid requestId);

    Task<ServiceResult> ReplyToContactRequestAsync(Guid requestId, string replyMessage, string adminId);
}