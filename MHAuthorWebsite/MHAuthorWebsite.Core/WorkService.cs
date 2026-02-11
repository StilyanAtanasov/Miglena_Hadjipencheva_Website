using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Dtos.Work;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Linq.Expressions;
using System.Text.Json;
using static MHAuthorWebsite.Core.Common.Extensions.ExpressionExtensions;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheDefaultDurations;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using static MHAuthorWebsite.GCommon.ApplicationRules.Pagination;

namespace MHAuthorWebsite.Core;

public class WorkService : IWorkService
{
    private readonly IWorkDataService _dataService;
    private readonly IApplicationRepository _repository;
    private readonly IFastCacheService _cache;
    private readonly ILogger<WorkService> _logger;

    public WorkService(IWorkDataService dataService, IApplicationRepository repository, IFastCacheService cache, ILogger<WorkService> logger)
    {
        _dataService = dataService;
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ICollection<WorkCardDto>> GetPagedWorksAsync(bool isUserAdmin, int page, string? searchString = null)
    {
        Expression<Func<Work, bool>>? filter = null;

        if (searchString != null) filter = w => w.Title.Contains(searchString);
        if (!isUserAdmin)
        {
            Expression<Func<Work, bool>> publicFilter = w => w.IsPublic;
            filter = filter == null ? publicFilter : filter.AndAlso(publicFilter);
        }

        Guid[] pagedWorksIds = await _repository
            .GetPagedAsync(page, PageSize, true, filter)
            .Select(w => w.Id)
            .ToArrayAsync();

        if (!pagedWorksIds.Any()) return Array.Empty<WorkCardDto>();

        ICollection<WorkCardDto> works = await GetWorkCardsBatchAsync(pagedWorksIds);

        _logger.LogInformation("Retrieved {Count} paged works. Page: {Page}, Search: '{SearchString}', IncludePrivate: {IncludePrivate}",
            works.Count, page, searchString, isUserAdmin);

        return works;
    }

    public async Task<int> GetWorksCountAsync(bool isUserAdmin, string? searchString = null)
    {
        IQueryable<Work> works = _repository.AllReadonly<Work>();

        if (!string.IsNullOrWhiteSpace(searchString))
            works = works.Where(w => w.Title.Contains(searchString));

        if (!isUserAdmin) works = works.Where(w => w.IsPublic);

        int count = await works.CountAsync();
        _logger.LogInformation("Counted {Count} works. Search: '{SearchString}', IncludePrivate: {IncludePrivate}",
            count, searchString, isUserAdmin);

        return count;
    }

    public async Task<ServiceResult<WorkDetailsDto>> GetWorkDetailsAsync(Guid id, bool isUserAdmin)
    {
        try
        {
            WorkDetailsDto? cachedWork = await _cache.GetAsync<WorkDetailsDto>(WorkDetailsKey(id));

            if (cachedWork != null)
            {
                if (!cachedWork.IsPublic && !isUserAdmin)
                {
                    _logger.LogInformation("Work found but not accessible. WorkId: {WorkId}", id);
                    return ServiceResult<WorkDetailsDto>.NotFound();
                }

                _logger.LogInformation("Retrieved work details from cache. WorkId: {WorkId}", id);
                return ServiceResult<WorkDetailsDto>.Ok(cachedWork);
            }

            Work? work = await _dataService.GetWorkByIdAsync(id, isUserAdmin);

            if (work == null)
            {
                _logger.LogInformation("Work not found. WorkId: {WorkId}. Is User Admin: {IsUserAdmin}", id, isUserAdmin);
                return ServiceResult<WorkDetailsDto>.NotFound();
            }

            WorkDetailsDto workDetails = new()
            {
                Id = work.Id,
                Title = work.Title,
                Content = work.Content,
                CoverImageUrl = work.CoverImageUrl,
                DatePublished = work.DatePublished
            };

            _cache.SetFireAndForget(WorkDetailsKey(id), workDetails, TimeSpan.FromDays(WorkDetailsTtlDays));

            _logger.LogInformation("Retrieved work details from database and cached. WorkId: {WorkId}", id);
            return ServiceResult<WorkDetailsDto>.Ok(workDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work details. WorkId: {WorkId}", id);
            return ServiceResult<WorkDetailsDto>.Failure();
        }
    }

    private async Task<ICollection<WorkCardDto>> GetWorkCardsBatchAsync(Guid[] workIds)
    {
        RedisKey[] keys = workIds
            .Select(id => (RedisKey)WorkCardKey(id))
            .ToArray();

        IBatch batch = _cache.CreateBatch();
        Task<RedisValue>[] tasks = keys.Select(key => batch.StringGetAsync(key)).ToArray();
        batch.Execute();

        RedisValue[] cachedValues = await Task.WhenAll(tasks);
        List<WorkCardDto> cachedWorks = new();
        List<Guid> missingIds = new();

        for (int i = 0; i < workIds.Length; i++)
        {
            if (cachedValues[i].HasValue)
            {
                WorkCardDto? work = JsonSerializer.Deserialize<WorkCardDto>(cachedValues[i].ToString());
                if (work != null) cachedWorks.Add(work);
            }
            else missingIds.Add(workIds[i]);
        }

        if (missingIds.Any())
        {
            WorkCardDto[] dbItems = await _repository
                .WhereReadonly<Work>(p => missingIds.Contains(p.Id))
                .Select(p => new WorkCardDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    CoverImageUrl = p.CoverImageUrl,
                    IsPublic = p.IsPublic
                })
                 .ToArrayAsync();

            IBatch writeBatch = _cache.CreateBatch();
            foreach (WorkCardDto item in dbItems)
            {
                WorkCardDto dto = new()
                {
                    Id = item.Id,
                    Title = item.Title,
                    CoverImageUrl = item.CoverImageUrl,
                    IsPublic = item.IsPublic
                };

                string json = JsonSerializer.Serialize(dto);
                await writeBatch.StringSetAsync((RedisKey)WorkCardKey(dto.Id), (RedisValue)json, TimeSpan.FromDays(WorkCardTtlDays), flags: CommandFlags.FireAndForget);
                cachedWorks.Add(dto);
            }

            writeBatch.Execute();
        }

        return cachedWorks;
    }
}
