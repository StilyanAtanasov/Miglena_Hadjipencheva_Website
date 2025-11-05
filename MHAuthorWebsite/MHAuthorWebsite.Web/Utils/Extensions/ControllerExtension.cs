using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace MHAuthorWebsite.Web.Utils.Extensions;

public static class ControllerExtensions
{
    public static async Task<string> RenderViewAsync<TModel>(
        this Controller controller,
        string viewName,
        TModel model,
        bool partial = false)
    {
        controller.ViewData.Model = model;
        await using StringWriter writer = new();

        ICompositeViewEngine viewEngine = controller.HttpContext.RequestServices.GetService<ICompositeViewEngine>()!;
        ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

        if (viewResult.View == null)
            throw new FileNotFoundException($"View '{viewName}' not found.");

        ViewContext viewContext = new(
            controller.ControllerContext,
            viewResult.View,
            controller.ViewData,
            controller.TempData,
            writer,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return writer.ToString();
    }
}