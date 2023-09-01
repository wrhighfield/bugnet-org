using BugNET.UI;

namespace BugNET.Administration.Projects.UserControls
{
    using System;
    using System.Linq;
    using Entities;
    using BLL;


    /// <summary>
    /// 
    /// </summary>
    public partial class ProjectDefaultValues : BugNetUserControl, IEditProjectControl
    {
        /// <summary>
        /// Binds the options.
        /// </summary>
        private void BindOptions()
        {
            FillDropDownLists();
            ReadDefaultValuesForProject();
        }

        /// <summary>
        /// Reads the default values for project.
        /// </summary>
        private void ReadDefaultValuesForProject()
        {
            var defValues = IssueManager.GetDefaultIssueTypeByProjectId(ProjectId);
            DefaultValue selectedValue = null;
            if (defValues.Count > 0)
                selectedValue = defValues.ElementAt<DefaultValue>(0);

            if (selectedValue != null)
            {
                DropIssueType.SelectedValue = selectedValue.IssueTypeId;
                DropPriority.SelectedValue = selectedValue.PriorityId;
                DropResolution.SelectedValue = selectedValue.ResolutionId;
                DropCategory.SelectedValue = selectedValue.CategoryId;
                DropMilestone.SelectedValue = selectedValue.MilestoneId;
                DropAffectedMilestone.SelectedValue = selectedValue.AffectedMilestoneId;

                if (selectedValue.AssignedUserName != "none" &&
                    DropAssignedTo.DataSource.Exists(i => i.UserName == selectedValue.AssignedUserName))
                    DropAssignedTo.SelectedValue = selectedValue.AssignedUserName;

                if (selectedValue.OwnerUserName != "none" &&
                    DropOwned.DataSource.Exists(i => i.UserName == selectedValue.OwnerUserName))
                    DropOwned.SelectedValue = selectedValue.OwnerUserName;

                DropStatus.SelectedValue = selectedValue.StatusId;

                if (selectedValue.IssueVisibility == 0) chkPrivate.Checked = false;
                if (selectedValue.IssueVisibility == 1) chkPrivate.Checked = true;

                if (selectedValue.DueDate.HasValue) DueDate.Text = selectedValue.DueDate.Value.ToString();

                ProgressSlider.Text = selectedValue.Progress.ToString();
                txtEstimation.Text = selectedValue.Estimation.ToString();

                //Visibility Section

                chkStatusVisibility.Checked = selectedValue.StatusVisibility;
                chkOwnedByVisibility.Checked = selectedValue.OwnedByVisibility;
                chkPriorityVisibility.Checked = selectedValue.PriorityVisibility;
                chkAssignedToVisibility.Checked = selectedValue.AssignedToVisibility;
                chkPrivateVisibility.Checked = selectedValue.PrivateVisibility;
                chkCategoryVisibility.Checked = selectedValue.CategoryVisibility;
                chkDueDateVisibility.Checked = selectedValue.DueDateVisibility;
                chkTypeVisibility.Checked = selectedValue.TypeVisibility;
                chkPercentCompleteVisibility.Checked = selectedValue.PercentCompleteVisibility;
                chkMilestoneVisibility.Checked = selectedValue.MilestoneVisibility;
                chkEstimationVisibility.Checked = selectedValue.EstimationVisibility;
                chkResolutionVisibility.Checked = selectedValue.ResolutionVisibility;
                chkAffectedMilestoneVisibility.Checked = selectedValue.AffectedMilestoneVisibility;
                chkNotifyAssignedTo.Checked = selectedValue.AssignedToNotify;
                chkNotifyOwner.Checked = selectedValue.OwnedByNotify;

                chkStatusEditVisibility.Checked = selectedValue.StatusEditVisibility;
                chkOwnedByEditVisibility.Checked = selectedValue.OwnedByEditVisibility;
                chkPriorityEditVisibility.Checked = selectedValue.PriorityEditVisibility;
                chkAssignedToEditVisibility.Checked = selectedValue.AssignedToEditVisibility;
                chkPrivateEditVisibility.Checked = selectedValue.PrivateEditVisibility;
                chkCategoryEditVisibility.Checked = selectedValue.CategoryEditVisibility;
                chkDueDateEditVisibility.Checked = selectedValue.DueDateEditVisibility;
                chkTypeEditVisibility.Checked = selectedValue.TypeEditVisibility;
                chkPercentCompleteEditVisibility.Checked = selectedValue.PercentCompleteEditVisibility;
                chkMilestoneEditVisibility.Checked = selectedValue.MilestoneEditVisibility;
                chkEstimationEditVisibility.Checked = selectedValue.EstimationEditVisibility;
                chkResolutionEditVisibility.Checked = selectedValue.ResolutionEditVisibility;
                chkAffectedMilestoneEditVisibility.Checked = selectedValue.AffectedMilestoneEditVisibility;
            }
        }

        /// <summary>
        /// Fills the drop down lists.
        /// </summary>
        private void FillDropDownLists()
        {
            var users = UserManager.GetUsersByProjectId(ProjectId);
            //Get Type 

            DropIssueType.DataSource = IssueTypeManager.GetByProjectId(ProjectId);
            DropIssueType.DataBind();

            //Get Priority
            DropPriority.DataSource = PriorityManager.GetByProjectId(ProjectId);
            DropPriority.DataBind();

            //Get Resolutions
            DropResolution.DataSource = ResolutionManager.GetByProjectId(ProjectId);
            DropResolution.DataBind();

            //Get categories
            var categories = new CategoryTree();
            DropCategory.DataSource = categories.GetCategoryTreeByProjectId(ProjectId);
            DropCategory.DataBind();

            //Get milestones          
            DropMilestone.DataSource = MilestoneManager.GetByProjectId(ProjectId, false);
            DropMilestone.DataBind();

            DropAffectedMilestone.DataSource = MilestoneManager.GetByProjectId(ProjectId, false);
            DropAffectedMilestone.DataBind();

            //Get Users
            DropAssignedTo.DataSource = users;
            DropAssignedTo.DataBind();

            DropOwned.DataSource = users;
            DropOwned.DataBind();

            DropStatus.DataSource = StatusManager.GetByProjectId(ProjectId);
            DropStatus.DataBind();
        }

        #region IEditProjectControl Members

        /// <summary>
        /// Gets or sets the project id.
        /// </summary>
        /// <value>The project id.</value>
        public int ProjectId
        {
            get => ((BugNetBasePage) Page).ProjectId;
            set => ((BugNetBasePage) Page).ProjectId = value;
        }

        /// <summary>
        /// Gets a value indicating whether [show save button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show save button]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSaveButton => true;

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public void Initialize()
        {
            DueDateLabel.Text = $" ({DateTime.Today.ToShortDateString()})   +";
            BindOptions();
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            if (Page.IsValid)
                return SaveDefaultValues();
            else
                return false;
        }

        /// <summary>
        /// Saves the default values.
        /// </summary>
        /// <returns></returns>
        private bool SaveDefaultValues()
        {
            var privateValue = chkPrivate.Checked ? 1 : 0;
            int? date = 0;
            if (!string.IsNullOrWhiteSpace(DueDate.Text))
                date = int.Parse(DueDate.Text);
            else
                date = null;

            decimal estimation = 0;
            if (!string.IsNullOrWhiteSpace(txtEstimation.Text)) estimation = Convert.ToDecimal(txtEstimation.Text);

            var newDefaultValues = new DefaultValue()
            {
                ProjectId = ProjectId,
                IssueTypeId = DropIssueType.SelectedValue,
                StatusId = DropStatus.SelectedValue,
                OwnerUserName = DropOwned.SelectedValue,
                PriorityId = DropPriority.SelectedValue,
                AffectedMilestoneId = DropAffectedMilestone.SelectedValue,
                AssignedUserName = DropAssignedTo.SelectedValue,
                PrivateVisibility = chkPrivateVisibility.Checked,
                IssueVisibility = privateValue,
                Progress = Convert.ToInt32(ProgressSlider.Text),
                MilestoneId = DropMilestone.SelectedValue,
                CategoryId = DropCategory.SelectedValue,
                DueDate = date,
                Estimation = estimation,
                ResolutionId = DropResolution.SelectedValue,
                StatusVisibility = chkStatusVisibility.Checked,
                OwnedByVisibility = chkOwnedByVisibility.Checked,
                PriorityVisibility = chkPriorityVisibility.Checked,
                AssignedToVisibility = chkAssignedToVisibility.Checked,
                TypeVisibility = chkTypeVisibility.Checked,
                PercentCompleteVisibility = chkPercentCompleteVisibility.Checked,
                MilestoneVisibility = chkMilestoneVisibility.Checked,
                EstimationVisibility = chkEstimationVisibility.Checked,
                ResolutionVisibility = chkResolutionVisibility.Checked,
                AffectedMilestoneVisibility = chkAffectedMilestoneVisibility.Checked,
                AssignedToNotify = chkNotifyAssignedTo.Checked,
                OwnedByNotify = chkNotifyOwner.Checked,
                CategoryVisibility = chkCategoryVisibility.Checked,
                DueDateVisibility = chkDueDateVisibility.Checked,
                StatusEditVisibility = chkStatusEditVisibility.Checked,
                OwnedByEditVisibility = chkOwnedByEditVisibility.Checked,
                PriorityEditVisibility = chkPriorityEditVisibility.Checked,
                AssignedToEditVisibility = chkAssignedToEditVisibility.Checked,
                TypeEditVisibility = chkTypeEditVisibility.Checked,
                PercentCompleteEditVisibility = chkPercentCompleteEditVisibility.Checked,
                MilestoneEditVisibility = chkMilestoneEditVisibility.Checked,
                EstimationEditVisibility = chkEstimationEditVisibility.Checked,
                ResolutionEditVisibility = chkResolutionEditVisibility.Checked,
                AffectedMilestoneEditVisibility = chkAffectedMilestoneEditVisibility.Checked,
                CategoryEditVisibility = chkCategoryEditVisibility.Checked,
                DueDateEditVisibility = chkDueDateEditVisibility.Checked,
                PrivateEditVisibility = chkPrivateEditVisibility.Checked
            };

            return IssueManager.SaveDefaultValues(newDefaultValues);
        }

        #endregion
    }
}