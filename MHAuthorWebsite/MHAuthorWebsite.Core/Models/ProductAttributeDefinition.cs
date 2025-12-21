using MHAuthorWebsite.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductAttributeDefinition;

namespace MHAuthorWebsite.Core.Models;

public class ProductAttributeDefinition
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(KeyMaxLength)]
    public string Key { get; set; } = null!;

    [Required]
    [MaxLength(LabelMaxLength)]
    public string Label { get; set; } = null!;

    [Required]
    public AttributeDataType DataType { get; set; }

    public bool HasPredefinedValue { get; set; }

    public bool IsRequired { get; set; }

    [Required]
    [ForeignKey(nameof(ProductType))]
    public int ProductTypeId { get; set; }

    public ProductType ProductType { get; set; } = null!;

    public ICollection<ProductAttributeOption> ProductAttributeOptions { get; set; } =
        new HashSet<ProductAttributeOption>();
}
