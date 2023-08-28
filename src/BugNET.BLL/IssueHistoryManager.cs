using System;
using System.Collections.Generic;
using BugNET.Common;
using BugNET.DAL;
using BugNET.Entities;
using log4net;

namespace BugNET.BLL
{
    public static class IssueHistoryManager
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Saves the issue history.
        /// </summary>
        /// <param name="issueHistoryToSave">The issue history to save.</param>
        /// <returns></returns>
        public static bool SaveOrUpdate(IssueHistory issueHistoryToSave)
        {
            if (issueHistoryToSave.Id > Globals.NewId) return false;

            var tempId = DataProviderManager.Provider.CreateNewIssueHistory(issueHistoryToSave);

            if (tempId <= 0) return false;

            issueHistoryToSave.Id = tempId;

            if (issueHistoryToSave.TriggerLastUpdateChange)
                DataProviderManager.Provider.UpdateIssueLastUpdated(issueHistoryToSave.IssueId,
                    issueHistoryToSave.CreatedUserName);

            return true;
        }

        /// <summary>
        /// Gets the BugHistory by issue id.
        /// </summary>
        /// <param name="issueId">The issue id.</param>
        /// <returns></returns>
        public static List<IssueHistory> GetByIssueId(int issueId)
        {
            if (issueId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(issueId));

            return DataProviderManager.Provider.GetIssueHistoryByIssueId(issueId);
        }
    }
}