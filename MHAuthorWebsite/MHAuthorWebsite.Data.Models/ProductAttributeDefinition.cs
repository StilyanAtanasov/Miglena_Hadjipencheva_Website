using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductAttributeDefinition;

namespace MHAuthorWebsite.Data.Models;

public class ProductAttributeDefinition
{
    [Key]
    [Comment("Primary key")]
    public int Id { get; set; }

    [Required]
    [MaxLength(KeyMaxLength)]
    [Comment("Attribute key (e.g., ISBN)")]
    public string Key { get; set; } = null!;

    [Required]
    [MaxLength(LabelMaxLength)]
    [Comment("Display label for the attribute")]
    public string Label { get; set; } = null!;

    [Required]
    [MaxLength(DataTypeMaxLength)]
    [Comment("UI data type (e.g., number, date)")]
    public string DataType { get; set; } = null!;

    [Comment("Indicates if the attribute can choose from options")]
    public bool HasPredefinedValue { get; set; }

    [Comment("Indicates if the attribute is mandatory")]
    public bool IsRequired { get; set; }

    [Required]
    [Comment("Foreign key to ProductType")]
    [ForeignKey(nameof(ProductType))]
    public int ProductTypeId { get; set; }

    public ProductType ProductType { get; set; } = null!;

    public ICollection<ProductAttributeOption> ProductAttributeOptions { get; set; } =
        new HashSet<ProductAttributeOption>();
}
