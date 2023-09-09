// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BugNet.Data;
using BugNet.Web.Common.Bases;

namespace BugNet.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ResendEmailConfirmationModel : BugNetPageModeBase<ResendEmailConfirmationModel>
{
    private readonly ApplicationUserManager userManager;
    private readonly IEmailSender emailSender;

    public ResendEmailConfirmationModel(
	    ApplicationUserManager userManager,
	    IEmailSender emailSender,
	    ILogger<ResendEmailConfirmationModel> logger) : base(logger)
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
        public string Email { get; set; }
    }

	public async Task<IActionResult> OnGetAsync(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
		{
			LogWarning("An invalid email was supplied for resend email confirmation reset");
			return Page();
		}

		var user = await userManager.FindByEmailAsync(email);

		if (user == null)
		{
			LogUserNotFoundByEmail(Input.Email);
			return NotFound($"Unable to load user with email '{email}'.");
		}

		Input.Email = email;

		return Page();
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
			return Page();
        }

        var userId = await userManager.GetUserIdAsync(user);
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        LogUserInformation(user, "Attempting to send an email confirmation");

		var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new {userId, code },
            protocol: Request.Scheme);

        await emailSender.SendEmailAsync(
            Input.Email,
            "Confirm your email",
            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        LogUserInformation(user, "Sent an email confirmation");
        ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
        return Page();
    }
}