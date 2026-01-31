using MHAuthorWebsite.Web.ViewModels.Shared.Enums;

namespace MHAuthorWebsite.Web.ViewModels.Shared;

public class SearchBarViewModel
{
    public string FormCustomId { get; set; } = null!;

    public string InputCustomId { get; set; } = null!;

    public string PlaceholderText { get; set; } = "Търсете...";

    public string? DefaultValue { get; set; }

    public SearchBarAlignOptions Align { get; set; } = SearchBarAlignOptions.Center;
}