using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Data.Models.Enums;

public enum AttributeDataType
{
    [Display(Name = "Текст")]
    Text = 0,

    [Display(Name = "Число")]
    Number = 1,

    [Display(Name = "Дата")]
    Date = 2,

    [Display(Name = "Да/Не")]
    Boolean = 3,

    // TODO 
    //    [Display(Name = "Падащо меню")]
    //    Dropdown = 4
}