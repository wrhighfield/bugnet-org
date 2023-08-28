using System;
using System.Collections.Generic;
using BugNET.Common;
using BugNET.DAL;
using BugNET.Entities;
using log4net;

namespace BugNET.BLL
{
    public static class IssueTypeManager
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <returns></returns>
        public static bool SaveOrUpdate(IssueType entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.ProjectId <= Globals.NewId) throw new ArgumentException("Cannot save issue type, the project id is invalid");
            if (string.IsNullOrEmpty(entity.Name)) throw new ArgumentException("The issue type name cannot be empty or null");

            if (entity.Id > Globals.NewId)
                return DataProviderManager.Provider.UpdateIssueType(entity);

            var tempId = DataProviderManager.Provider.CreateNewIssueType(entity);

            if (tempId <= 0)
                return false;

            entity.Id = tempId;
            return true;
        }


        /// <summary>
        /// Gets the IssueType by id.
        /// </summary>
        /// <param name="issueTypeId">The IssueType id.</param>
        /// <returns></returns>
        public static IssueType GetById(int issueTypeId)
        {
            if (issueTypeId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(issueTypeId));

            return DataProviderManager.Provider.GetIssueTypeById(issueTypeId);
        }

        /// <summary>
        /// Deletes the Issue Type.
        /// </summary>
        /// <param name="id">The id for the item to be deleted.</param>
        /// <param name="cannotDeleteMessage">If</param>
        /// <returns></returns>
        public static bool Delete(int id, out string cannotDeleteMessage)
        {
            if (id <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(id));

            var entity = GetById(id);

            cannotDeleteMessage = string.Empty;

            if (entity == null) return true;

            var canBeDeleted = DataProviderManager.Provider.CanDeleteIssueType(entity.Id);

            if (canBeDeleted)
                return DataProviderManager.Provider.DeleteIssueType(entity.Id);

            cannotDeleteMessage = ResourceStrings.GetGlobalResource(GlobalResources.Exceptions, "DeleteItemAssignedToIssueError");
            cannotDeleteMessage = string.Format(cannotDeleteMessage, entity.Name,  ResourceStrings.GetGlobalResource(GlobalResources.SharedResources, "IssueType", "issue type").ToLower());

            return false;
        }

        /// <summary>
        /// Gets the issue type by project id.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        public static List<IssueType> GetByProjectId(int projectId)
        {
            if (projectId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(projectId));
            return DataProviderManager.Provider.GetIssueTypesByProjectId(projectId);
        }
    }
}
