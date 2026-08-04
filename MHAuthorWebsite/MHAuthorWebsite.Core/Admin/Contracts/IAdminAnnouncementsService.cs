using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.Announcements;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminAnnouncementsService
{
    Task<ICollection<AnnouncementListItemDto>> GetAnnouncementsPagedReadonlyAsync(int page, string? search = null);

    Task<int> GetAnnouncementsCountReadonlyAsync(string? search = null);

    Task<ServiceResult<AnnouncementDetailsDto>> GetAnnouncementDetailsReadonlyAsync(Guid id);

    Task<ServiceResult> CreateAnnouncementAsync(CreateAnnouncementDto model, string adminId);
}
