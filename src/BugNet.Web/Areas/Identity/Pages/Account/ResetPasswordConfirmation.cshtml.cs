// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using BugNet.Web.Common.Bases;

namespace BugNet.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ResetPasswordConfirmationModel : BugNetPageModeBase<ResetPasswordConfirmationModel>
{
    public void OnGet()
    {
    }

    public ResetPasswordConfirmationModel(ILogger<ResetPasswordConfirmationModel> logger) : base(logger)
    {
    }
}