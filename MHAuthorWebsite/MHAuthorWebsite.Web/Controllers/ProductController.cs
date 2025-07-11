﻿using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHAuthorWebsite.Web.Controllers;

public class ProductController : Controller
{
    private readonly IProductTypeService _productTypeService;
    private readonly IProductService _productService;

    public ProductController(IProductTypeService productTypeService, IProductService productService)
    {
        _productTypeService = productTypeService;
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> AddProduct()
    {
        ICollection<ProductTypeDto> productTypes = await _productTypeService.GetAllReadonlyAsync();

        ViewBag.ProductTypes = productTypes
            .Select(pt => new SelectListItem
            {
                Value = pt.Id.ToString(),
                Text = pt.Name
            })
            .ToArray();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct(AddProductForm model)
    {
        if (!ModelState.IsValid)
        {
            ICollection<ProductTypeDto> productTypes = await _productTypeService.GetAllReadonlyAsync();
            ViewBag.ProductTypes = productTypes
                .Select(pt => new SelectListItem
                {
                    Value = pt.Id.ToString(),
                    Text = pt.Name
                })
                .ToArray();
            return View(model);
        }

        ServiceResult result = await _productService.AddProductAsync(model);
        if (!result.Success) return StatusCode(500);

        return RedirectToAction(nameof(Index), "Home");
    }

    [HttpGet]
    public async Task<IActionResult> AllProducts()
    {
        ICollection<ProductCardViewModel> products = await _productService.GetAllProductCardsReadonlyAsync();
        return View(products);
    }

    [HttpGet("Product/GetCategoryTypeAttributes/{productTypeId}")]
    public async Task<IActionResult> GetCategoryTypeAttributes([FromRoute] int productTypeId)
    {
        if (HttpContext.Request.Headers["X-Requested-With"] != "XMLHttpRequest") return Forbid();

        ICollection<ProductTypeAttributesDto> attributesDto = await _productService.GetProductTypeAttributesAsync(productTypeId);

        ICollection<AttributeValueForm> attributes = attributesDto
            .Select(a => new AttributeValueForm
            {
                Key = a.Key,
                Label = a.Label,
                DataType = (AttributeDataType)a.DataType,
                IsRequired = a.IsRequired,
                HasPredefinedValue = a.HasPredefinedValue
            }).ToList();

        return PartialView("_DynamicAttributesPartial", attributes);
    }
}