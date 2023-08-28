using System.Web.UI.WebControls;

namespace BugNET.UI
{
    /// <summary>
    /// Menu Tab Class
    /// </summary>
    public sealed class Tab : HyperLink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Tab"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="url">The URL.</param>
        public Tab(string name, string url)
        {
            Text = $@"<span>{name}</span>";
            NavigateUrl = url;
        }
    }
}