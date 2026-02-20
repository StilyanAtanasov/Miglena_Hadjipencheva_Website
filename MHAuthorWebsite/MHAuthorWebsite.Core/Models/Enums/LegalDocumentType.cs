using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Core.Models.Enums;

public enum LegalDocumentType
{
    [Display(Name = "Политика за поверителност")]
    PrivacyPolicy = 1,

    [Display(Name = "Общи условия")]
    TermsOfService = 2
}
