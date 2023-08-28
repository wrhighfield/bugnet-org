﻿using System;
using System.Collections.Generic;
using BugNET.Common;
using BugNET.DAL;
using BugNET.Entities;
using log4net;

namespace BugNET.BLL
{
    public static class CustomFieldSelectionManager
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <param name="entity">The custom field selection to save.</param>
        /// <returns></returns>
        public static bool SaveOrUpdate(CustomFieldSelection entity)
        {
            if (entity.Id > Globals.NewId)
                return DataProviderManager.Provider.UpdateCustomFieldSelection(entity);

            var tempId = DataProviderManager.Provider.CreateNewCustomFieldSelection(entity);

            if (tempId <= 0)
                return false;

            entity.Id = tempId;
            return true;
        }

        /// <summary>
        /// Deletes the custom field selection.
        /// </summary>
        /// <param name="customFieldSelectionId">The custom field selection id.</param>
        /// <returns></returns>
        public static bool Delete(int customFieldSelectionId)
        {
            if (customFieldSelectionId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(customFieldSelectionId));

            return DataProviderManager.Provider.DeleteCustomFieldSelection(customFieldSelectionId);
        }


        /// <summary>
        /// Gets the custom fields selections by custom field id.
        /// </summary>
        /// <param name="customFieldId">The custom field id.</param>
        /// <returns></returns>
        public static List<CustomFieldSelection> GetByCustomFieldId(int customFieldId)
        {
            if (customFieldId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(customFieldId));

            return DataProviderManager.Provider.GetCustomFieldSelectionsByCustomFieldId(customFieldId);
        }

        /// <summary>
        /// Gets the custom field selection by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static CustomFieldSelection GetById(int id)
            => DataProviderManager.Provider.GetCustomFieldSelectionById(id);
    }
}
