using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Core.Models.Enums;

public enum ProductAttributeDisplayPosition
{
    [Display(Name = "Таблица за допълнителни данни")]
    AdditionalInfoTable = 0,

    [Display(Name = "Основна информация")]
    MainInfo = 1,
}