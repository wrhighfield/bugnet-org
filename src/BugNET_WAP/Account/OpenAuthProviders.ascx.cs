using System;
using System.Web;
using System.Web.UI;
using BugNET.UI;
using Microsoft.AspNet.Membership.OpenAuth;

namespace BugNET.Account
{
    public partial class OpenAuthProviders : BugNetUserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.PreRenderComplete += Page_PreRenderComplete;

            if (!IsPostBack) return;
            var provider = Request.Form["provider"];
            if (provider == null) return;

            var redirectUrl = "~/Account/RegisterExternalLogin";
            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                var resolvedReturnUrl = ResolveUrl(ReturnUrl);
                redirectUrl += "?ReturnUrl=" + HttpUtility.UrlEncode(resolvedReturnUrl);
            }

            OpenAuth.RequestAuthentication(provider, redirectUrl);
        }


        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            providersList.DataSource = OpenAuth.AuthenticationClients.GetAll();
            ;
            providersList.DataBind();
            socialLoginList.Visible = providersList.Items.Count > 0;
        }

        protected T Item<T>() where T : class
        {
            return Page.GetDataItem() as T;
        }

        protected string GetLocalizedTitleAttribute()
        {
            var item = Item<ProviderDetails>();
            return string.Format(GetLocalString("ButtonTitle"), item.ProviderDisplayName);
        }

        protected string GetLocalizedText()
        {
            var item = Item<ProviderDetails>();
            return string.Format(GetLocalString("ButtonText"), item.ProviderDisplayName);
        }

        public string ReturnUrl { get; set; }
    }
}