using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Data.Models.Enums;

public enum ShipmentEventSource
{
    [Display(Name = "Система")]
    System = 0,

    [Display(Name = "Еконт")]
    Econt = 1
}