using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.Product;
using MHAuthorWebsite.Core.Dtos.Images;
using MHAuthorWebsite.Core.Dtos.Product;
using MHAuthorWebsite.Core.Models.Enums;
using MHAuthorWebsite.Web.Dto.Product;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using MHAuthorWebsite.Web.ViewModels.Admin.Product;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;
using static MHAuthorWebsite.GCommon.ApplicationRules.Product;
using static MHAuthorWebsite.GCommon.EntityConstraints.Product;
using static MHAuthorWebsite.Web.Utils.Helpers.EditorHelper;
using static MHAuthorWebsite.Web.Utils.Mappers.ImageMapper;
using AddProductDto = MHAuthorWebsite.Core.Admin.Dto.AddProductDto;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminProductController : AdminBaseController
{
    private readonly IAdminProductTypeService _productTypeService;
    private readonly IAdminProductService _productService;
    private readonly IAdminProductImageService _imageService;

    public AdminProductController
        (IAdminProductTypeService productTypeService, IAdminProductService productService, IAdminProductImageService imageService)
    {
        _productTypeService = productTypeService;
        _productService = productService;
        _imageService = imageService;
    }

    [HttpGet]
    [SecurityHeaders(CspFeature.Editor | CspFeature.Notifications)]
    public async Task<IActionResult> AddProduct()
    {
        await PrepareViewBagForAddProduct();
        return View();
    }

    [HttpPost]
    [SecurityHeaders(CspFeature.Editor | CspFeature.Notifications)]
    public async Task<IActionResult> AddProduct(AddProductForm model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareViewBagForAddProduct();
            return View(model);
        }

        if (model.Images.Count > MaxImages)
        {
            ModelState.AddModelError(nameof(model.Images), $"Можете да качите максимум {MaxImages} снимки.");
            ModelState.AddModelError(nameof(model.Description), "Описанието не трябва да надвишава 4000 символа текст.");
            await PrepareViewBagForAddProduct();
            return View(model);
        }

        string delta = model.Description;
        string plainText = ExtractPlainTextFromQuillDelta(delta);

        if (plainText.Length < DescriptionTextMinLength)
        {
            ModelState.AddModelError(nameof(model.Description), $"Описанието не трябва да е по-кратко от {DescriptionTextMinLength} символа.");
            await PrepareViewBagForAddProduct();
            return View(model);
        }

        if (plainText.Length > DescriptionTextMaxLength)
        {
            ModelState.AddModelError(nameof(model.Description), "Описанието не трябва да надвишава 4000 символа текст.");
            await PrepareViewBagForAddProduct();
            return View(model);
        }

        if (delta.Length > DescriptionDeltaMaxLength)
        {
            ModelState.AddModelError(nameof(model.Description), "Съдържанието е прекалено голямо.");
            await PrepareViewBagForAddProduct();
            return View(model);
        }

        if (model.TitleImageId > model.Images.Count - 1 || model.TitleImageId < 0)
            return BadRequest("Invalid title image id!");

        ServiceResult<ICollection<ImageUploadResultDto>> imageResult =
            await _imageService.UploadProductImagesAsync(
                await MapIFormFileCollectionToUploadImageRequestDtoAsync(model.Images));

        if (!imageResult.Success) return StatusCode(500);
        if (imageResult.Result is null || !imageResult.Result.Any()) return StatusCode(500);

        ServiceResult<ICollection<ImageUploadResultDto>> thumbnailUploadResult =
            await _imageService.UploadProductThumbnailAsync(
                await MapIFormFileToUploadImageRequestDtoAsync(model.Images.ElementAt(model.TitleImageId)));

        if (!thumbnailUploadResult.Success) return StatusCode(500);
        if (thumbnailUploadResult.Result is null || !thumbnailUploadResult.Result.Any()) return StatusCode(500);

        AddProductDto dto = new()
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            StockQuantity = model.StockQuantity,
            ProductTypeId = model.ProductTypeId,
            ImageUrls = imageResult.Result,
            Thumbnail = thumbnailUploadResult.Result.First(),
            Attributes = model.Attributes
                .Select(a => new AttributeValueDto
                {
                    Key = a.Key,
                    Value = a.Value,
                    Label = a.Label,
                    DataType = a.DataType,
                    AttributeDefinitionId = a.AttributeDefinitionId,
                    DisplayPosition = a.DisplayPosition,
                    IsRequired = a.IsRequired,
                    ProductAttributeOptionId = a.ProductAttributeOptionId
                })
                .ToArray(),
            Weight = model.Weight,
            ThumbnailOriginalImageIndex = model.TitleImageId
        };

        ServiceResult productResult = await _productService.AddProductAsync(dto);
        if (!productResult.Success)
        {
            string[] publicIds = imageResult.Result.Select(x => x.PublicId).ToArray();
            await _imageService.DeleteImagesAsync(publicIds);

            return StatusCode(500, "Грешка при запис в базата. Снимките бяха изтрити.");
        }


        return RedirectToAction(nameof(ProductsList));
    }

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> ProductsList()
    {
        ICollection<ProductListItemDto> products = await _productService.GetProductsListReadonlyAsync();

        ICollection<ProductListItemViewModel> productViewModels = products
            .Select(p => new ProductListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                IsPublic = p.IsPublic,
                HasActiveDiscount = p.HasActiveDiscount,
                ProductTypeName = p.ProductTypeName
            })
            .ToList();

        return View(productViewModels);
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
                PredefinedValues = a.PredefinedValues
                    .Select(v => new AttributeOptionViewModel
                    {
                        Id = v.Id,
                        Value = v.Value
                    })
                    .ToList()
            }).ToList();

        return PartialView("_DynamicAttributesPartial", attributes);
    }

    [SecurityHeaders(CspFeature.Editor)]
    [HttpGet("/Admin/AdminProduct/EditProduct/{productId}")]
    public async Task<IActionResult> EditProduct([FromRoute] Guid productId)
    {
        ServiceResult<EditProductDto> result = await _productService.GetProductForEditAsync(productId);
        if (!result.Found) return NotFound();

        EditProductDto dto = result.Result!;
        EditProductFormViewModel viewModel = new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            ProductTypeName = dto.ProductTypeName,
            ImagesJson = dto.ImagesJson,
            Attributes = dto.Attributes
                .Select(a => new AttributeValueForm
                {
                    Key = a.Key,
                    Value = a.Value,
                    Label = a.Label,
                    DataType = a.DataType,
                    AttributeDefinitionId = a.AttributeDefinitionId,
                    DisplayPosition = a.DisplayPosition,
                    ProductAttributeOptionId = a.ProductAttributeOptionId,
                    IsRequired = a.IsRequired,
                    PredefinedValues = a.PredefinedValues
                        .Select(v => new AttributeOptionViewModel
                        {
                            Id = v.Id,
                            Value = v.Value
                        })
                        .ToList()
                })
                .ToArray(),
            Weight = dto.Weight,
            Images = dto.Images
                .Select(i => new ProductImageViewModel
                {
                    Id = i.Id,
                    Url = i.Url,
                    IsTitle = i.IsTitle
                })
                .ToArray(),
            NewImages = new HashSet<IFormFile>()
        };

        return View(viewModel);
    }

    [SecurityHeaders(CspFeature.Editor)]
    [HttpPost("/Admin/AdminProduct/EditProduct/{productId}")]
    public async Task<IActionResult> EditProduct([FromRoute] Guid productId, [FromForm] EditProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            UpdateExistingProductImagesBasedOnImagesJsonDto(model);
            return View(model);
        }

        string delta = model.Description;
        string plainText = ExtractPlainTextFromQuillDelta(delta);

        if (plainText.Length < DescriptionTextMinLength)
        {
            ModelState.AddModelError(nameof(model.Description), $"Описанието не трябва да е по-кратко от {DescriptionTextMinLength} символа.");
            UpdateExistingProductImagesBasedOnImagesJsonDto(model);
            return View(model);
        }

        if (plainText.Length > DescriptionTextMaxLength)
        {
            ModelState.AddModelError(nameof(model.Description), $"Описанието не трябва да надвишава {DescriptionTextMaxLength} символа.");
            UpdateExistingProductImagesBasedOnImagesJsonDto(model);
            return View(model);
        }

        if (delta.Length > DescriptionDeltaMaxLength)
        {
            ModelState.AddModelError(nameof(model.Description), "HTML съдържанието е прекалено голямо.");
            UpdateExistingProductImagesBasedOnImagesJsonDto(model);
            return View(model);
        }

        if (string.IsNullOrEmpty(model.ImagesJson))
        {
            ModelState.AddModelError(nameof(model.Images), "Грешка при вземането на изображенията.");
            UpdateExistingProductImagesBasedOnImagesJsonDto(model);
            return View(model);
        }

        ProductImagesJsonDto? images = JsonSerializer.Deserialize<ProductImagesJsonDto>(model.ImagesJson);

        if (images is null) return StatusCode(500);

        int imagesCount = images.Existing.Length + images.Added.Length;
        if (imagesCount > MaxImages)
        {
            ModelState.AddModelError(nameof(model.Images), $"Можете да качите максимум {MaxImages} снимки.");
            UpdateExistingProductImagesBasedOnImagesJsonDto(model);
            return View(model);
        }
        if (imagesCount == 0)
        {
            ModelState.AddModelError(nameof(model.Images), "Трябва да добавите поне една снимка.");
            UpdateExistingProductImagesBasedOnImagesJsonDto(model);
            return View(model);
        }

        if (images.Added.Count(i => i) + images.Existing.Count(i => i.IsTitle) != 1)
        {
            ModelState.AddModelError(nameof(model.Images), "Невалиден брой заглавни изображения.");
            UpdateExistingProductImagesBasedOnImagesJsonDto(model);
            return View(model);
        }


        Guid? newTitleImageId = null;
        if (images.Existing.Any(i => i.IsTitle))
            newTitleImageId = images.Existing.First(i => i.IsTitle).Id;

        if (images.Added.Any())
        {
            int titleImageIndex = Array.IndexOf(images.Added, true);
            ServiceResult<Guid?> imageResult = await _imageService.LinkImagesToProductAsync(
               await MapIFormFileCollectionToUploadImageRequestDtoAsync(model.NewImages!),
               titleImageIndex != -1 ? titleImageIndex : null, productId);

            if (!imageResult.Success) return StatusCode(500);

            if ((imageResult.Result is null && newTitleImageId is null)
                || imageResult.Result is not null && newTitleImageId is not null) return StatusCode(500);
            if (imageResult.Result is not null)
                newTitleImageId = imageResult.Result.Value;
        }

        ServiceResult updateTitleImageResult = await _imageService.UpdateProductTitleImageAsync(productId, newTitleImageId!.Value);
        if (!updateTitleImageResult.Success) return StatusCode(500);

        if (images.Deleted.Any())
            foreach (Guid id in images.Deleted)
            {
                ServiceResult r = await _imageService.DeleteProductImageByIdAsync(id);
                if (!r.Found) return NotFound();
                if (!r.Success) return StatusCode(500);
            }

        EditProductDto modelDto = new()
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            StockQuantity = model.StockQuantity,
            Attributes = model.Attributes
                .Select(a => new AttributeValueDto
                {
                    Key = a.Key,
                    Value = a.Value,
                    Label = a.Label,
                    DataType = a.DataType,
                    AttributeDefinitionId = a.AttributeDefinitionId,
                    ProductAttributeOptionId = a.ProductAttributeOptionId,
                    DisplayPosition = a.DisplayPosition,
                    IsRequired = a.IsRequired
                })
                .ToArray(),
            Weight = model.Weight,
            Images = images.Existing
                .Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    IsTitle = i.IsTitle
                })
                .ToArray(),
            NewImages = model.NewImages,
            ProductTypeName = model.ProductTypeName,
            ImagesJson = model.ImagesJson
        };

        ServiceResult result = await _productService.UpdateProductAsync(modelDto);
        if (!result.Found) return NotFound();

        return RedirectToAction(nameof(ProductsList));
    }

    [HttpPost("/AdminProduct/DeleteProduct/{productId}")]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid productId)
    {
        ServiceResult result = await _productService.DeleteProductAsync(productId);
        if (!result.Found) return NotFound();
        if (!result.Success) return StatusCode(500);

        ICollection<Guid> productImageIds = await _productService.GetImageIdsByProductId(productId);
        foreach (Guid id in productImageIds)
        {
            ServiceResult deleteImagesResult = await _imageService.DeleteProductImageByIdAsync(id);
            if (!deleteImagesResult.Success) return StatusCode(500);
        }

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

    [SecurityHeaders(CspFeature.Notifications)]
    [HttpGet]
    public async Task<IActionResult> AddDiscount(Guid productId)
    {
        ServiceResult<decimal> result = await _productService.GetProductPriceReadonlyAsync(productId);
        if (!result.Found) return NotFound();

        return View(new AddProductDiscountFormViewModel
        {
            ProductId = productId,
            CurrentPrice = result.Result,
            StartDate = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId)
            )
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddDiscount(AddProductDiscountFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        AddProductDiscountDto dto = new()
        {
            ProductId = model.ProductId,
            CurrentPrice = model.CurrentPrice,
            NewPrice = model.NewPrice,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
        };

        ServiceResult result = await _productService.AddDiscountAsync(dto);
        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Value);

            return View(model);
        }

        return RedirectToAction(nameof(ProductsList));
    }

    [HttpPost]
    public async Task<IActionResult> EndDiscount(Guid productId)
    {
        ServiceResult result = await _productService.EndDiscountAsync(productId);
        if (!result.Success) return StatusCode(500);

        return RedirectToAction(nameof(ProductsList));
    }

    private async Task PrepareViewBagForAddProduct()
    {
        ViewBag.ProductTypes = (await _productTypeService.GetAllReadonlyAsync())
            .Select(pt => new SelectListItem
            {
                Value = pt.Id.ToString(),
                Text = pt.Name
            })
            .ToArray();
    }

    private void UpdateExistingProductImagesBasedOnImagesJsonDto(EditProductFormViewModel model)
    {
        ProductImagesJsonDto? json = JsonSerializer.Deserialize<ProductImagesJsonDto>(model.ImagesJson);

        if (json is null) return;

        model.Images = json.Existing
            .Where(img => !json.Deleted.Contains(img.Id))
            .Select(i => new ProductImageViewModel
            {
                Id = i.Id,
                Url = i.Url,
                IsTitle = i.IsTitle
            })
            .ToArray();

        ModelState.AddModelError("AddedImages",
            "Моля, добавете отново новите изображения и проверете дали е избрано заглавно изображение");
    }
}