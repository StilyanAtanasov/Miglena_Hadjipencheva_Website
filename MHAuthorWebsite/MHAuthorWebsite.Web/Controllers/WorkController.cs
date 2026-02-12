using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Work;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static MHAuthorWebsite.GCommon.ApplicationRules.Pagination;

namespace MHAuthorWebsite.Web.Controllers
{
    public class WorkController : BaseController
    {
        private readonly IWorkService _workService;

        public WorkController(IWorkService workService) => _workService = workService;

        [HttpGet]
        [AllowAnonymous]
        [SecurityHeaders(CspFeature.Notifications)]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] string? search = null)
        {
            if (page < 1) page = 1;

            bool isAdmin = User.IsInRole("Admin");
            int worksCount = await _workService.GetWorksCountAsync(isAdmin, search);

            if (worksCount > 0 && Math.Ceiling((double)worksCount / WorksPageSize) < page)
                return NotFound();

            ICollection<WorkCardDto> works = await _workService.GetPagedWorksAsync(isAdmin, page, search);

            ViewBag.WorksCount = worksCount;
            ViewBag.CurrentSearch = search ?? "";

            if (HttpContext.Request.Headers.Any(h => h.Key == "X-Requested-With" && h.Value == "XMLHttpRequest"))
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
