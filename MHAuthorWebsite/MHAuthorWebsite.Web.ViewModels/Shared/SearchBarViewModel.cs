using MHAuthorWebsite.Web.ViewModels.Shared.Enums;

namespace MHAuthorWebsite.Web.ViewModels.Shared;

public class SearchBarViewModel
{
    public string FormCustomId { get; set; } = null!;

    public string InputCustomId { get; set; } = null!;

    public string PlaceholderText { get; set; } = "Търсете...";

    public string? DefaultValue { get; set; }

    public SearchBarAlignOptions Align { get; set; } = SearchBarAlignOptions.Center;

    public bool EnableRecaptchaV3 { get; set; }

    public string? RecaptchaV3SiteKey { get; set; }

    public string RecaptchaV3Action { get; set; } = "search_query";

    public string? RecaptchaV2SiteKey { get; set; }
}
