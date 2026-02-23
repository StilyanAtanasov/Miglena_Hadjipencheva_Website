using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Work;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Contracts;
using MHAuthorWebsite.Web.Utils.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static MHAuthorWebsite.GCommon.ApplicationRules.Pagination;

namespace MHAuthorWebsite.Web.Controllers
{
    public class WorkController : BaseController
    {
        private const string AutomationBlockMessage = "Вашата активност наподобява автоматизирано поведение. Моля, опитайте отново.";
        private const string ManualCaptchaMessage = "Моля, потвърдете ръчно, че не сте робот.";

        private readonly IWorkService _workService;
        private readonly IRecaptchaValidationService _recaptchaValidationService;

        public WorkController(IWorkService workService, IRecaptchaValidationService recaptchaValidationService)
        {
            _workService = workService;
            _recaptchaValidationService = recaptchaValidationService;
        }

        [HttpGet]
        [AllowAnonymous]
        [SecurityHeaders(CspFeature.Notifications | CspFeature.Recaptcha)]
        public async Task<IActionResult> Index(
            [FromQuery] int page = 1,
            [FromQuery] string? search = null,
            [FromQuery] string? recaptchaToken = null,
            [FromQuery] string? recaptchaV2Token = null,
            CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;

            bool isAjaxRequest = HttpContext.Request.Headers.Any(h => h.Key == "X-Requested-With" && h.Value == "XMLHttpRequest");
            if (isAjaxRequest && !(User.Identity?.IsAuthenticated ?? false) && !string.IsNullOrWhiteSpace(search))
            {
                RecaptchaValidationResult v3VerificationResult = await _recaptchaValidationService.VerifyV3Async(
                    recaptchaToken,
                    "work_search",
                    cancellationToken: cancellationToken);

                if (!v3VerificationResult.IsSuccess)
                {
                    if (v3VerificationResult.RequiresManualChallenge)
                    {
                        bool hasManualToken = !string.IsNullOrWhiteSpace(recaptchaV2Token);
                        RecaptchaValidationResult v2VerificationResult = await _recaptchaValidationService.VerifyV2Async(
                            recaptchaV2Token,
                            cancellationToken);

                        if (!v2VerificationResult.IsSuccess)
                        {
                            if (hasManualToken) return BadRequest(new { message = AutomationBlockMessage });
                            return StatusCode(StatusCodes.Status428PreconditionRequired, new { message = ManualCaptchaMessage });
                        }
                    }
                    else return BadRequest(new { message = AutomationBlockMessage });
                }
            }

            bool isAdmin = User.IsInRole("Admin");
            int worksCount = await _workService.GetWorksCountAsync(isAdmin, search);

            if (worksCount > 0 && Math.Ceiling((double)worksCount / WorksPageSize) < page)
                return NotFound();

            ICollection<WorkCardDto> works = await _workService.GetPagedWorksAsync(isAdmin, page, search);

            ViewBag.WorksCount = worksCount;
            ViewBag.CurrentSearch = search ?? "";

            if (isAjaxRequest)
                return PartialView("_WorkCardsPartial", works);

            return View(works);
        }

        [HttpGet]
        [AllowAnonymous]
        [SecurityHeaders(CspFeature.Editor)]
        public async Task<IActionResult> Details(Guid id)
        {
            bool isAdmin = User.IsInRole("Admin");
            ServiceResult<WorkDetailsDto> result = await _workService.GetWorkDetailsAsync(id, isAdmin);

            if (!result.Found) return NotFound();
            if (!result.Success) return StatusCode(500);

            return View(result.Result);
        }
    }
}
