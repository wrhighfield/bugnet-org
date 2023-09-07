// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BugNet.Data;
using BugNet.Web.Common.Bases;

namespace BugNet.Web.Areas.Identity.Pages.Account;

public class ResetPasswordModel : BugNetPageModeBase<ResetPasswordModel>
{
    private readonly UserManager<ApplicationUser> userManager;

    public ResetPasswordModel(
	    UserManager<ApplicationUser> userManager,
	    ILogger<ResetPasswordModel> logger) : base(logger) => this.userManager = userManager;

	[BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; init; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; }

        [Required]
        public string Code { get; init; }

    }

    [TempData]
    public string ErrorMessage { get; set; }

	public IActionResult OnGet(string code = null)
    {
	    if (code == null)
        {
            LogWarning("An invalid code was supplied for password reset");
            return BadRequest("An invalid code was supplied for password reset.");
        }

	    Input = new InputModel
	    {
		    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
	    };

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
			return RedirectToPage("./ResetPasswordConfirmation");
        }

        var result = await userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
        if (result.Succeeded)
        {
            LogUserInformation(user, "Password reset was successful");
			return RedirectToPage("./ResetPasswordConfirmation");
        }

        LogIdentityErrors(result.Errors);
        ErrorMessage = result.Errors.FirstOrDefault()?.Description;

        // If we got this far, something failed, redisplay form
        return Page();
	}
}