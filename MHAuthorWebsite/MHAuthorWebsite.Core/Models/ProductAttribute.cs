using MHAuthorWebsite.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductAttribute;

namespace MHAuthorWebsite.Core.Models;

public class ProductAttribute
{
    [Key]
    public int Id { get; set; }

    [MaxLength(ValueMaxLength)]
    public string? Value { get; set; } = null!;

    [Required]
    public ProductAttributeDisplayPosition DisplayPosition { get; set; }

    [Required]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [ForeignKey(nameof(ProductAttributeOption))]
    public int? ProductAttributeOptionId { get; set; }

    public ProductAttributeOption? ProductAttributeOption { get; set; }

    [ForeignKey(nameof(AttributeDefinition))]
    public int AttributeDefinitionId { get; set; }

    public ProductAttributeDefinition AttributeDefinition { get; set; } = null!;
}