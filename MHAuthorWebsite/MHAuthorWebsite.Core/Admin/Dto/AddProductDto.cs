﻿using MHAuthorWebsite.Web.ViewModels.Product;

namespace MHAuthorWebsite.Core.Admin.Dto;

public class AddProductDto
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int ProductTypeId { get; set; }

    public ICollection<ImageUploadResultDto> ImageUrls { get; set; } = null!;

    public ICollection<AttributeValueForm> Attributes { get; set; } = new HashSet<AttributeValueForm>();
}