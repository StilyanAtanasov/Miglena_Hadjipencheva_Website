using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Web.Utils.Validation;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MustBeTrueAttribute : ValidationAttribute, IClientModelValidator
{
    public MustBeTrueAttribute()
    {
        ErrorMessage = "Това поле трябва да е отбелязано!";
    }

    public override bool IsValid(object? value)
    {
        return value is true;
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        AddAttribute(context.Attributes, "data-val", "true");
        AddAttribute(context.Attributes, "data-val-mustbetrue", ErrorMessage!);
    }

    private static void AddAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (!attributes.ContainsKey(key))
            attributes.Add(key, value);
    }
}
