using System;
using System.Collections.Generic;
using BugNET.Common;
using BugNET.DAL;
using BugNET.Entities;
using log4net;

namespace BugNET.BLL
{
    public static class CategoryManager
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <returns></returns>
        public static bool SaveOrUpdate(Category entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.ProjectId <= Globals.NewId)
                throw new ArgumentException("Cannot save category, the project id is invalid");
            if (string.IsNullOrEmpty(entity.Name))
                throw new ArgumentException("The category name cannot be empty or null");

            if (entity.Id > Globals.NewId)
                return DataProviderManager.Provider.UpdateCategory(entity);

            var tempId = DataProviderManager.Provider.CreateNewCategory(entity);

            if (tempId <= 0)
                return false;

            entity.Id = tempId;
            return true;
        }

        /// <summary>
        /// Deletes the Category.
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public static bool Delete(int categoryId)
        {
            if (categoryId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(categoryId));

            return DataProviderManager.Provider.DeleteCategory(categoryId);
        }

        /// <summary>
        /// Gets the Categories by project ID.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        public static List<Category> GetByProjectId(int projectId)
        {
            if (projectId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(projectId));

            return DataProviderManager.Provider.GetCategoriesByProjectId(projectId);
        }

        /// <summary>
        /// Gets the root Categories by project id.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        public static List<Category> GetRootCategoriesByProjectId(int projectId)
        {
            if (projectId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(projectId));

            return DataProviderManager.Provider.GetRootCategoriesByProjectId(projectId);
        }

        /// <summary>
        /// Gets the root Categories by project id.
        /// </summary>
        /// <param name="categoryId">The Category id.</param>
        /// <returns></returns>
        public static List<Category> GetChildCategoriesByCategoryId(int categoryId)
        {
            if (categoryId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(categoryId));

            return DataProviderManager.Provider.GetChildCategoriesByCategoryId(categoryId);
        }

        /// <summary>
        /// Deletes the child categories by category id.
        /// </summary>
        /// <param name="categoryId">The category id.</param>
        public static void DeleteChildCategoriesByCategoryId(int categoryId)
        {
            if (categoryId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(categoryId));

            var c = GetById(categoryId);

            foreach (var childCategory in GetChildCategoriesByCategoryId(c.Id))
                Delete(childCategory.Id);

            if (c.ChildCount > 0)
                DeleteChildCategoriesByCategoryId(c.Id);
        }

        /// <summary>
        /// Gets the Category by id.
        /// </summary>
        /// <param name="categoryId">The Category id.</param>
        /// <returns></returns>
        public static Category GetById(int categoryId)
        {
            if (categoryId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(categoryId));

            return DataProviderManager.Provider.GetCategoryById(categoryId);
        }
    }
}