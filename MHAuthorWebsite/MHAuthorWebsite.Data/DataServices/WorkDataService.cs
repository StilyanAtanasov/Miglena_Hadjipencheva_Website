using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.DataServices;

public class WorkDataService : IWorkDataService
{
    private readonly IApplicationRepository _repository;

    public WorkDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<Work?> GetWorkByIdAsync(Guid id, bool includeHidden = false)
    {
        IQueryable<Work> works = _repository.AllReadonly<Work>();

        if (!includeHidden) works = works.Where(w => w.IsPublic);

        return await works.FirstOrDefaultAsync(w => w.Id == id);
    }
}
