using System;
using System.Collections.Generic;
using BugNET.Entities;

namespace BugNET.BLL.Comparers
{
    public class ApplicationLogComparer : IComparer<ApplicationLog>
    {
        /// <summary>
        /// Sorting column
        /// </summary>
        private readonly string sortColumn;

        /// <summary>
        /// Reverse sorting
        /// </summary>
        private readonly bool reverse;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationLogComparer"/> class.
        /// </summary>
        /// <param name="sortEx">The sort ex.</param>
        /// <param name="ascending">The ascending.</param>
        public ApplicationLogComparer(string sortEx, bool ascending)
        {
            if (string.IsNullOrEmpty(sortEx)) return;
            reverse = ascending;
            sortColumn = sortEx;
        }

        /// <summary>
        /// Equalizes the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public bool Equals(ApplicationLog x, ApplicationLog y)
        {
            return x.Id == y.Id;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value Condition Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        public int Compare(ApplicationLog x, ApplicationLog y)
        {
            var retVal = 0;
            switch (sortColumn)
            {
                case "Id":
                    retVal = x.Id - y.Id;
                    break;
                case "Logger":
                    retVal = string.Compare(x.Logger, y.Logger, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case "Message":
                    retVal = string.Compare(x.Message, y.Message, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case "User":
                    retVal = string.Compare(x.User, y.User, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case "Level":
                    retVal = string.Compare(x.Level, y.Level, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case "Date":
                    retVal = DateTime.Compare(x.Date, y.Date);
                    break;
            }

            return retVal * (reverse ? -1 : 1);
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public int GetHashCode(Project obj)
        {
            return 0;
        }
    }
}