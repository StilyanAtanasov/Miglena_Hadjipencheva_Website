using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHAuthorWebsite.Core.Models;

public class ProductAttributeOption
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Value { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(AttributeDefinition))]
    public int AttributeDefinitionId { get; set; }

    public ProductAttributeDefinition AttributeDefinition { get; set; } = null!;

    public ICollection<ProductAttribute> UsedInAttributes { get; set; } = new HashSet<ProductAttribute>();
}