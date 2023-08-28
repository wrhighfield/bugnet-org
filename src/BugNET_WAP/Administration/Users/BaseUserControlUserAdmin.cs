using System;
using System.Web.Security;
using BugNET.BLL;
using BugNET.UI;

namespace BugNET.Administration.Users
{
    public abstract class BaseUserControlUserAdmin : BugNetUserControl
    {
        protected MembershipUser MembershipData { get; set; }

        protected void GetMembershipData(Guid userId)
        {
            MembershipData = UserManager.GetUser(userId);
        }
    }
}