// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BugNet.Data;
using BugNet.Web.Common.Bases;

namespace BugNet.Web.Areas.Identity.Pages.Account;

public class RegisterModel : BugNetPageModeBase<RegisterModel>
{
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IUserStore<ApplicationUser> userStore;
    private readonly IUserEmailStore<ApplicationUser> emailStore;
    private readonly IEmailSender emailSender;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender): base(logger)
    {
        this.userManager = userManager;
        this.userStore = userStore;
        emailStore = GetEmailStore();
        this.signInManager = signInManager;
        this.emailSender = emailSender;
    }


    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; init; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; init; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; }
	}

    [TempData]
    public string ErrorMessage { get; set; }

	public async Task OnGetAsync(string returnUrl = null)
    {
	    ErrorMessage = string.Empty;
		ReturnUrl = returnUrl;
        ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
	    ErrorMessage = string.Empty;

		returnUrl ??= Url.Content("~/");
        ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!IsModelValid)
        {
	        return Page();
        }

		var user = CreateUser();

        await userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
        var result = await userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
	        LogUserInformation(user, "Account was created successfully");

	        var userId = await userManager.GetUserIdAsync(user);
	        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
	        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

	        LogUserInformation(user, "Attempting to send confirm registration email");

			var callbackUrl = Url.Page(
		        "/Account/ConfirmEmail",
		        pageHandler: null,
		        values: new { area = "Identity", userId, code, returnUrl },
		        protocol: Request.Scheme);

	        await emailSender.SendEmailAsync(Input.Email, "Confirm your email",
		        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

	        LogUserInformation(user, "Sent email");

			if (userManager.Options.SignIn.RequireConfirmedAccount)
	        {
		        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
	        }

			LogUserInformation(user, "Require Confirmed Account not enabled signing in user");

			await signInManager.SignInAsync(user, isPersistent: false);

	        return LocalRedirect(returnUrl);
        }

        LogIdentityErrors(result.Errors);
		ErrorMessage = result.Errors.FirstOrDefault()?.Description;

		return Page();
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                                                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameter-less constructor, or alternatively " +
                                                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)userStore;
    }
}