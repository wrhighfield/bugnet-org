namespace BugNET.Common
{
    public delegate void ActionEventHandler(object sender, ActionEventArgs args);

    /// <summary>
    /// Global constants, enumerations and properties
    /// </summary>
    public static class Globals
    {
        #region Public Constants

        //Cookie Constants
        public const string UserCookie = "BugNETUser";
        public const string IssueColumns = "issuecolumns";

        public const string ConfigFolder = @"\Config\";
        public const string UploadToken = "UploadToken";
        public const int UploadFolderLimit = 64;

        public const string SkipProjectIntro = "skipprojectintro";
        public const string UnassignedDisplayText = "none";

        public const int NewId = 0;
        public const string EmptyGuid = "00000000-0000-0000-0000-000000000000";

        public const string SuperUserRole = "Super Users";

        public static readonly string[] DefaultRoles =
            {"Project Administrators", "Read Only", "Reporter", "Developer", "Quality Assurance"};

        public static readonly string ProjectAdministratorRole = DefaultRoles[0];

        public const string ProjectCustomFieldsViewName = "BugNet_P{0}_CFV";
        public const string ProjectCustomFieldsPrefix = "bgn_cf_";

        public const string UserCustomFieldsViewName = "BugNet_USER_CFV";
        public const string UserCustomFieldsPrefix = "bgn_ucf_";

        /// <summary>
        /// The default length of short comments (if not specified).
        /// </summary>
        public const int DefaultShortCommentLength = 100;


        /// <summary>
        /// Default read only role permissions
        /// </summary>
        public static readonly int[] ReadOnlyPermissions =
        {
            (int) Permission.SubscribeIssue
        };

        /// <summary>
        /// Default reporter role permissions
        /// </summary>
        public static readonly int[] ReporterPermissions =
        {
            (int) Permission.AddIssue,
            (int) Permission.AddComment,
            (int) Permission.OwnerEditComment,
            (int) Permission.SubscribeIssue,
            (int) Permission.AddAttachment,
            (int) Permission.AddRelated,
            (int) Permission.AddParentIssue,
            (int) Permission.AddSubIssue
        };

        /// <summary>
        /// Default developer role permissions
        /// </summary>
        public static readonly int[] DeveloperPermissions =
        {
            (int) Permission.AddIssue,
            (int) Permission.AddComment,
            (int) Permission.AddAttachment,
            (int) Permission.AddRelated,
            (int) Permission.AddTimeEntry,
            (int) Permission.AddParentIssue,
            (int) Permission.AddSubIssue,
            (int) Permission.AddQuery,
            (int) Permission.OwnerEditComment,
            (int) Permission.SubscribeIssue,
            (int) Permission.EditIssue,
            (int) Permission.AssignIssue,
            (int) Permission.ChangeIssueStatus
        };

        /// <summary>
        /// Default QA role permissions
        /// </summary>
        public static readonly int[] QualityAssurancePermissions =
        {
            (int) Permission.AddIssue,
            (int) Permission.AddComment,
            (int) Permission.AddAttachment,
            (int) Permission.AddRelated,
            (int) Permission.AddTimeEntry,
            (int) Permission.AddParentIssue,
            (int) Permission.AddSubIssue,
            (int) Permission.AddQuery,
            (int) Permission.OwnerEditComment,
            (int) Permission.SubscribeIssue,
            (int) Permission.EditIssue,
            (int) Permission.EditIssueTitle,
            (int) Permission.AssignIssue,
            (int) Permission.CloseIssue,
            (int) Permission.DeleteIssue,
            (int) Permission.ChangeIssueStatus
        };

        /// <summary>
        /// Default project administrator role permissions
        /// </summary>
        public static readonly int[] AdministratorPermissions =
        {
            (int) Permission.AddIssue,
            (int) Permission.AddComment,
            (int) Permission.AddAttachment,
            (int) Permission.AddRelated,
            (int) Permission.AddTimeEntry,
            (int) Permission.AddParentIssue,
            (int) Permission.AddSubIssue,
            (int) Permission.AddQuery,
            (int) Permission.OwnerEditComment,
            (int) Permission.SubscribeIssue,
            (int) Permission.EditIssue,
            (int) Permission.EditComment,
            (int) Permission.EditIssueDescription,
            (int) Permission.EditIssueTitle,
            (int) Permission.EditQuery,
            (int) Permission.DeleteQuery,
            (int) Permission.DeleteAttachment,
            (int) Permission.DeleteComment,
            (int) Permission.DeleteIssue,
            (int) Permission.DeleteRelated,
            (int) Permission.DeleteTimeEntry,
            (int) Permission.DeleteQuery,
            (int) Permission.DeleteSubIssue,
            (int) Permission.DeleteParentIssue,
            (int) Permission.AssignIssue,
            (int) Permission.CloseIssue,
            (int) Permission.AdminEditProject,
            (int) Permission.ChangeIssueStatus,
            (int) Permission.ReopenIssue
        };

        #endregion
    }
}