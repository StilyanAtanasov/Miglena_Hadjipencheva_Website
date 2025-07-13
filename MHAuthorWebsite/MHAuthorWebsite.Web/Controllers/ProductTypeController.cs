extern alias common;

using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Web.ViewModels.ProductType;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static common::MHAuthorWebsite.Data.Common.Extensions.EnumExtensions;

namespace MHAuthorWebsite.Web.Controllers;

public class ProductTypeController : BaseController
{
    private readonly IProductTypeService _productTypeService;

    public ProductTypeController(IProductTypeService productTypeService) => _productTypeService = productTypeService;

    [HttpGet]
    public IActionResult AddProductType()
    {
        AddProductTypeForm f = new();

        ViewBag.AttributeDataTypes = Enum.GetValues(typeof(AttributeDataType))
            .Cast<AttributeDataType>()
            .Select(e => new SelectListItem
            {
                Value = ((int)e).ToString(),
                Text = e.GetDisplayName()
            })
            .ToList();

        return View(f);
    }

    [HttpPost]
    public async Task<IActionResult> AddProductType([FromForm] AddProductTypeForm form)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AttributeDataTypes = Enum.GetValues(typeof(AttributeDataType))
                .Cast<AttributeDataType>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.GetDisplayName()
                })
                .ToList();

            return View(form);
        }

        ServiceResult result = await _productTypeService.AddProductTypeAsync(form);
        if (!result.Success) return StatusCode(500);

        return RedirectToAction("Dashboard", "Admin");
    }
}
