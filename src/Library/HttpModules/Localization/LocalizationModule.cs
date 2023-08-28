﻿using System;
using System.Globalization;
using System.Threading;
using System.Web;
using BugNET.BLL;
using BugNET.Common;

namespace BugNET.HttpModules
{
    /// <summary>
    /// 
    /// </summary>
    public class LocalizationModule : IHttpModule
    {
        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        /// <value>The name of the module.</value>
        public string ModuleName => "LocalizationModule";

        #region IHttpModule Members

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += ContextPreRequestHandlerExecute;
        }

        #endregion

        /// <summary>
        /// Handles the PreRequestHandlerExecute event of the context control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private static void ContextPreRequestHandlerExecute(object sender, EventArgs e)
        {        
            if (HttpContext.Current.Request.Url.LocalPath.ToLower().EndsWith("install.aspx"))
                return;

            string culture;

            if (HttpContext.Current.User == null || HttpContext.Current.Profile == null || !HttpContext.Current.User.Identity.IsAuthenticated)
            {
                culture = HostSettingManager.Get(HostSettingNames.ApplicationDefaultLanguage);
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            }
            else
            {
                if (HttpContext.Current.Profile["PreferredLocale"] == null ||
                    string.IsNullOrEmpty(HttpContext.Current.Profile["PreferredLocale"].ToString())) return;
                //retrieve culture
                culture = HttpContext.Current.Profile["PreferredLocale"].ToString();

                //set culture
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

            }
        }
    }
}