using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Data.Models.Enums;

public enum OrderStatus
{
    [Display(Name = "Обработва се")]
    InReview = 0,

    [Display(Name = "Приета")]
    Accepted = 1,

    [Display(Name = "Отказана")]
    Rejected = 2,

    [Display(Name = "Изпратена")]
    Shipped = 3,

    [Display(Name = "Прекратена")]
    Terminated = 4,

    [Display(Name = "Доставена")]
    Delivered = 5
}