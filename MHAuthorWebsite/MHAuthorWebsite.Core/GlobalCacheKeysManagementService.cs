using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using Microsoft.AspNetCore.Identity;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core;

public class GlobalCacheKeysManagementService : IGlobalCacheKeysManagementService
{
    protected readonly IFastCacheService Cache;
    protected readonly IApplicationRepository Repository;
    protected readonly UserManager<ApplicationUser> UserManager;

    public GlobalCacheKeysManagementService(IFastCacheService cacheService, IApplicationRepository repository, UserManager<ApplicationUser> userManager)
    {
        Cache = cacheService;
        Repository = repository;
        UserManager = userManager;
    }

    public async Task<ICollection<string>> GetAllAdminIdsAsync()
    {
        string[]? adminIds = await Cache.GetAsync<string[]>(AdminIdsKey());
        if (adminIds is null)
        {
            IList<ApplicationUser> admins = await UserManager.GetUsersInRoleAsync(AdminRoleName);
            adminIds = admins.Select(u => u.Id).Distinct().ToArray();

            Cache.SetFireAndForget(AdminIdsKey(), adminIds, TimeSpan.FromDays(5));
        }

        return adminIds;
    }

    public async Task<Guid> DiscountsGlobalStateId()
    {
        Guid? state = await Cache.GetAsync<Guid?>(GlobalDiscountsStateIdKey());
        if (state is null)
        {
            state = Guid.NewGuid();
            Cache.SetFireAndForget(GlobalDiscountsStateIdKey(), state, TimeSpan.FromDays(5));
        }

        return state.Value;
    }
}