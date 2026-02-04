using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Error;
using MHAuthorWebsite.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MHAuthorWebsite.Web.Controllers;

public class ErrorController : BaseController
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWebHostEnvironment _environment;

    public ErrorController(IServiceScopeFactory scopeFactory, IWebHostEnvironment environment)
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
    }

    [AllowAnonymous]
    [Route("Error/Error/{statusCode?}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode)
    {
        IExceptionHandlerFeature? exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        IStatusCodeReExecuteFeature? statusCodeFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        Exception? exception = exceptionHandlerFeature?.Error;

        string originalPath = exceptionHandlerFeature?.Path
                              ?? statusCodeFeature?.OriginalPath
                              ?? HttpContext.Request.Path;
        if (string.IsNullOrEmpty(originalPath)) return RedirectToAction("Index", "Home");

        string requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        if ((exception != null || statusCode >= 500) && !_environment.IsDevelopment())
            _ = Task.Run(async () =>
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                ILogger<ErrorController> logger = scope.ServiceProvider.GetRequiredService<ILogger<ErrorController>>();

                try
                {
                    IErrorService scopedErrorService = scope.ServiceProvider.GetRequiredService<IErrorService>();
                    await scopedErrorService.HandleErrorAsync(new HandleErrorDto
                    {
                        Exception = exception,
                        Path = originalPath,
                        RequestId = requestId,
                        UserId = GetUserId(),
                        Method = HttpContext.Request.Method,
                        UserNameIdentifier = User.Identity?.Name ?? "Anonymous"
                    });
                }
                catch
                {
                    logger.LogCritical("CRITICAL: Failed to handle error!!!");
                    logger.LogCritical($"Original Path: {originalPath}");
                    logger.LogCritical($"Exception: {exception?.Message}");
                }
            });

        return statusCode switch
        {
            400 => View("400", new ErrorViewModel { RequestId = requestId }),
            401 => View("401"),
            403 => View("403"),
            404 => View("404"),
            _ => View("Error", new ErrorViewModel { RequestId = requestId })
        };
    }
}