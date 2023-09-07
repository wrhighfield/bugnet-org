// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BugNet.Data;
using BugNet.Web.Common.Bases;

namespace BugNet.Web.Areas.Identity.Pages.Account;

public class ForgotPasswordModel : BugNetPageModeBase<ForgotPasswordModel>
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IEmailSender emailSender;

    public ForgotPasswordModel(
	    UserManager<ApplicationUser> userManager,
	    IEmailSender emailSender,
	    ILogger<ForgotPasswordModel> logger) : base(logger)
    {
        this.userManager = userManager;
        this.emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
	    if (!IsModelValid)
	    {
		    return Page();
	    }

		var user = await userManager.FindByEmailAsync(Input.Email);

	    if (user == null)
	    {
		    LogUserNotFoundByEmail(Input.Email);
		    return RedirectToPage("./ForgotPasswordConfirmation");
		}

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
			LogUserWarning(user, "Forgot password reset was attempted but user has not confirmed their email");
	        return RedirectToPage("./ForgotPasswordConfirmation");
        }

        // For more information on how to enable account confirmation and password reset please
        // visit https://go.microsoft.com/fwlink/?LinkID=532713
        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page(
	        "/Account/ResetPassword",
	        pageHandler: null,
	        values: new { area = "Identity", code },
	        protocol: Request.Scheme);

        await emailSender.SendEmailAsync(
	        Input.Email,
	        "Reset Password",
	        $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        return RedirectToPage("./ForgotPasswordConfirmation");

    }
}