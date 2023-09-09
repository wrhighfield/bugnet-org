// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BugNet.Data;
using BugNet.Web.Common.Bases;

namespace BugNet.Web.Areas.Identity.Pages.Account
{
    public class LoginModel : BugNetPageModeBase<LoginModel>
	{
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IStringLocalizer<LoginModel> pageStrings;
        private readonly ApplicationUserManager userManager;

		public LoginModel(
			SignInManager<ApplicationUser> signInManager,
			IStringLocalizer<LoginModel> pageStrings,
			ApplicationUserManager userManager,
			ILogger<LoginModel> logger) : base(logger)
        {
            this.signInManager = signInManager;
            this.pageStrings = pageStrings;
            this.userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ErrorMessage = string.Empty;

            if (User.Identity?.IsAuthenticated ?? false)
            {
	            RedirectToPage("~/Home/Index");
            }

			ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

			if (!IsModelValid)
			{
				return Page();
			}

			// This doesn't count login failures towards account lockout
			// To enable password failures to trigger account lockout, set lockoutOnFailure: true
			var result = await signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

			if (result.Succeeded)
			{
				LogInformation($"User [{Input.Email}] logged in");
				return LocalRedirect(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
	            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
            }

            if (result.IsLockedOut)
            {
	            LogWarning($"User [{Input.Email}] account locked out");
	            return RedirectToPage("./Lockout");
            }

            var user = await userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
	            LogUserNotFoundByEmail(Input.Email);
	            return NotFound($"Unable to load user with email '{Input.Email}'.");
			}

            ErrorMessage = pageStrings["Invalid.Login.Attempt"];

			if (user is {EmailConfirmed: false} && signInManager.Options.SignIn.RequireConfirmedEmail)
            {
				ErrorMessage = pageStrings["Email.Not.Confirmed"];
			}

            return Page();
        }
    }
}
