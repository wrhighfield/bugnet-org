﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BugNet.Data;

namespace BugNet.Web.Areas.Identity.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly ILogger<LogoutModel> logger;

    public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
    {
        this.signInManager = signInManager;
        this.logger = logger;
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out.");
        if (returnUrl != null)
        {
            return LocalRedirect(returnUrl);
        }
        else
        {
            // This needs to be a redirect so that the browser performs a new
            // request and the identity for the user gets updated.
            return RedirectToPage();
        }
    }
}