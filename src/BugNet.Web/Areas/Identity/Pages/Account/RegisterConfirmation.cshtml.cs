// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BugNet.Data;
using BugNet.Web.Common.Bases;

namespace BugNet.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterConfirmationModel : BugNetPageModeBase<RegisterConfirmationModel>
{
    private readonly ApplicationUserManager userManager;

    public RegisterConfirmationModel(
	    ApplicationUserManager userManager,
	    ILogger<RegisterConfirmationModel> logger) : base(logger) => this.userManager = userManager;

	public async Task<IActionResult> OnGetAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
	        LogWarning("An invalid email was supplied for register confirmation");
			return RedirectToPage("/Index");
        }

        var user = await userManager.FindByEmailAsync(email);

		if (user == null)
        {
	        LogUserNotFoundByEmail(email);
			return NotFound($"Unable to load user with email '{email}'.");
        }

        if (!userManager.Options.SignIn.RequireConfirmedEmail)
        {
	        LogInformation("Require Confirmed Email not enabled, redirecting to Login");
			return RedirectToPage("/Login");
        }

        if (!user.EmailConfirmed || !userManager.Options.SignIn.RequireConfirmedEmail)
        {
	        LogUserInformation(user, "Require Confirmed Email not enabled and AspNetUser.EmailConfirmed is false, possible mis-configuration in Identity");
			return Page();
		}

        LogUserInformation(user, "Attempt to confirm registration but has already confirmed, redirecting to Home");
        return RedirectToPage("/Index");

    }
}