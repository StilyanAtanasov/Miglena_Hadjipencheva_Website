// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MHAuthorWebsite.Web.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        public IActionResult OnGet() => RedirectToAction("Index", "Home");

        public IActionResult OnPost() => RedirectToAction("Index", "Home");
    }
}
