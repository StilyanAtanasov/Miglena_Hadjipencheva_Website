using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Contracts.DataServices;

public interface IWorkDataService
{
    Task<Work?> GetWorkByIdAsync(Guid id, bool includeHidden = false);
}
