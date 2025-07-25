﻿using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;
using static MHAuthorWebsite.GCommon.ApplicationRules.Product;
using static MHAuthorWebsite.GCommon.EntityConstraints.Product;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminProductController : AdminBaseController
{
    private readonly IAdminProductTypeService _productTypeService;
    private readonly IAdminProductService _productService;
    private readonly IImageService _imageService;

    public AdminProductController
        (IAdminProductTypeService productTypeService, IAdminProductService productService, IImageService imageService)
    {
        _productTypeService = productTypeService;
        _productService = productService;
        _imageService = imageService;
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

        if (model.Images.Count > MaxImages)
        {
            ModelState.AddModelError(nameof(model.Images), $"Можете да качите максимум {MaxImages} снимки.");
            ModelState.AddModelError(nameof(model.Description), "Описанието не трябва да надвишава 4000 символа текст.");
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

        string delta = model.Description;
        string plainText = Regex.Replace(delta, "<.*?>", string.Empty);

        if (plainText.Length > DescriptionTextMaxLength)
        {
            ModelState.AddModelError(nameof(model.Description), "Описанието не трябва да надвишава 4000 символа текст.");
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

        if (delta.Length > DescriptionDeltaMaxLength)
        {
            ModelState.AddModelError(nameof(model.Description), "HTML съдържанието е прекалено голямо.");
            ICollection<ProductTypeDto> productTypes = await _productTypeService.GetAllReadonlyAsync();
            ViewBag.ProductTypes = productTypes
                .Select(pt => new SelectListItem
                {
                    Value = pt.Id.ToString(),
                    Text = pt.Name
                })
                .ToArray();
            return View(model); // TODO: DRY 
        }

        if (model.TitleImageId > model.Images.Count - 1 || model.TitleImageId < 0)
            return BadRequest("Invalid title image id!");

        ServiceResult<ICollection<ImageUploadResultDto>> imageResult = await _imageService.UploadImageWithPreviewAsync(model.Images, model.TitleImageId);
        if (!imageResult.Success) return StatusCode(500);
        if (imageResult.Result is null || !imageResult.Result.Any()) return StatusCode(500);

        AddProductDto dto = new AddProductDto
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            StockQuantity = model.StockQuantity,
            ProductTypeId = model.ProductTypeId,
            ImageUrls = imageResult.Result,
            Attributes = model.Attributes
        };

        ServiceResult productResult = await _productService.AddProductAsync(dto);
        if (!productResult.Success) return StatusCode(500);

        return RedirectToAction(nameof(ProductsList));
    }

    [HttpGet]
    public async Task<IActionResult> ProductsList()
    {
        ICollection<ProductListViewModel> products = await _productService.GetProductsListReadonlyAsync();
        return View(products);
    }

    [HttpGet("/AdminProduct/GetCategoryTypeAttributes/{productTypeId}")]
    public async Task<IActionResult> GetCategoryTypeAttributes([FromRoute] int productTypeId)
    {
        if (HttpContext.Request.Headers["X-Requested-With"] != "XMLHttpRequest") return Forbid();

        ICollection<ProductTypeAttributesDto> attributesDto = await _productService.GetProductTypeAttributesAsync(productTypeId);

        ICollection<AttributeValueForm> attributes = attributesDto
            .Select(a => new AttributeValueForm
            {
                AttributeDefinitionId = a.AttributeDefinitionId,
                Key = a.Key,
                Label = a.Label,
                DataType = (AttributeDataType)a.DataType,
                IsRequired = a.IsRequired,
                HasPredefinedValue = a.HasPredefinedValue
            }).ToList();

        return PartialView("_DynamicAttributesPartial", attributes);
    }

    [HttpGet("/Admin/AdminProduct/EditProduct/{productId}")]
    public async Task<IActionResult> EditProduct([FromRoute] Guid productId)
    {
        ServiceResult<EditProductFormViewModel> result = await _productService.GetProductForEditAsync(productId);
        if (!result.Found) return NotFound();

        return View(result.Result);
    }

    [HttpPost("/Admin/AdminProduct/EditProduct/{productId}")]
    public async Task<IActionResult> EditProduct([FromRoute] Guid productId, [FromForm] EditProductFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        string delta = model.Description;
        string plainText = Regex.Replace(delta, "<.*?>", string.Empty);

        if (plainText.Length > DescriptionTextMaxLength)
        {
            ModelState.AddModelError(nameof(model.Description), "Описание не трябва да надвишава 4000 символа текст.");
            return View(model);
        }

        if (delta.Length > DescriptionDeltaMaxLength)
        {
            ModelState.AddModelError(nameof(model.Description), "HTML съдържанието е прекалено голямо.");
            return View(model);
        }

        ServiceResult result = await _productService.UpdateProductAsync(model);
        if (!result.Found) return NotFound();

        return RedirectToAction("Details", "Product", new { productId = model.Id });
    }

    [HttpPost("/AdminProduct/DeleteProduct/{productId}")]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid productId)
    {
        ServiceResult result = await _productService.DeleteProductAsync(productId);
        if (!result.Found) return NotFound();
        if (!result.Success) return StatusCode(500);

        return RedirectToAction(nameof(ProductsList));
    }

    [HttpPost("/AdminProduct/TogglePublicity/{productId}")]
    public async Task<IActionResult> TogglePublicity([FromRoute] Guid productId)
    {
        ServiceResult result = await _productService.ToggleProductPublicityAsync(productId);
        if (!result.Found) return NotFound();
        if (!result.Success) return StatusCode(500);

        return RedirectToAction(nameof(ProductsList));
    }
}