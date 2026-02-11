using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Admin.Contracts.DataServices;

public interface IAdminWorkDataService
{
    Task<Work?> GetWorkForEditReadonlyAsync(Guid id);

    Task<Work?> GetWorkForEditAsync(Guid id);
}