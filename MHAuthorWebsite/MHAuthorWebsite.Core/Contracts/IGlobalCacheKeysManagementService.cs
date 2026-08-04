namespace MHAuthorWebsite.Core.Contracts;

public interface IGlobalCacheKeysManagementService
{
    Task<ICollection<string>> GetAllAdminIdsAsync();

    Task<Guid> DiscountsGlobalStateId();
}