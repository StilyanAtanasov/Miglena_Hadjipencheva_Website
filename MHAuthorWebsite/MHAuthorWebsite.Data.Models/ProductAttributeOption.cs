using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHAuthorWebsite.Data.Models;

public class ProductAttributeOption
{
    [Key]
    [Comment("Primary key")]
    public int Id { get; set; }

    [Required]
    [Comment("Predefined selectable value")]
    public string Value { get; set; } = null!;

    [Required]
    [Comment("FK to attribute definition this value belongs to")]
    [ForeignKey(nameof(AttributeDefinition))]
    public int AttributeDefinitionId { get; set; }

    public ProductAttributeDefinition AttributeDefinition { get; set; } = null!;

    public ICollection<ProductAttribute> UsedInAttributes { get; set; } = new HashSet<ProductAttribute>();
}