using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using BugNET.Common;
using BugNET.DAL;
using BugNET.Entities;
using log4net;

namespace BugNET.BLL
{
    public static class IssueAttachmentManager
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // BGN-2090
        /// <summary>
        /// Validates if the requesting user can download the attachment
        /// </summary>
        /// <param name="attachmentId">The attachment id to fetch</param>
        /// <returns>An attachment if the security checks pass</returns>
        /// <remarks>
        /// The following defines the logic for a attachment NOT to be returned
        /// <list type="number">
        /// <item><description>When the user is anon and anon access is disabled</description></item>
        /// <item><description>When the project or the issue is deleted / disabled</description></item>
        /// <item><description>When the project is private and (the user does not have project access or elevated permissions)</description></item>
        /// <item><description>When the issue is private and (the user is neither the creator of the issue or assigned to the issue) or (the user does not have elevated permissions)</description></item>
        /// </list>
        /// </remarks>
        public static IssueAttachment GetAttachmentForDownload(int attachmentId)
        {
            // validate input
            if (attachmentId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(attachmentId));

            var requestingUser = string.Empty;

            if (HttpContext.Current.User == null)
                return DataProviderManager.Provider.GetAttachmentForDownload(attachmentId, requestingUser);
            if (HttpContext.Current.User.Identity.IsAuthenticated)
                requestingUser = Security.GetUserName();

            return DataProviderManager.Provider.GetAttachmentForDownload(attachmentId, requestingUser);
        }

        /// <summary>
        /// Strips the unique guid from the file system version of the file
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns></returns>
        public static string StripGuidFromFileName(string fileName)
        {
            var guidLength = Globals.EmptyGuid.Length;
            var guidEnd = fileName.LastIndexOf(".", StringComparison.Ordinal);
            var guidStart = guidEnd - guidLength;
            if (guidStart > -1)
                fileName = string.Concat(fileName.Substring(0, guidStart), fileName.Substring(guidEnd + 1));

            return fileName;
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <param name="entity">The issue attachment to save.</param>
        /// <returns></returns>
        public static bool SaveOrUpdate(IssueAttachment entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.IssueId <= Globals.NewId)
                throw new ArgumentException("Cannot save issue attachment, the issue id is invalid");
            if (string.IsNullOrEmpty(entity.FileName))
                throw new ArgumentException("The attachment file name cannot be empty or null");

            if (!IsValidFile(entity.FileName, out var invalidReason)) throw new ApplicationException(invalidReason);

            //Start new save attachment code
            if (entity.Attachment.Length <= 0) return false;
            // save the file to the upload directory
            var projectId = IssueManager.GetById(entity.IssueId).ProjectId;
            var project = ProjectManager.GetById(projectId);

            if (!project.AllowAttachments) return false;
            entity.ContentType = entity.ContentType.Replace("/x-png", "/png");

            if (entity.ContentType == "image/bmp")
            {
                using (var ms = new MemoryStream(entity.Attachment, 0, entity.Attachment.Length))
                {
                    ms.Write(entity.Attachment, 0, entity.Attachment.Length);
                    var img = Image.FromStream(ms);
                    img.Save(ms, ImageFormat.Png);
                    ms.Seek(0, SeekOrigin.Begin);
                    entity.Attachment = ms.ToArray();
                }

                entity.ContentType = "image/png";
                entity.FileName = Path.ChangeExtension(entity.FileName, "png");
            }

            entity.Size = entity.Attachment.Length;

            if (HostSettingManager.Get(HostSettingNames.AttachmentStorageType, 0) ==
                (int) IssueAttachmentStorageTypes.Database)
            {
                //save the attachment record to the database.
                var tempId = DataProviderManager.Provider.CreateNewIssueAttachment(entity);
                if (tempId <= 0) return false;
                entity.Id = tempId;
                return true;
            }

            var projectPath = project.UploadPath;

            try
            {
                if (projectPath.Length == 0)
                    throw new ApplicationException(
                        string.Format(LoggingManager.GetErrorMessageResource("UploadPathNotDefined"), project.Name));

                var attachmentGuid = Guid.NewGuid();
                var attachmentBytes = entity.Attachment;
                entity.Attachment = null; //set attachment to null    
                entity.FileName =
                    $"{Path.GetFileNameWithoutExtension(entity.FileName)}.{attachmentGuid}{Path.GetExtension(entity.FileName)}";

                var uploadedFilePath = string.Empty;

                // added by WRH 2012-08-18
                // this to fix the issue where attachments from the mailbox reader cannot be saved due to the lack of a http context.
                // we need to supply the actual folder path on the entity
                if (HttpContext.Current != null)
                {
                    uploadedFilePath =
                        $@"{$"{HostSettingManager.Get(HostSettingNames.AttachmentUploadPath)}{projectPath}"}\{entity.FileName}";

                    if (uploadedFilePath.StartsWith("~"))
                        uploadedFilePath = HttpContext.Current.Server.MapPath(uploadedFilePath);
                }
                else
                {
                    if (entity.ProjectFolderPath.Trim().Length > 0)
                        uploadedFilePath = $"{entity.ProjectFolderPath}\\{entity.FileName}";
                }

                //save the attachment record to the database.
                var tempId = DataProviderManager.Provider.CreateNewIssueAttachment(entity);

                if (tempId <= 0) return false;
                entity.Id = tempId;

                //save file to file system
                var fi = new FileInfo(uploadedFilePath);

                if (!Directory.Exists(fi.DirectoryName))
                    Directory.CreateDirectory(fi.DirectoryName);

                File.WriteAllBytes(uploadedFilePath, attachmentBytes);

                return true;
            }
            catch (DirectoryNotFoundException ex)
            {
                if (Log.IsErrorEnabled)
                    Log.Error(string.Format(LoggingManager.GetErrorMessageResource("UploadPathNotFound"), projectPath),
                        ex);
                throw;
            }
            catch (Exception ex)
            {
                if (Log.IsErrorEnabled)
                    Log.Error(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Gets all IssueAttachments for an issue
        /// </summary>
        /// <param name="issueId"></param>
        /// <returns></returns>
        public static List<IssueAttachment> GetByIssueId(int issueId)
        {
            // validate input
            if (issueId <= Globals.NewId)
                throw new ArgumentOutOfRangeException(nameof(issueId));

            return DataProviderManager.Provider.GetIssueAttachmentsByIssueId(issueId);
        }

        /// <summary>
        /// Gets an IssueAttachment by id
        /// </summary>
        /// <param name="attachmentId">The IssueAttachment id.</param>
        /// <returns></returns>
        public static IssueAttachment GetById(int attachmentId)
        {
            // validate input
            if (attachmentId <= Globals.NewId)
                throw new ArgumentOutOfRangeException(nameof(attachmentId));

            return DataProviderManager.Provider.GetIssueAttachmentById(attachmentId);
        }

        /// <summary>
        /// Deletes the IssueAttachment.
        /// </summary>
        /// <param name="issueAttachmentId">The issue attachment id.</param>
        /// <returns></returns>
        public static bool Delete(int issueAttachmentId)
        {
            var att = GetById(issueAttachmentId);
            var issue = IssueManager.GetById(att.IssueId);
            var project = ProjectManager.GetById(issue.ProjectId);

            if (!DataProviderManager.Provider.DeleteIssueAttachment(issueAttachmentId)) return true;
            try
            {
                var history = new IssueHistory
                {
                    IssueId = att.IssueId,
                    CreatedUserName = Security.GetUserName(),
                    DateChanged = DateTime.Now,
                    FieldChanged =
                        ResourceStrings.GetGlobalResource(GlobalResources.SharedResources, "Attachment", "Attachment"),
                    OldValue = att.FileName,
                    NewValue = ResourceStrings.GetGlobalResource(GlobalResources.SharedResources, "Deleted", "Deleted"),
                    TriggerLastUpdateChange = true
                };

                IssueHistoryManager.SaveOrUpdate(history);

                var changes = new List<IssueHistory> {history};

                IssueNotificationManager.SendIssueNotifications(att.IssueId, changes);
            }
            catch (Exception ex)
            {
                if (Log.IsErrorEnabled) Log.Error(ex);
            }

            if (HostSettingManager.Get(HostSettingNames.AttachmentStorageType, 0) !=
                (int) IssueAttachmentStorageTypes.FileSystem) return true;
            {
                //delete IssueAttachment from file system.
                try
                {
                    var filePath = string.Format(@"{2}{0}\{1}", project.UploadPath, att.FileName,
                        HostSettingManager.Get(HostSettingNames.AttachmentUploadPath));

                    if (filePath.StartsWith("~")) filePath = HttpContext.Current.Server.MapPath(filePath);

                    if (File.Exists(filePath))
                        File.Delete(filePath);
                    else
                        Log.Info(
                            $"Failed to locate file {filePath} to delete, it may have been moved or manually deleted");
                }
                catch (Exception ex)
                {
                    //set user to log4net context, so we can use %X{user} in the appenders
                    if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                        MDC.Set("user", HttpContext.Current.User.Identity.Name);

                    if (Log.IsErrorEnabled)
                        Log.Error(
                            $"Error Deleting IssueAttachment - {$"{project.UploadPath}\\{att.FileName}"}", ex);

                    throw new ApplicationException(LoggingManager.GetErrorMessageResource("AttachmentDeleteError"), ex);
                }
            }
            return true;
        }

        ///// <summary>
        ///// Stewart Moss
        ///// Apr 14 2010 
        ///// 
        ///// Performs a query containing any number of query clauses on a certain IssueID
        ///// </summary>
        ///// <param name="issueId"></param>
        ///// <param name="queryClauses"></param>
        ///// <returns></returns>
        //public static List<IssueAttachment> PerformQuery(int issueId, List<QueryClause> queryClauses)
        //{
        //    if (issueId < 0)
        //        throw new ArgumentOutOfRangeException("issueId", "must be bigger than 0");

        //    queryClauses.Add(new QueryClause("AND", "IssueId", "=", issueId.ToString(), SqlDbType.Int, false));

        //    return PerformQuery(queryClauses);
        //}

        ///// <summary>
        ///// Stewart Moss
        ///// Apr 14 2010 8:30 pm
        ///// 
        ///// Performs any query containing any number of query clauses
        ///// WARNING! Will expose the entire IssueAttachment table, regardless of 
        ///// project level privileges. (that's why its private for now)
        ///// </summary>        
        ///// <param name="queryClauses"></param>
        ///// <returns></returns>
        //private static List<IssueAttachment> PerformQuery(List<QueryClause> queryClauses)
        //{
        //    if (queryClauses == null)
        //        throw new ArgumentNullException("queryClauses");

        //    var lst = new List<IssueAttachment>();
        //    DataProviderManager.Provider.PerformGenericQuery(ref lst, queryClauses, @"SELECT a.*, b.UserName as CreatorUserName, a.Userid as CreatorUserID, b.Username as CreatorDisplayName from BugNet_IssueAttachment as a, aspnet_Users as b  WHERE a.UserId=b.UserID ", @" ORDER BY IssueAttachmentId DESC");

        //    return lst;
        //}


        /// <summary>
        /// Validate the file if we can attach it or not
        /// </summary>
        /// <param name="fileName">The file name to validate</param>
        /// <param name="inValidReason">The reason the validation failed</param>
        /// <returns>True if the file is valid, otherwise false</returns>
        public static bool IsValidFile(string fileName, out string inValidReason)
        {
            inValidReason = string.Empty;
            fileName = fileName.Trim();
            fileName = Path.GetFileName(fileName);

            // empty file name
            if (string.IsNullOrEmpty(fileName))
            {
                inValidReason = LoggingManager.GetErrorMessageResource("InvalidFileName");
                return false;
            }

            var allowedFileTypes =
                HostSettingManager.Get(HostSettingNames.AllowedFileExtensions, string.Empty).Split(';');
            var fileExt = Path.GetExtension(fileName);
            var fileOk = false;

            if (allowedFileTypes.Length > 0 && string.CompareOrdinal(allowedFileTypes[0], "*.*") == 0)
            {
                fileOk = true;
            }
            else
            {
                if (allowedFileTypes
                    .Select(fileType => fileType.Substring(fileType.LastIndexOf(".", StringComparison.Ordinal)))
                    .Any(newFileType => string.Compare(newFileType, fileExt, StringComparison.Ordinal) == 0))
                    fileOk = true;
            }

            // valid file type
            if (!fileOk)
            {
                inValidReason = string.Format(LoggingManager.GetErrorMessageResource("InvalidFileType"), fileName);
                return false;
            }

            // illegal filename characters
            if (!Path.GetInvalidFileNameChars()
                    .Any(invalidFileNameChar => fileName.Contains(invalidFileNameChar))) return true;
            inValidReason = string.Format(LoggingManager.GetErrorMessageResource("InvalidFileName"), fileName);
            return false;
        }
    }
}