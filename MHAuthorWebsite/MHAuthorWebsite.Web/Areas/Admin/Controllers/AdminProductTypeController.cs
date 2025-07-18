using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Data.Common.Extensions;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Web.ViewModels.ProductType;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminProductTypeController : AdminBaseController
{
    private readonly IAdminProductTypeService _productTypeService;

    public AdminProductTypeController(IAdminProductTypeService productTypeService) => _productTypeService = productTypeService;

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