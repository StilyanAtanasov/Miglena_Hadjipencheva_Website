using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Work;

namespace MHAuthorWebsite.Core.Contracts;

public interface IWorkService
{
    Task<ICollection<WorkCardDto>> GetPagedWorksAsync(bool isUserAdmin, int page, string? searchString = null);

    Task<int> GetWorksCountAsync(bool isUserAdmin, string? searchString = null);

    Task<ServiceResult<WorkDetailsDto>> GetWorkDetailsAsync(Guid id, bool isUserAdmin);
}
