using MHAuthorWebsite.Web.Common.Localization;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductDiscount;

namespace MHAuthorWebsite.Web.ViewModels.Admin.Product;

public class AddProductDiscountFormViewModel : IValidatableObject
{
    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    public decimal NewPrice { get; set; }

    public decimal CurrentPrice { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    public DateTime EndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NewPrice < NewPriceMinValue || NewPrice >= CurrentPrice)
        {
            yield return new ValidationResult(
                $"Новата цена трябва да бъде между {NewPriceMinValue} и {CurrentPrice - 0.01m}.",
                new[] { nameof(NewPrice) });
        }

        if (EndDate <= StartDate)
        {
            yield return new ValidationResult(
                "Крайната дата трябва да бъде след началната.",
                new[] { nameof(EndDate) });
        }

        if (StartDate < DateTime.Now.AddHours(-1))
        {
            yield return new ValidationResult(
                "Началната дата не може да бъде в миналото.",
                new[] { nameof(StartDate) });
        }
    }
}