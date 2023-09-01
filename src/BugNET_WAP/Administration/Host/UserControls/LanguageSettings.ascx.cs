using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.UI;

namespace BugNET.Administration.Host.UserControls
{
    public partial class LanguageSettings : BugNetUserControl, IEditHostSettingControl
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //BGN-1835 Problematic displaying of Languages settings
            lblDefaultLanguage.Text = HostSettingManager.Get(HostSettingNames.ApplicationDefaultLanguage);
        }

        #region IEditHostSettingControl Members

        /// <summary>
        /// Updates this instance.
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            HostSettingManager.UpdateHostSetting(HostSettingNames.ApplicationDefaultLanguage,
                ApplicationDefaultLanguage.SelectedValue);
            return true;
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public void Initialize()
        {
            var resources = ResourceManager.GetInstalledLanguageResources();
            var resourceItems = new List<ListItem>();
            foreach (var code in resources)
            {
                var cultureInfo = new CultureInfo(code, false);
                resourceItems.Add(new ListItem(cultureInfo.DisplayName, code));
            }


            ApplicationDefaultLanguage.DataSource = resourceItems;
            ApplicationDefaultLanguage.DataBind();
            LanguagesGridView.DataSource = resourceItems;
            LanguagesGridView.DataBind();

            ApplicationDefaultLanguage.SelectedValue =
                HostSettingManager.Get(HostSettingNames.ApplicationDefaultLanguage);
        }

        public bool ShowSaveButton => true;

        #endregion
    }
}