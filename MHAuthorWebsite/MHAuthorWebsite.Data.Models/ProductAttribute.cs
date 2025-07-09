using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductAttribute;

namespace MHAuthorWebsite.Data.Models;

public class ProductAttribute
{
    [Key]
    [Comment("Primary key")]
    public int Id { get; set; }

    [Required]
    [MaxLength(KeyMaxLength)]
    [Comment("Name of the attribute (e.g., ISBN)")]
    public string Key { get; set; } = null!;

    [MaxLength(ValueMaxLength)]
    [Comment("Value of the attribute")]
    public string? Value { get; set; } = null!;

    [Required]
    [Comment("Foreign key to Product")]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [Comment("Foreign key to Product Attribute Options")]
    [ForeignKey(nameof(ProductAttributeOptions))]
    public int? ProductAttributeOptionsId { get; set; }

    public ProductAttributeOption ProductAttributeOptions { get; set; } = null!;
}