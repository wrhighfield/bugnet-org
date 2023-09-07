// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BugNet.Data;
using BugNet.Web.Common.Bases;

namespace BugNet.Web.Areas.Identity.Pages.Account;

public class LoginWith2FaModel : BugNetPageModeBase<LoginWith2FaModel>
{
    private readonly SignInManager<ApplicationUser> signInManager;

    public LoginWith2FaModel(
        SignInManager<ApplicationUser> signInManager,
        ILogger<LoginWith2FaModel> logger): base(logger) => this.signInManager = signInManager;

    [BindProperty]
    public InputModel Input { get; set; }

    public bool RememberMe { get; set; }

    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator code")]
        public string TwoFactorCode { get; init; }

        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; init; }
    }

    public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user != null)
        {
            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }

        LogWarning("Unable to load two-factor authentication user");
        throw new InvalidOperationException("Unable to load two-factor authentication user.");
    }

    public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
    {
        if (!IsModelValid)
        {
            return Page();
        }

        returnUrl ??= Url.Content("~/");

        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            LogWarning("Unable to load two-factor authentication user");
            return RedirectToPage("/Index");
        }

        var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);

        if (result.Succeeded)
        {
            Logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            Logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
            return RedirectToPage("./Lockout");
        }

        Logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
        ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        return Page();
    }
}