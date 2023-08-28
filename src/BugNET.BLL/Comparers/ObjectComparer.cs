using System;
using System.Collections;
using System.Collections.Generic;

namespace BugNET.BLL.Comparers
{
    /// http://aadreja.wordpress.com/2009/02/09/c-sorting-with-objects-on-multiple-fields/
    /// http://www.codeproject.com/KB/recipes/Sorting_with_Objects.aspx
    /// <summary>
    /// Allows multi column sorting of an object
    /// </summary>
    /// <typeparam name="TComparableObject">The type of the comparable object.</typeparam>
    [Serializable]
    public class ObjectComparer<TComparableObject> : IComparer<TComparableObject>
    {
        private string propertyName;
        private bool multiColumn;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectComparer&lt;ComparableObject&gt;"/> class.
        /// </summary>
        public ObjectComparer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectComparer&lt;ComparableObject&gt;"/> class.
        /// </summary>
        /// <param name="pPropertyName">Name of the p_property.</param>
        public ObjectComparer(string pPropertyName)
        {
            //We must have a property name for this comparer to work
            PropertyName = pPropertyName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectComparer&lt;ComparableObject&gt;"/> class.
        /// </summary>
        /// <param name="pPropertyName">Name of the p_property.</param>
        /// <param name="pMultiColumn">if set to <c>true</c> [p_ multi column].</param>
        public ObjectComparer(string pPropertyName, bool pMultiColumn)
        {
            //We must have a property name for this comparer to work
            PropertyName = pPropertyName;
            MultiColumn = pMultiColumn;
        }

        #endregion

        #region Property

        /// <summary>
        /// Gets or sets a value indicating whether [multi column].
        /// </summary>
        /// <value><c>true</c> if [multi column]; otherwise, <c>false</c>.</value>
        public bool MultiColumn
        {
            get => multiColumn;
            set => multiColumn = value;
        }


        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName
        {
            get => propertyName;
            set => propertyName = value;
        }

        #endregion

        #region IComparer<ComparableObject> Members

        /// <summary>
        /// This comparer is used to sort the generic comparer
        /// The constructor sets the PropertyName that is used
        /// by reflection to access that property in the object to 
        /// object compare.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(TComparableObject x, TComparableObject y)
        {
            var t = x.GetType();

            if (multiColumn) // Multi Column Sorting
            {
                var sortExpressions = propertyName.Trim().Split(',');
                foreach (var sortExpression in sortExpressions)
                {
                    string fieldName, direction = "ASC";
                    if (sortExpression.Trim().EndsWith(" DESC"))
                    {
                        fieldName = sortExpression.Replace(" DESC", "").Trim();
                        direction = "DESC";
                    }
                    else
                    {
                        fieldName = sortExpression.Replace(" ASC", "").Trim();
                    }

                    //Get property by name
                    var propertyInfo = t.GetProperty(fieldName);

                    if (propertyInfo != null)
                    {
                        var iResult = Comparer.DefaultInvariant.Compare
                            (propertyInfo.GetValue(x, null), propertyInfo.GetValue(y, null));
                        if (iResult == 0) continue;
                        //Return if not equal
                        return direction == "DESC"
                            ?
                            //Invert order
                            -iResult
                            : iResult;
                    }

                    throw new Exception(
                        $"{fieldName} is not a valid property to sort on. It doesn't exist in the Class.");

                    //Compare values, using IComparable interface of the property's type
                }

                //Objects have the same sort order
                return 0;
            }

            var val = t.GetProperty(PropertyName);
            if (val != null)
                return Comparer.DefaultInvariant.Compare
                    (val.GetValue(x, null), val.GetValue(y, null));

            throw new Exception(PropertyName + "is not a valid property to sort on. It doesn't exist in the Class.");
        }

        #endregion
    }
}