// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BugNet.Data;
using BugNet.Web.Common.Bases;

namespace BugNet.Web.Areas.Identity.Pages.Account;

public class LogoutModel : BugNetPageModeBase<LogoutModel>
{
    private readonly SignInManager<ApplicationUser> signInManager;

    public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger) : base(logger) =>
	    this.signInManager = signInManager;

	public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        await signInManager.SignOutAsync();

        LogInformation($"User [{HttpContext.User.Identity?.Name ?? "Anonymous"}] logged out.");

        if (returnUrl != null)
        {
            return LocalRedirect(returnUrl);
        }

        // This needs to be a redirect so that the browser performs a new
        // request and the identity for the user gets updated.
        return RedirectToPage();
    }
}