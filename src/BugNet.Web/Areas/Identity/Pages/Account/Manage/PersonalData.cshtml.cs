// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BugNet.Data;

namespace BugNet.Web.Areas.Identity.Pages.Account.Manage;

public class PersonalDataModel : PageModel
{
    private readonly ApplicationUserManager userManager;
    private readonly ILogger<PersonalDataModel> logger;

    public PersonalDataModel(
	    ApplicationUserManager userManager,
        ILogger<PersonalDataModel> logger)
    {
        this.userManager = userManager;
        this.logger = logger;
    }

    public async Task<IActionResult> OnGet()
    {
        var user = await userManager.GetUserAsync(User);
        if (user != null) return Page();

        logger.LogWarning("Unable to load user with ID {UserId}", userManager.GetUserId(User));
        return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
    }
}