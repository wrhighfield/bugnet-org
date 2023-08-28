using System;
using System.Linq;
using System.Web.UI.WebControls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.UI;

namespace BugNET.Administration.Users
{
    /// <summary>
    /// Summary description for UserList.
    /// </summary>
    public partial class UserList : BugNetBasePage
    {
        /// <summary>
        /// Gets or sets the sort field.
        /// </summary>
        /// <value>The sort field.</value>
        private string SortField
        {
            get => ViewState.Get("SortField", "UserName");
            set
            {
                if (value == SortField)
                    // same as current sort file, toggle sort direction
                    SortAscending = !SortAscending;
                ViewState.Set("SortField", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [sort ascending].
        /// </summary>
        /// <value><c>true</c> if [sort ascending]; otherwise, <c>false</c>.</value>
        private bool SortAscending
        {
            get => ViewState.Get("SortAscending", true);
            set => ViewState.Set("SortAscending", value);
        }

        /// <summary>
        /// Creates the letter search.
        /// </summary>
        private void CreateLetterSearch()
        {
            string[] alphabet =
            {
                "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U",
                "V", "W", "X", "Y", "Z", "All", "Unauthorized"
            };
            LetterSearch.DataSource = alphabet;
            LetterSearch.DataBind();
        }

        /// <summary>
        /// Gets or sets the search filter.
        /// </summary>
        /// <value>The search filter.</value>
        private string SearchFilter
        {
            get => ViewState.Get("SearchFilter", string.Empty);
            set => ViewState.Set("SearchFilter", value);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserManager.IsSuperUser())
                Response.Redirect("~/Errors/AccessDenied");

            if (IsPostBack) return;

            CreateLetterSearch();
            BindData(string.Empty);
        }

        /// <summary>
        /// Handles the RowCommand event of the gvUsers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.Web.UI.WebControls.GridViewCommandEventArgs"/> instance containing the event data.</param>
        protected void GvUsersRowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "Edit":
                    Response.Redirect($"~/Administration/Users/EditUser/{e.CommandArgument}");
                    break;
                case "ManageRoles":
                    Response.Redirect($"~/Administration/Users/EditUser/{e.CommandArgument}/1");
                    break;
                case "Delete":
                    Response.Redirect($"~/Administration/Users/EditUser/{e.CommandArgument}/4");
                    break;
            }
        }

        protected string GetLocalizedText(object dataItem)
        {
            var s = dataItem.ToString();
            switch (s)
            {
                case "All":
                case "Unauthorized":
                    return GetLocalString(s);
                default:
                    return s;
            }
        }

        /// <summary>
        /// Binds the data.
        /// </summary>
        /// <param name="filter">The filter.</param>
        private void BindData(string filter)
        {
            SearchFilter = filter;
            string searchText;

            switch (filter)
            {
                case "All":
                    searchText = string.Empty;
                    break;
                case "Unauthorized":
                    searchText = string.Empty;
                    break;
                default:
                    searchText = string.Concat(filter, "%");
                    break;
            }

            var users = string.IsNullOrEmpty(searchText)
                ? UserManager.GetAllUsers()
                : UserManager.FindUsersByName(searchText);

            if (filter == "Unauthorized")
                users = users.Where(user => !user.IsApproved || user.LastLoginDate == DateTime.MinValue).ToList();

            var sort = $"{SortField} {(SortAscending ? "asc" : "desc")}";

            gvUsers.DataSource = users.Sort(sort).ToList();
            gvUsers.DataBind();
        }

        /// <summary>
        /// Filters the URL.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="currentPage">The current page.</param>
        protected string FilterUrl(object filter, string currentPage)
        {
            var f = (string) filter;
            var url = Page.TemplateControl.AppRelativeVirtualPath;
            if (!string.IsNullOrEmpty(f)) url = $"{url}?Filter={f}";
            return ResolveUrl(url);
        }

        /// <summary>
        /// Handles the Click event of the FilterButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void FilterButtonClick(object sender, EventArgs e)
        {
            var lb = (LinkButton) sender;
            BindData(lb.CommandArgument);
        }

        /// <summary>
        /// Handles the RowCreated event of the gvUsers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void GvUsersRowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
                PresentationUtils.SetSortImageStates(gvUsers, e.Row, 1, SortField, SortAscending);
        }

        /// <summary>
        /// Handles the Sorting event of the gvUsers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewSortEventArgs"/> instance containing the event data.</param>
        protected void GvUsersSorting(object sender, GridViewSortEventArgs e)
        {
            SortField = e.SortExpression;
            BindData(SearchFilter);
        }

        /// <summary>
        /// Handles the Click event of the AddUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void AddUserClick(object sender, EventArgs e)
        {
            Response.Redirect("~/Administration/Users/AddUser.aspx");
        }

        /// <summary>
        /// Handles the Click event of the ibSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void IbSearchClick(object sender, EventArgs e)
        {
            var users = SearchField.SelectedValue == "Email"
                ? UserManager.FindUsersByEmail(txtSearch.Text + "%")
                : UserManager.FindUsersByName(txtSearch.Text + "%");

            gvUsers.DataSource = users;
            gvUsers.DataBind();
        }

        /// <summary>
        /// Handles the PageIndexChanging event of the gvUsers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.Web.UI.WebControls.GridViewPageEventArgs"/> instance containing the event data.</param>
        protected void GvUsersPageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsers.PageIndex = e.NewPageIndex;
            BindData(SearchFilter);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ddlPages_SelectedIndexChanged(object sender, EventArgs e)
        {
            var gvrPager = gvUsers.BottomPagerRow;
            if (gvrPager == null)
                return;
            var ddlPages = (DropDownList) gvrPager.Cells[0].FindControl("ddlPages");
            gvUsers.PageIndex = ddlPages.SelectedIndex;
            BindData(SearchFilter);
        }
    }
}