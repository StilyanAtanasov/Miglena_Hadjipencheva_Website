using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Web.ViewModels.Shared.Enums;

public enum SearchBarAlignOptions
{
    [Display(Name = "left")]
    Left,

    [Display(Name = "center")]
    Center,

    [Display(Name = "right")]
    Right
}