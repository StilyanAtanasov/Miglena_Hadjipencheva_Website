using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Web.ViewModels.ProductType;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Controllers;

public class ProductTypeController : BaseController
{
    private readonly IProductTypeService _productTypeService;

    public ProductTypeController(IProductTypeService productTypeService) => _productTypeService = productTypeService;

    [HttpGet]
    public IActionResult AddProductType()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddProductType([FromForm] AddProductTypeForm form)
    {
        if (!ModelState.IsValid) return View(form);

        ServiceResult result = await _productTypeService.AddProductTypeAsync(form);
        if (!result.Success) return StatusCode(500);

        return RedirectToAction(nameof(Index), "Home");
    }
}
