using System.Linq;
using BugNET.Entities;
using System.Collections.Generic;
using log4net;

namespace BugNET.BLL
{
	/// <summary>
	/// Summary description forCategoryTree.
	/// </summary>
	public class CategoryTree
	{
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    private int compIndent = 1;
        private List<Category> unSortedCats;
		private List<Category> sortedCats;

        /// <summary>
        /// Gets the component tree by project id.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
		public List<Category> GetCategoryTreeByProjectId(int projectId) 
		{
  
            sortedCats = new List<Category>();
			unSortedCats = CategoryManager.GetByProjectId(projectId);
			foreach(var parentCat in GetTopLevelCategories() ) 
			{
				sortedCats.Add( parentCat );
				BindSubCategories(parentCat.Id);
			}
			return sortedCats;
		}


        /// <summary>
        /// Binds the sub categories.
        /// </summary>
        /// <param name="parentId">The parent id.</param>
        private void BindSubCategories(int parentId) 
		{
			foreach(var childCat in GetChildCategories(parentId) )
			{
			    var categoryName = string.Concat(DisplayIndent(), childCat.Name);
                sortedCats.Add(new Category { Name = categoryName, Id = childCat.Id });
				compIndent ++;
				BindSubCategories(childCat.Id);
				compIndent --;
			}
		}

        /// <summary>
        /// Gets the top level categories.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Category> GetTopLevelCategories()
            => unSortedCats.Where(cat => cat.ParentCategoryId == 0).ToList();

        /// <summary>
        /// Gets the child categories.
        /// </summary>
        /// <param name="parentId">The parent id.</param>
        /// <returns></returns>
        private IEnumerable<Category> GetChildCategories(int parentId)
            => unSortedCats.Where(cat => cat.ParentCategoryId == parentId).ToList();

        /// <summary>
        /// Displays the indent.
        /// </summary>
        /// <returns></returns>
        private string DisplayIndent()
            => new string('-', compIndent) + " ";
    }
}
