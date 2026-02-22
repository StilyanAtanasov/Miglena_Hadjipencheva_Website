using MHAuthorWebsite.Core.Dtos.Seo;

namespace MHAuthorWebsite.Core.Contracts;

public interface ISitemapService
{
    Task<IReadOnlyCollection<SitemapUrlDto>> GetPublicSitemapUrlsAsync();
}
