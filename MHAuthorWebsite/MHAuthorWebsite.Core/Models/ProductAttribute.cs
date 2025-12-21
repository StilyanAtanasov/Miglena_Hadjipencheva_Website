using MHAuthorWebsite.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductAttribute;

namespace MHAuthorWebsite.Core.Models;

public class ProductAttribute
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(KeyMaxLength)]
    public string Key { get; set; } = null!;

    [MaxLength(ValueMaxLength)]
    public string? Value { get; set; } = null!;

    [Required]
    public ProductAttributeDisplayPosition DisplayPosition { get; set; }

    [Required]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [ForeignKey(nameof(ProductAttributeOptions))]
    public int? ProductAttributeOptionsId { get; set; }

    public ProductAttributeOption ProductAttributeOptions { get; set; } = null!;

    [ForeignKey(nameof(AttributeDefinition))]
    public int AttributeDefinitionId { get; set; }

    public ProductAttributeDefinition AttributeDefinition { get; set; } = null!;
}