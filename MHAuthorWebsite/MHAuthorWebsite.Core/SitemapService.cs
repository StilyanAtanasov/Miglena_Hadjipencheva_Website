using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Seo;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.Extensions.Logging;

namespace MHAuthorWebsite.Core;

public class SitemapService : ISitemapService
{
    private readonly IApplicationRepository _repository;
    private readonly ILogger<SitemapService> _logger;

    public SitemapService(IApplicationRepository repository, ILogger<SitemapService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<SitemapUrlDto>> GetPublicSitemapUrlsAsync()
    {
        SitemapUrlDto[] publicProducts = await _repository
            .WhereReadonly<Product>(p => p.IsPublic && !p.IsDeleted)
            .Select(product => new SitemapUrlDto
            {
                RelativeUrl = $"/Product/Details/{product.Id}",
                LastModifiedUtc = product.UpdatedOn == null
                    ? null
                    : DateTime.SpecifyKind(product.UpdatedOn.Value, DateTimeKind.Utc),
                ChangeFrequency = "weekly",
                Priority = 0.8m
            })
            .ToArrayAsync();

        SitemapUrlDto[] publicWorks = await _repository
            .WhereReadonly<Work>(w => w.IsPublic)
            .Select(work => new SitemapUrlDto
            {
                RelativeUrl = $"/Work/Details/{work.Id}",
                LastModifiedUtc = work.UpdatedOn == null
                    ? DateTime.SpecifyKind(work.DatePublished, DateTimeKind.Utc)
                    : DateTime.SpecifyKind(work.UpdatedOn.Value, DateTimeKind.Utc),
                ChangeFrequency = "weekly",
                Priority = 0.7m
            })
            .ToArrayAsync();

        List<SitemapUrlDto> urls = new()
        {
            new SitemapUrlDto { RelativeUrl = "/", ChangeFrequency = "daily", Priority = 1.0m },
            new SitemapUrlDto { RelativeUrl = "/Product/AllProducts", ChangeFrequency = "daily", Priority = 0.9m },
            new SitemapUrlDto { RelativeUrl = "/Work", ChangeFrequency = "weekly", Priority = 0.8m },
            new SitemapUrlDto { RelativeUrl = "/Contacts", ChangeFrequency = "monthly", Priority = 0.6m },
            new SitemapUrlDto { RelativeUrl = "/Home/PrivacyPolicy", ChangeFrequency = "monthly", Priority = 0.5m },
            new SitemapUrlDto { RelativeUrl = "/Home/TermsOfService", ChangeFrequency = "monthly", Priority = 0.5m }
        };

        urls.AddRange(publicProducts);
        urls.AddRange(publicWorks);

        _logger.LogInformation(
            "Prepared sitemap URLs. Static: {StaticCount}, Products: {ProductsCount}, Works: {WorksCount}",
            6, publicProducts.Length, publicWorks.Length);

        return urls;
    }
}
