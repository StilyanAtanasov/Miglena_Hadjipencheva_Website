using MHAuthorWebsite.Core.Admin.Contracts.DataServices;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.DataServices.Admin;

public class AdminWorkDataService : IAdminWorkDataService
{
    private readonly IApplicationRepository _repository;

    public AdminWorkDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<Work?> GetWorkForEditAsync(Guid id)
        => await _repository
            .All<Work>()
            .FirstOrDefaultAsync(w => w.Id == id);

    public async Task<Work?> GetWorkForEditReadonlyAsync(Guid id)
        => await _repository
            .AllReadonly<Work>()
            .FirstOrDefaultAsync(w => w.Id == id);
}