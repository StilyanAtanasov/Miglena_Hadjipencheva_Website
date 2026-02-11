using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Contracts.DataServices;
using MHAuthorWebsite.Core.Admin.Dto.Work;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Images;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.Extensions.Logging;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductImage;

namespace MHAuthorWebsite.Core.Admin;

public class AdminWorkService : IAdminWorkService
{
    private readonly IAdminWorkDataService _dataService;
    private readonly IApplicationRepository _repository;
    private readonly IAdminProductImageService _imageService;
    private readonly IFastCacheService _cache;
    private readonly ILogger<AdminWorkService> _logger;

    public AdminWorkService(IAdminWorkDataService dataService, IApplicationRepository repository,
        IAdminProductImageService imageService, IFastCacheService cache, ILogger<AdminWorkService> logger)
    {
        _dataService = dataService;
        _repository = repository;
        _imageService = imageService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ServiceResult> AddWorkAsync(AddWorkDto model)
    {
        try
        {
            ServiceResult<ICollection<ImageUploadResultDto>> uploadResult = await _imageService
                .UploadImagesAsync(new[] { model.CoverImage }, "Works", OriginalWidth);

            if (!uploadResult.Success || uploadResult.Result == null || uploadResult.Result.Count == 0)
                return ServiceResult.Failure();

            ImageUploadResultDto image = uploadResult.Result.First();

            Work work = new()
            {
                Title = model.Title,
                Content = model.Content,
                CoverImageUrl = image.ImageUrl,
                CoverImagePublicId = image.PublicId,
                IsPublic = model.IsPublic,
                DatePublished = DateTime.UtcNow
            };

            await _repository.AddAsync(work);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Admin added new work: {Title}", model.Title);
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding new work: {Title}", model.Title);
            return ServiceResult.Failure();
        }
    }

    public async Task<ServiceResult<EditWorkDto>> GetWorkForEditReadonlyAsync(Guid id)
    {
        Work? work = await _dataService.GetWorkForEditReadonlyAsync(id);

        if (work == null) return ServiceResult<EditWorkDto>.NotFound();

        return ServiceResult<EditWorkDto>.Ok(new EditWorkDto
        {
            Id = work.Id,
            Title = work.Title,
            Content = work.Content,
            CurrentCoverImageUrl = work.CoverImageUrl,
            IsPublic = work.IsPublic
        });
    }

    public async Task<ServiceResult> UpdateWorkAsync(EditWorkDto model)
    {
        try
        {
            Work? work = await _dataService.GetWorkForEditAsync(model.Id);

            if (work == null) return ServiceResult.NotFound();

            work.Title = model.Title;
            work.Content = model.Content;
            work.IsPublic = model.IsPublic;

            if (model.NewCoverImage != null)
            {
                ServiceResult<ICollection<ImageUploadResultDto>> uploadResult = await _imageService
                    .UploadImagesAsync(new[] { model.NewCoverImage }, "Works", OriginalWidth);

                if (uploadResult is { Success: true, Result.Count: > 0 })
                {
                    await _imageService.DeleteImageAsync(work.CoverImagePublicId);

                    ImageUploadResultDto newImage = uploadResult.Result.First();
                    work.CoverImageUrl = newImage.ImageUrl;
                    work.CoverImagePublicId = newImage.PublicId;
                }
            }

            await _repository.SaveChangesAsync();

            await _cache.RemoveAsync(WorkDetailsKey(work.Id));
            await _cache.RemoveAsync(WorkCardKey(work.Id));

            _logger.LogInformation("Admin updated work: {Title}", model.Title);
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating work: {Title}", model.Title);
            return ServiceResult.Failure();
        }
    }

    public async Task<ServiceResult> DeleteWorkAsync(Guid id)
    {
        try
        {
            Work? work = await _dataService.GetWorkForEditAsync(id);

            if (work == null) return ServiceResult.NotFound();

            await _imageService.DeleteImageAsync(work.CoverImagePublicId);

            _repository.Delete(work);
            await _repository.SaveChangesAsync();

            await _cache.RemoveAsync(WorkDetailsKey(work.Id));
            await _cache.RemoveAsync(WorkCardKey(work.Id));

            _logger.LogInformation("Admin deleted work: {Title}", work.Title);
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting work with ID: {Id}", id);
            return ServiceResult.Failure();
        }
    }

    public async Task<ServiceResult> ToggleWorkPublicityAsync(Guid id)
    {
        try
        {
            Work? work = await _dataService.GetWorkForEditAsync(id);

            if (work == null) return ServiceResult.NotFound();

            work.IsPublic = !work.IsPublic;
            await _repository.SaveChangesAsync();

            await _cache.RemoveAsync(WorkDetailsKey(work.Id));
            await _cache.RemoveAsync(WorkCardKey(work.Id));

            _logger.LogInformation("Toggled publicity for work: {Title}. New status: {Status}", work.Title, work.IsPublic);
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling publicity for work with ID: {Id}", id);
            return ServiceResult.Failure();
        }
    }
}
