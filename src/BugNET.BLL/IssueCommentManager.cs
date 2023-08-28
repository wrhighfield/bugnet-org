using System;
using System.Collections.Generic;
using System.Data;
using BugNET.Common;
using BugNET.DAL;
using BugNET.Entities;
using log4net;

namespace BugNET.BLL
{
    public static class IssueCommentManager
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Static Methods

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <returns></returns>
        public static bool SaveOrUpdate(IssueComment entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.IssueId <= Globals.NewId)
                throw new ArgumentException("Cannot save issue comment, the issue id is invalid");
            if (string.IsNullOrEmpty(entity.Comment))
                throw new ArgumentException("The issue comment cannot be empty or null");

            if (entity.Id > Globals.NewId)
                return DataProviderManager.Provider.UpdateIssueComment(entity);

            var tempId = DataProviderManager.Provider.CreateNewIssueComment(entity);

            if (tempId <= Globals.NewId)
                return false;

            entity.Id = tempId;

            IssueNotificationManager.SendNewIssueCommentNotification(entity.IssueId, GetById(entity.Id));

            return true;
        }

        /// <summary>
        /// Gets a "short text" version of the comment.
        /// This is handy for lists or summary data. (perhaps something in notifications too).
        /// The returned text is "how much" chars long, and is centered on the middle-point
        /// (if the string is long enough).
        /// This strips HTML from the comment and does a bit of cleaning of the output.
        /// BGN-1732 - IssueComment needs a new Property to read short comments for list displays
        /// </summary>
        /// <param name="comment">The comment.</param>
        /// <param name="shortCommentLength">How long must the string be.</param>
        /// <returns></returns>
        public static string GetShortTextComment(string comment,
            int shortCommentLength = Globals.DefaultShortCommentLength)
        {
            var temporaryComment = comment.Trim();

            if (temporaryComment == "") return temporaryComment;

            temporaryComment = Utilities.StripHtml(temporaryComment).Trim();

            // Now fix up any other characters we dont want. 
            // This is a quick summary of a comment after all.
            temporaryComment = temporaryComment.Replace("\t", " ");
            temporaryComment = temporaryComment.Replace("\r", " ");
            temporaryComment = temporaryComment.Replace("\n", " ");

            // Keep replacing double-spaces until there are none left.
            while (temporaryComment.IndexOf("  ", StringComparison.Ordinal) != -1)
                temporaryComment = temporaryComment.Replace("  ", " ");

            // Give it one last trim
            temporaryComment = temporaryComment.Trim();

            // Now find the centre of the string
            var commentLength = temporaryComment.Length;
            var length = commentLength / 2;

            // and create a string "how much" chars long centered on the middle-point
            // if the string is long enough.
            if (length > shortCommentLength)
                // Longer than the string
                temporaryComment = temporaryComment.Substring(length - shortCommentLength / 2, shortCommentLength);

            return temporaryComment;
        }

        /// <summary>
        /// Gets all comments for a issue
        /// </summary>
        /// <param name="issueId"></param>
        /// <returns>List of Comment Objects</returns>
        public static List<IssueComment> GetByIssueId(int issueId)
        {
            return DataProviderManager.Provider.GetIssueCommentsByIssueId(issueId);
        }

        /// <summary>
        /// Delete a comment by Id
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns>True if successful</returns>
        public static bool Delete(int commentId)
        {
            return DataProviderManager.Provider.DeleteIssueCommentById(commentId);
        }

        /// <summary>
        /// Gets the issue comment by id.
        /// </summary>
        /// <param name="issueCommentId">The issue comment id.</param>
        /// <returns></returns>
        public static IssueComment GetById(int issueCommentId)
        {
            return DataProviderManager.Provider.GetIssueCommentById(issueCommentId);
        }

        /// <summary>
        /// Stewart Moss
        /// Apr 14 2010 
        /// 
        /// Performs a query containing any number of query clauses on a certain IssueID
        /// </summary>
        /// <param name="issueId"></param>
        /// <param name="queryClauses"></param>
        /// <returns></returns>
        public static List<IssueComment> PerformQuery(int issueId, List<QueryClause> queryClauses)
        {
            if (issueId < 0) throw new ArgumentOutOfRangeException(nameof(issueId), "issueId must be bigger than 0");
            queryClauses.Add(new QueryClause("AND", "IssueId", "=", issueId.ToString(), SqlDbType.Int));

            return PerformQuery(queryClauses);
        }

        /// <summary>
        /// Stewart Moss
        /// Apr 14 2010 8:30 pm
        /// 
        /// Performs any query containing any number of query clauses
        /// WARNING! Will expose the entire IssueComment table, regardless of 
        /// project level privileges. (that's why its private for now)
        /// </summary>        
        /// <param name="queryClauses"></param>
        /// <returns></returns>
        private static List<IssueComment> PerformQuery(List<QueryClause> queryClauses)
        {
            if (queryClauses == null)
                throw new ArgumentNullException(nameof(queryClauses));

            var lst = new List<IssueComment>();
            DataProviderManager.Provider.PerformIssueCommentSearchQuery(ref lst, queryClauses);

            return lst;
        }

        #endregion
    }
}