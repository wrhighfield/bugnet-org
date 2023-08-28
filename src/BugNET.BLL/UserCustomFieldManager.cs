using System;
using System.Collections.Generic;
using BugNET.Common;
using BugNET.DAL;
using BugNET.Entities;
using log4net;

namespace BugNET.BLL
{
    public static class UserCustomFieldManager
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <param name="entity">The custom field to save.</param>
        /// <returns></returns>
        public static bool SaveOrUpdate(UserCustomField entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.Name))
                throw new ArgumentException("The custom field name cannot be empty or null");

            if (entity.Id > Globals.NewId)
                if (DataProviderManager.Provider.UpdateUserCustomField(entity))
                {
                    UpdateCustomFieldView();
                    return true;
                }

            var tempId = DataProviderManager.Provider.CreateNewUserCustomField(entity);

            if (tempId <= 0)
                return false;

            entity.Id = tempId;
            UpdateCustomFieldView();
            return true;
        }

        /// <summary>
        /// Deletes the custom field.
        /// </summary>
        /// <param name="customFieldId">The custom field id.</param>
        /// <returns></returns>
        public static bool Delete(int customFieldId)
        {
            if (customFieldId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(customFieldId));
            var entity = GetById(customFieldId);

            if (entity == null) return true;

            if (!DataProviderManager.Provider.DeleteUserCustomField(entity.Id)) return false;
            UpdateCustomFieldView();
            return true;
        }

        /// <summary>
        /// Saves the custom field values.
        /// </summary>
        /// <param name="issueId">The issue id.</param>
        /// <param name="fields">The fields.</param>
        /// <returns></returns>
        public static bool SaveCustomFieldValues(Guid userId, List<UserCustomField> fields)
        {
            if (fields == null) throw new ArgumentOutOfRangeException(nameof(fields));

            try
            {
                DataProviderManager.Provider.SaveUserCustomFieldValues(userId, fields);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LoggingManager.GetErrorMessageResource("SaveCustomFieldValuesError"), ex);
                return false;
            }
        }

        /// <summary>
        /// Gets all the custom fields
        /// </summary>
        /// <returns></returns>
        public static List<UserCustomField> GetAll()
        {
            return DataProviderManager.Provider.GetUserCustomFields();
        }

        /// <summary>
        /// Gets the custom field by id.
        /// </summary>
        /// <param name="customFieldId">The custom field id.</param>
        /// <returns></returns>
        public static UserCustomField GetById(int customFieldId)
        {
            if (customFieldId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(customFieldId));

            return DataProviderManager.Provider.GetUserCustomFieldById(customFieldId);
        }

        /// <summary>
        /// Gets the custom fields by user id.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<UserCustomField> GetByUserId(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentOutOfRangeException(nameof(userId));

            return DataProviderManager.Provider.GetUserCustomFieldsByUserId(userId);
        }

        /// <summary>
        /// Creates a PIVOT view for the custom fields to make loading them easier for the UI
        /// this will only work with SQL 2005 and higher
        /// </summary>
        private static bool UpdateCustomFieldView()
        {
            // not implemented for some reason
            return false;
        }
    }
}