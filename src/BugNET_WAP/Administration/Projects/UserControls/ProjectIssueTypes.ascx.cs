﻿using System;
using System.Web.UI.WebControls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.Entities;
using BugNET.UI;
using BugNET.UserControls;

namespace BugNET.Administration.Projects.UserControls
{
    public partial class ProjectIssueTypes : BugNetUserControl, IEditProjectControl
    {
        //*********************************************************************
        //
        // This user control is used by both the new project wizard and update
        // project page.
        //
        //*********************************************************************

        /// <summary>
        /// Gets or sets the project id.
        /// </summary>
        /// <value>The project id.</value>
        public int ProjectId
        {
            get => ViewState.Get("ProjectId", 0);
            set => ViewState.Set("ProjectId", value);
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            return Page.IsValid;
        }

        /// <summary>
        /// Gets a value indicating whether [show save button].
        /// </summary>
        /// <value><c>true</c> if [show save button]; otherwise, <c>false</c>.</value>
        public bool ShowSaveButton => false;

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public void Initialize()
        {
            BindIssueType();
            lstImages.Initialize();
        }


        /// <summary>
        /// Binds the status.
        /// </summary>
        private void BindIssueType()
        {
            grdIssueTypes.Columns[1].HeaderText = GetGlobalString("SharedResources", "IssueType");
            grdIssueTypes.Columns[2].HeaderText = GetGlobalString("SharedResources", "Image");
            grdIssueTypes.Columns[3].HeaderText = GetGlobalString("SharedResources", "Order");

            grdIssueTypes.DataSource = IssueTypeManager.GetByProjectId(ProjectId);
            grdIssueTypes.DataKeyField = "Id";
            grdIssueTypes.DataBind();

            grdIssueTypes.Visible = grdIssueTypes.Items.Count != 0;
        }


        /// <summary>
        /// Handles the ItemCommand event of the grdIssueTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.DataGridCommandEventArgs"/> instance containing the event data.</param>
        protected void grdIssueTypes_ItemCommand(object sender, DataGridCommandEventArgs e)
        {
            IssueType s;
            var itemIndex = e.Item.ItemIndex;
            switch (e.CommandName)
            {
                case "up":
                    //move row up
                    if (itemIndex == 0)
                        return;
                    s = IssueTypeManager.GetById(Convert.ToInt32(grdIssueTypes.DataKeys[e.Item.ItemIndex]));
                    s.SortOrder -= 1;
                    IssueTypeManager.SaveOrUpdate(s);
                    break;
                case "down":
                    //move row down
                    if (itemIndex == grdIssueTypes.Items.Count - 1)
                        return;
                    s = IssueTypeManager.GetById(Convert.ToInt32(grdIssueTypes.DataKeys[e.Item.ItemIndex]));
                    s.SortOrder += 1;
                    IssueTypeManager.SaveOrUpdate(s);
                    break;
            }

            BindIssueType();
        }

        /// <summary>
        /// Adds the status.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void AddIssueType(object s, EventArgs e)
        {
            var newName = txtName.Text.Trim();

            if (newName == string.Empty)
                return;

            var newIssueType = new IssueType
                {ProjectId = ProjectId, Name = newName, ImageUrl = lstImages.SelectedValue};

            if (IssueTypeManager.SaveOrUpdate(newIssueType))
            {
                txtName.Text = "";
                BindIssueType();
                lstImages.SelectedValue = string.Empty;
            }
            else
            {
                ActionMessage.ShowErrorMessage(LoggingManager.GetErrorMessageResource("SaveIssueTypeError"));
            }
        }

        /// <summary>
        /// Deletes the status.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.DataGridCommandEventArgs"/> instance containing the event data.</param>
        protected void grdIssueTypes_Delete(object s, DataGridCommandEventArgs e)
        {
            var id = (int) grdIssueTypes.DataKeys[e.Item.ItemIndex];

            if (!IssueTypeManager.Delete(id, out var cannotDeleteMessage))
            {
                ActionMessage.ShowErrorMessage(cannotDeleteMessage);
                return;
            }

            BindIssueType();
        }

        /// <summary>
        /// Handles the Edit event of the grdIssueTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.DataGridCommandEventArgs"/> instance containing the event data.</param>
        protected void grdIssueTypes_Edit(object sender, DataGridCommandEventArgs e)
        {
            grdIssueTypes.EditItemIndex = e.Item.ItemIndex;
            grdIssueTypes.DataBind();
        }

        /// <summary>
        /// Handles the Update event of the grdIssueTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.DataGridCommandEventArgs"/> instance containing the event data.</param>
        protected void grdIssueTypes_Update(object sender, DataGridCommandEventArgs e)
        {
            var txtIssueTypeName = (TextBox) e.Item.FindControl("txtIssueTypeName");
            var pickImage = (PickImage) e.Item.FindControl("lstEditImages");

            if (txtIssueTypeName.Text.Trim() == "") throw new ArgumentNullException("Issue Type name empty");

            var s = IssueTypeManager.GetById(Convert.ToInt32(grdIssueTypes.DataKeys[e.Item.ItemIndex]));
            s.Name = txtIssueTypeName.Text.Trim();
            s.ImageUrl = pickImage.SelectedValue;
            IssueTypeManager.SaveOrUpdate(s);

            grdIssueTypes.EditItemIndex = -1;
            BindIssueType();
        }

        /// <summary>
        /// Handles the Cancel event of the grdIssueTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.DataGridCommandEventArgs"/> instance containing the event data.</param>
        protected void grdIssueTypes_Cancel(object sender, DataGridCommandEventArgs e)
        {
            grdIssueTypes.EditItemIndex = -1;
            grdIssueTypes.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the grdIssueTypes control.
        /// </summary>
        /// <param name="s">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.DataGridItemEventArgs"/> instance containing the event data.</param>
        protected void grdIssueTypes_ItemDataBound(object s, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var currentIssueType = (IssueType) e.Item.DataItem;

                var lblIssueTypeName = (Label) e.Item.FindControl("lblIssueTypeName");
                lblIssueTypeName.Text = currentIssueType.Name;

                var imgIssueType = (Image) e.Item.FindControl("imgIssueType");
                if (currentIssueType.ImageUrl == string.Empty)
                {
                    imgIssueType.Visible = false;
                }
                else
                {
                    imgIssueType.ImageUrl = "~/Images/IssueType/" + currentIssueType.ImageUrl;
                    imgIssueType.AlternateText = currentIssueType.Name;
                }

                var cmdDelete = (ImageButton) e.Item.FindControl("cmdDelete");
                var message = string.Format(GetLocalString("ConfirmDelete"), currentIssueType.Name.Trim());
                cmdDelete.Attributes.Add("onclick", $"return confirm('{message.JsEncode()}');");
            }

            if (e.Item.ItemType != ListItemType.EditItem) return;
            {
                var currentIssueType = (IssueType) e.Item.DataItem;
                var txtIssueTypeName = (TextBox) e.Item.FindControl("txtIssueTypeName");
                var pickImage = (PickImage) e.Item.FindControl("lstEditImages");

                txtIssueTypeName.Text = currentIssueType.Name;
                pickImage.Initialize();
                pickImage.SelectedValue = currentIssueType.ImageUrl;
            }
        }

        /// <summary>
        /// Validates the status.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void ValidateIssueType(object s, ServerValidateEventArgs e)
        {
            e.IsValid = grdIssueTypes.Items.Count > 0;
        }
    }
}