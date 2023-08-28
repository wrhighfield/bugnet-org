using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using BugNET.DAL;
using BugNET.Entities;
using log4net;
using BugNET.Common;

namespace BugNET.BLL
{
    public static class ProjectManager
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <returns></returns>
        public static bool SaveOrUpdate(Project entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.Name)) throw new ArgumentException("The project name cannot be empty or null");

            if (entity.Id > 0)
                return Update(entity);

            entity.UploadPath = Guid.NewGuid().ToString();
            var tempId = DataProviderManager.Provider.CreateNewProject(entity);
            if (tempId <= Globals.NewId)
                return false;

            entity.Id = tempId;

            CustomFieldManager.UpdateCustomFieldView(entity.Id);

            try
            {
                //create default roles for new project.
                RoleManager.CreateDefaultProjectRoles(entity.Id);
            }
            catch (Exception ex)
            {
                if (Log.IsErrorEnabled)
                    Log.Error(
                        string.Format(
                            LoggingManager.GetErrorMessageResource("CouldNotCreateDefaultProjectRoles"),
                            $"ProjectID= {entity.Id}"), ex);
                return false;
            }

            //create attachment directory
            if (HostSettingManager.Get(HostSettingNames.AttachmentStorageType, 0) !=
                (int) IssueAttachmentStorageTypes.FileSystem) return true;
            {
                var uploadPath = string.Concat(HostSettingManager.Get(HostSettingNames.AttachmentUploadPath), entity.UploadPath);
                if(uploadPath.StartsWith("~"))
                {
                    uploadPath = HttpContext.Current.Server.MapPath(uploadPath);
                }


                try
                {
                    // BGN-1909
                    // Better sanitization of Upload Paths
                    if (!Utilities.CheckUploadPath(uploadPath))
                        throw new InvalidDataException(LoggingManager.GetErrorMessageResource("UploadPathInvalid"));

                    Directory.CreateDirectory(uploadPath);
                }
                catch (Exception ex)
                {
                    if (Log.IsErrorEnabled)
                        Log.Error(
                            string.Format(
                                LoggingManager.GetErrorMessageResource("CouldNotCreateUploadDirectory"), uploadPath), ex);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the project by id.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        public static Project GetById(int projectId)
        {
            // validate input
            if (projectId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(projectId));

            return DataProviderManager.Provider.GetProjectById(projectId);
        }

        /// <summary>
        /// Gets the project image.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        public static ProjectImage GetProjectImageById(int projectId)
        {
            // validate input
            if (projectId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(projectId));

            return DataProviderManager.Provider.GetProjectImageById(projectId);
        }

        /// <summary>
        /// Deletes the project image.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        public static bool DeleteProjectImageById(int projectId)
        {
            // validate input
            if (projectId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(projectId));

            return DataProviderManager.Provider.DeleteProjectImage(projectId);
        }

        /// <summary>
        /// Gets the project by code.
        /// </summary>
        /// <param name="projectCode">The project code.</param>
        /// <returns></returns>
        public static Project GetByCode(string projectCode)
        {
            // validate input
            if (string.IsNullOrEmpty(projectCode)) throw new ArgumentOutOfRangeException(nameof(projectCode));

            return DataProviderManager.Provider.GetProjectByCode(projectCode);
        }

        /// <summary>
        /// Gets all projects.
        /// </summary>
        /// <returns></returns>
        public static List<Project> GetAllProjects(bool? activeOnly = true)
        {
            return DataProviderManager.Provider.GetAllProjects(activeOnly);
        }

        /// <summary>
        /// Gets the public projects.
        /// </summary>
        /// <returns></returns>
        public static List<Project> GetPublicProjects()
            => DataProviderManager.Provider.GetPublicProjects();

        /// <summary>
        /// Gets the name of the projects by user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public static List<Project> GetByMemberUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentOutOfRangeException(nameof(userName));

            return GetByMemberUserName(userName, true);
        }

        /// <summary>
        /// Gets the name of the projects by user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <returns></returns>
        public static List<Project> GetByMemberUserName(string userName, bool activeOnly)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentOutOfRangeException(nameof(userName));

            return DataProviderManager.Provider.GetProjectsByMemberUserName(userName, activeOnly);

        }

        /// <summary>
        /// Adds the user to project.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        public static bool AddUserToProject(string userName, int projectId)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentOutOfRangeException(nameof(userName));
            if (projectId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(projectId));

            return DataProviderManager.Provider.AddUserToProject(userName, projectId);
        }

        /// <summary>
        /// Removes the user from project.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        public static bool RemoveUserFromProject(string userName, int projectId)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentOutOfRangeException(nameof(userName));
            if (projectId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(projectId));

            if (!DataProviderManager.Provider.RemoveUserFromProject(userName, projectId)) return true;
            //Remove the user from any project notifications.
            var notifications = ProjectNotificationManager.GetByUsername(userName);

            if (notifications.Count <= 0) return true;
            foreach (var notify in notifications)
                ProjectNotificationManager.Delete(notify.ProjectId, userName);

            return true;
        }

        /// <summary>
        /// Determines whether [is project member] [the specified user id].
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="projectId">The project id.</param>
        /// <returns>
        /// 	<c>true</c> if [is project member] [the specified user id]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsUserProjectMember(string userName, int projectId)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentOutOfRangeException(nameof(userName));
            if (projectId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(projectId));

            return DataProviderManager.Provider.IsUserProjectMember(userName, projectId);
        }

        /// <summary>
        /// Deletes the project.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        public static bool Delete(int projectId)
        {
            if (projectId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(projectId));

            var uploadPath = GetById(projectId).UploadPath;

            if (!DataProviderManager.Provider.DeleteProject(projectId)) return false;
            DeleteProjectCustomView(projectId);

            try
            {

                uploadPath = string.Concat(HostSettingManager.Get(HostSettingNames.AttachmentUploadPath), uploadPath);
                if(uploadPath.StartsWith("~"))
                {
                    uploadPath = HttpContext.Current.Server.MapPath(uploadPath);
                }

                Directory.Delete(uploadPath, true);
            }
            catch (Exception ex)
            {
                Log.Error(string.Format(LoggingManager.GetErrorMessageResource("DeleteProjectUploadFolderError"), uploadPath, projectId), ex);
            }

            return true;
        }

        /// <summary>
        /// Delete the project specific custom view
        /// </summary>
        /// <param name="projectId">The project id for the custom fields for the project</param>
        private static bool DeleteProjectCustomView(int projectId)
        {
            try
            {
                var viewName = string.Format(Globals.ProjectCustomFieldsViewName, projectId);
                var sql = string.Concat("IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'", viewName, "') AND OBJECTPROPERTY(id, N'IsView') = 1) DROP VIEW ", viewName);
                DataProviderManager.Provider.ExecuteScript(new[] { sql });
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return false;
        }

        /// <summary>
        /// Clones the project.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <returns></returns>
        public static int CloneProject(int projectId, string projectName)
        {
            if (projectId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(projectId));
            if (string.IsNullOrEmpty(projectName)) throw new ArgumentNullException(nameof(projectName));

            var newProjectId = DataProviderManager.Provider.CloneProject(projectId, projectName, Security.GetUserName());

            if (newProjectId == 0) return 0;
            var newProject = GetById(newProjectId);

            CustomFieldManager.UpdateCustomFieldView(newProjectId);

            try
            {
                if (newProject.AllowAttachments && HostSettingManager.Get(HostSettingNames.AttachmentStorageType, 0) == (int)IssueAttachmentStorageTypes.FileSystem)
                {
                    // set upload path to new Guid
                    newProject.UploadPath = Guid.NewGuid().ToString();

                    DataProviderManager.Provider.UpdateProject(newProject);

                    var fullPath = string.Concat(HostSettingManager.Get(HostSettingNames.AttachmentUploadPath), newProject.UploadPath);

                    if (fullPath.StartsWith("~"))
                    {
                        fullPath = HttpContext.Current.Server.MapPath(fullPath);
                    }

                    Directory.CreateDirectory(fullPath);
                }
            }
            catch (Exception ex)
            {
                if (Log.IsErrorEnabled)
                    Log.Error(string.Format(LoggingManager.GetErrorMessageResource("CreateProjectUploadFolderError"), newProject.UploadPath, projectId), ex);
            }

            HttpContext.Current.Cache.Remove("RolePermission");

            return newProjectId;

        }

        /// <summary>
        /// Gets the road map progress.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <param name="milestoneId">The milestone id.</param>
        /// <returns>total number of issues and total number of close issues</returns>
        public static int[] GetRoadMapProgress(int projectId, int milestoneId)
        {
            if (projectId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(projectId));
            if (milestoneId < -1) throw new ArgumentNullException(nameof(milestoneId));

            return DataProviderManager.Provider.GetProjectRoadmapProgress(projectId, milestoneId);
        }

        /// <summary>
        /// Gets the users and roles by project id.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        public static List<MemberRoles> GetProjectMembersRoles(int projectId)
        {
            return DataProviderManager.Provider.GetProjectMembersRoles(projectId);
        }

        #region Private Methods

        /// <summary>
        /// Updates the project.
        /// </summary>
        /// <returns></returns>
        private static bool Update(Project entity)
        {
            var project = GetById(entity.Id);
            return DataProviderManager.Provider.UpdateProject(entity);

        }
        #endregion
    }
}
