using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Extensions;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.ProductType;
using MHAuthorWebsite.Core.Models.Enums;
using MHAuthorWebsite.Web.ViewModels.Admin.ProductType;
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

        AddProductTypeDto dto = new()
        {

            Name = form.Name,
            Attributes = form.Attributes
                .Select(a => new AttributeDefinitionDto
                {
                    Key = a.Key,
                    Label = a.Label,
                    DataType = a.DataType,
                    IsRequired = a.IsRequired,
                    PredefinedValues = a.PredefinedValues
                })
                .ToArray(),
            HasAdditionalProperties = form.HasAdditionalProperties,
        };

        ServiceResult result = await _productTypeService.AddProductTypeAsync(dto);
        if (!result.Success) return StatusCode(500);

        return RedirectToAction("Dashboard", "AdminDashboard");
    }
}