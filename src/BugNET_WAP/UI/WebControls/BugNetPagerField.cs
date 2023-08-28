using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BugNET.UI.WebControls
{
    public class BugNetPagerField : DataPagerField
    {
        private readonly string rowXofY = HttpContext.GetGlobalResourceObject("SharedResources", "RowXofY").ToString();

        private readonly string showRows =
            HttpContext.GetGlobalResourceObject("SharedResources", "ShowRows").ToString();

        private readonly string perPage = HttpContext.GetGlobalResourceObject("SharedResources", "PerPage").ToString();

        private int startRowIndex;
        private int maximumRows;
        private int totalRowCount;

        public BugNetPagerField()
        {
        }

        [DefaultValue(5)]
        [Category("Appearance")]
        public int ButtonCount
        {
            get
            {
                var o = ViewState["ButtonCount"];
                if (o != null) return (int) o;
                return 5;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(value));

                if (value == ButtonCount) return;
                ViewState["ButtonCount"] = value;
                OnFieldChanged();
            }
        }

        /// <devdoc>
        /// <para>Indicates the button type for the field.</para>
        /// </devdoc>
        [Category("Appearance")]
        [DefaultValue(ButtonType.Link)]
        public ButtonType ButtonType
        {
            get
            {
                var o = ViewState["ButtonType"];
                if (o != null)
                    return (ButtonType) o;
                return ButtonType.Link;
            }
            set
            {
                if (value < ButtonType.Button || value > ButtonType.Link)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (value == ButtonType) return;
                ViewState["ButtonType"] = value;
                OnFieldChanged();
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [CssClassProperty]
        public string CurrentPageLabelCssClass
        {
            get
            {
                var o = ViewState["CurrentPageLabelCssClass"];
                if (o != null) return (string) o;
                return string.Empty;
            }
            set
            {
                if (value == CurrentPageLabelCssClass) return;
                ViewState["CurrentPageLabelCssClass"] = value;
                OnFieldChanged();
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings",
            Justification = "Required by ASP.NET parser.")]
        [UrlProperty()]
        public string NextPageImageUrl
        {
            get
            {
                var o = ViewState["NextPageImageUrl"];
                if (o != null) return (string) o;
                return string.Empty;
            }
            set
            {
                if (value == NextPageImageUrl) return;
                ViewState["NextPageImageUrl"] = value;
                OnFieldChanged();
            }
        }

        [Category("Appearance")]
        [Localizable(true)]
        public string NextPageText
        {
            get
            {
                var o = ViewState["NextPageText"];
                if (o != null) return (string) o;
                return "Next";
            }
            set
            {
                if (value == NextPageText) return;
                ViewState["NextPageText"] = value;
                OnFieldChanged();
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [CssClassProperty]
        public string NextPreviousButtonCssClass
        {
            get
            {
                var o = ViewState["NextPreviousButtonCssClass"];
                if (o != null) return (string) o;
                return string.Empty;
            }
            set
            {
                if (value == NextPreviousButtonCssClass) return;
                ViewState["NextPreviousButtonCssClass"] = value;
                OnFieldChanged();
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [CssClassProperty]
        public string NumericButtonCssClass
        {
            get
            {
                var o = ViewState["NumericButtonCssClass"];
                if (o != null) return (string) o;
                return string.Empty;
            }
            set
            {
                if (value == NumericButtonCssClass) return;
                ViewState["NumericButtonCssClass"] = value;
                OnFieldChanged();
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings",
            Justification = "Required by ASP.NET parser.")]
        [UrlProperty()]
        public string PreviousPageImageUrl
        {
            get
            {
                var o = ViewState["PreviousPageImageUrl"];
                if (o != null) return (string) o;
                return string.Empty;
            }
            set
            {
                if (value == PreviousPageImageUrl) return;
                ViewState["PreviousPageImageUrl"] = value;
                OnFieldChanged();
            }
        }

        [Category("Appearance")]
        [Localizable(true)]
        public string PreviousPageText
        {
            get
            {
                var o = ViewState["PreviousPageText"];
                if (o != null) return (string) o;
                return "Previous";
            }
            set
            {
                if (value == PreviousPageText) return;
                ViewState["PreviousPageText"] = value;
                OnFieldChanged();
            }
        }

        [DefaultValue(true)]
        [Category("Behavior")]
        public bool RenderNonBreakingSpacesBetweenControls
        {
            get
            {
                var o = ViewState["RenderNonBreakingSpacesBetweenControls"];
                if (o != null) return (bool) o;
                return true;
            }
            set
            {
                if (value == RenderNonBreakingSpacesBetweenControls) return;
                ViewState["RenderNonBreakingSpacesBetweenControls"] = value;
                OnFieldChanged();
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2204:LiteralsShouldBeSpelledCorrectly", MessageId = "nbsp",
            Justification = "Literal is HTML escape sequence.")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters",
            MessageId = "System.Web.UI.LiteralControl.#ctor(System.String)",
            Justification = "Literal is HTML escape sequence.")]
        private void AddNonBreakingSpace(DataPagerFieldItem container)
        {
            if (RenderNonBreakingSpacesBetweenControls) container.Controls.Add(new LiteralControl(" "));
        }

        protected override void CopyProperties(DataPagerField newField)
        {
            ((NumericPagerField) newField).ButtonCount = ButtonCount;
            ((NumericPagerField) newField).ButtonType = ButtonType;
            ((NumericPagerField) newField).CurrentPageLabelCssClass = CurrentPageLabelCssClass;
            ((NumericPagerField) newField).NextPageImageUrl = NextPageImageUrl;
            ((NumericPagerField) newField).NextPageText = NextPageText;
            ((NumericPagerField) newField).NextPreviousButtonCssClass = NextPreviousButtonCssClass;
            ((NumericPagerField) newField).NumericButtonCssClass = NumericButtonCssClass;
            ((NumericPagerField) newField).PreviousPageImageUrl = PreviousPageImageUrl;
            ((NumericPagerField) newField).PreviousPageText = PreviousPageText;

            base.CopyProperties(newField);
        }

        protected override DataPagerField CreateField()
        {
            return new NumericPagerField();
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        public override void HandleEvent(CommandEventArgs e)
        {
            if (!string.IsNullOrEmpty(DataPager.QueryStringField)) return;
            var newStartRowIndex = -1;
            var currentPageIndex = startRowIndex / DataPager.PageSize;
            var firstButtonIndex = startRowIndex / (ButtonCount * DataPager.PageSize) * ButtonCount;
            var lastButtonIndex = firstButtonIndex + ButtonCount - 1;
            var lastRecordIndex = (lastButtonIndex + 1) * DataPager.PageSize - 1;

            if (string.Equals(e.CommandName, DataControlCommands.PreviousPageCommandArgument))
            {
                newStartRowIndex = (firstButtonIndex - 1) * DataPager.PageSize;
                if (newStartRowIndex < 0) newStartRowIndex = 0;
            }
            else if (string.Equals(e.CommandName, DataControlCommands.NextPageCommandArgument))
            {
                newStartRowIndex = lastRecordIndex + 1;
                if (newStartRowIndex > totalRowCount) newStartRowIndex = totalRowCount - DataPager.PageSize;
            }
            else if (string.Equals(e.CommandName, "UpdatePageSize"))
            {
                DataPager.PageSize = int.Parse(e.CommandArgument.ToString());
                DataPager.SetPageProperties(startRowIndex, DataPager.PageSize, true);
            }
            else
            {
                var pageIndex = Convert.ToInt32(e.CommandName, CultureInfo.InvariantCulture);
                newStartRowIndex = pageIndex * DataPager.PageSize;
            }

            if (newStartRowIndex != -1) DataPager.SetPageProperties(newStartRowIndex, DataPager.PageSize, true);
        }

        private Control CreateNumericButton(string buttonText, string commandArgument, string commandName)
        {
            IButtonControl button;

            switch (ButtonType)
            {
                case ButtonType.Button:
                    button = new Button();
                    break;
                case ButtonType.Link:
                case ButtonType.Image:
                default:
                    button = new LinkButton();
                    break;
            }

            button.Text = buttonText;
            button.CommandName = commandName;
            button.CommandArgument = commandArgument;
            button.CausesValidation = false;

            if (button is WebControl webControl && !string.IsNullOrEmpty(NumericButtonCssClass))
                webControl.CssClass = NumericButtonCssClass;

            return (Control) button;
        }

        private HyperLink CreateNumericLink(int pageIndex)
        {
            var pageNumber = pageIndex + 1;
            var link = new HyperLink();
            link.Text = pageNumber.ToString(CultureInfo.InvariantCulture);
            link.NavigateUrl = GetQueryStringNavigateUrl(pageNumber);

            if (!string.IsNullOrEmpty(NumericButtonCssClass)) link.CssClass = NumericButtonCssClass;

            return link;
        }

        private Control CreateNextPrevButton(string buttonText, string commandName, string commandArgument,
            string imageUrl)
        {
            IButtonControl button;

            switch (ButtonType)
            {
                case ButtonType.Link:
                    button = new LinkButton();
                    break;
                case ButtonType.Button:
                    button = new Button();
                    break;
                case ButtonType.Image:
                default:
                    button = new ImageButton();
                    ((ImageButton) button).ImageUrl = imageUrl;
                    ((ImageButton) button).AlternateText = HttpUtility.HtmlDecode(buttonText);
                    break;
            }

            button.Text = buttonText;
            button.CommandName = commandName;
            button.CommandArgument = commandArgument;

            if (button is WebControl webControl && !string.IsNullOrEmpty(NextPreviousButtonCssClass))
                webControl.CssClass = NextPreviousButtonCssClass;

            return (Control) button;
        }

        private HyperLink CreateNextPrevLink(string buttonText, int pageIndex, string imageUrl)
        {
            var pageNumber = pageIndex + 1;
            var link = new HyperLink();
            link.Text = buttonText;
            link.NavigateUrl = GetQueryStringNavigateUrl(pageNumber);
            link.ImageUrl = imageUrl;
            if (!string.IsNullOrEmpty(NextPreviousButtonCssClass)) link.CssClass = NextPreviousButtonCssClass;
            return link;
        }

        /// <summary>
        /// Creates the label record control.
        /// </summary>
        /// <param name="container">The container.</param>
        private void CreateLabelRecordControl(DataPagerFieldItem container)
        {
            var endRowIndex = startRowIndex + DataPager.PageSize;

            if (endRowIndex > totalRowCount)
                endRowIndex = totalRowCount;

            container.Controls.Add(new LiteralControl("<span>"));
            container.Controls.Add(
                new LiteralControl(string.Format(rowXofY, startRowIndex + 1, endRowIndex, totalRowCount)));
            container.Controls.Add(new LiteralControl("</span>"));
        }

        /// <summary>
        /// Creates the page size control.
        /// </summary>
        /// <param name="container">The container.</param>
        private void CreatePageSizeControl(DataPagerFieldItem container)
        {
            var pageSizeDropDownList = new ButtonDropDownList
                {CommandName = "UpdatePageSize", CssClass = "form-control"};

            pageSizeDropDownList.Items.Add(new ListItem("5", "5"));
            pageSizeDropDownList.Items.Add(new ListItem("10", "10"));
            pageSizeDropDownList.Items.Add(new ListItem("15", "15"));
            pageSizeDropDownList.Items.Add(new ListItem("25", "25"));
            pageSizeDropDownList.Items.Add(new ListItem("50", "50"));
            pageSizeDropDownList.Items.Add(new ListItem("75", "75"));
            pageSizeDropDownList.Items.Add(new ListItem("100", "100"));

            var pageSizeItem = pageSizeDropDownList.Items.FindByValue(DataPager.PageSize.ToString());

            if (pageSizeItem == null)
            {
                pageSizeItem = new ListItem(DataPager.PageSize.ToString(), DataPager.PageSize.ToString());
                pageSizeDropDownList.Items.Insert(0, pageSizeItem);
            }

            pageSizeItem.Selected = true;
            container.Controls.Add(pageSizeDropDownList);

            container.Controls.Add(new LiteralControl($"<span>{perPage}</span>"));
        }

        public override void CreateDataPagers(DataPagerFieldItem container, int startRowIndex, int maximumRows,
            int totalRowCount, int fieldIndex)
        {
            this.startRowIndex = startRowIndex;
            this.maximumRows = maximumRows;
            this.totalRowCount = totalRowCount;

            if (string.IsNullOrEmpty(DataPager.QueryStringField))
                CreateDataPagersForCommand(container, fieldIndex);
            else
                CreateDataPagersForQueryString(container, fieldIndex);
        }

        private void CreateDataPagersForCommand(DataPagerFieldItem container, int fieldIndex)
        {
            var currentPageIndex = startRowIndex / maximumRows;
            var firstButtonIndex = startRowIndex / (ButtonCount * maximumRows) * ButtonCount;
            var lastButtonIndex = firstButtonIndex + ButtonCount - 1;
            var lastRecordIndex = (lastButtonIndex + 1) * maximumRows - 1;


            container.Controls.Add(new LiteralControl("<div class=\"grid-footer-left pull-left\">"));

            //Set of records - total records
            CreateLabelRecordControl(container);

            //Control used to set the page size.
            CreatePageSizeControl(container);

            container.Controls.Add(new LiteralControl("</div>"));

            container.Controls.Add(new LiteralControl("<ul class=\"pagination pull-right\">"));

            if (firstButtonIndex != 0)
            {
                container.Controls.Add(new LiteralControl("<li>"));
                container.Controls.Add(CreateNextPrevButton(PreviousPageText,
                    DataControlCommands.PreviousPageCommandArgument, fieldIndex.ToString(CultureInfo.InvariantCulture),
                    PreviousPageImageUrl));
                container.Controls.Add(new LiteralControl("</li>"));
            }

            for (var i = 0; i < ButtonCount && totalRowCount > (i + firstButtonIndex) * maximumRows; i++)
                if (i + firstButtonIndex == currentPageIndex)
                {
                    //Label pageNumber = new Label();
                    //pageNumber.Text = (i + firstButtonIndex + 1).ToString(CultureInfo.InvariantCulture);
                    //if (!String.IsNullOrEmpty(CurrentPageLabelCssClass))
                    //{
                    //    pageNumber.CssClass = CurrentPageLabelCssClass;
                    //}
                    container.Controls.Add(new LiteralControl("<li class=\"active\">"));
                    container.Controls.Add(new LiteralControl(
                        $"<span>{i + firstButtonIndex + 1}<span class=\"sr-only\">(current)</span></span>"));
                    container.Controls.Add(new LiteralControl("</li>"));
                }
                else
                {
                    container.Controls.Add(new LiteralControl("<li>"));
                    container.Controls.Add(CreateNumericButton(
                        (i + firstButtonIndex + 1).ToString(CultureInfo.InvariantCulture),
                        fieldIndex.ToString(CultureInfo.InvariantCulture),
                        (i + firstButtonIndex).ToString(CultureInfo.InvariantCulture)));
                    container.Controls.Add(new LiteralControl("</li>"));
                }

            if (lastRecordIndex < totalRowCount - 1)
            {
                container.Controls.Add(new LiteralControl("<li>"));
                container.Controls.Add(CreateNextPrevButton(NextPageText, DataControlCommands.NextPageCommandArgument,
                    fieldIndex.ToString(CultureInfo.InvariantCulture), NextPageImageUrl));
                container.Controls.Add(new LiteralControl("</li>"));
            }

            container.Controls.Add(new LiteralControl("</ul>"));
        }

        private void CreateDataPagersForQueryString(DataPagerFieldItem container, int fieldIndex)
        {
            var currentPageIndex = startRowIndex / maximumRows;
            var resetProperties = false;
            if (!QueryStringHandled)
            {
                QueryStringHandled = true;
                var parsed = int.TryParse(QueryStringValue, out var currentQsPageIndex);
                if (parsed)
                {
                    currentQsPageIndex--; //convert page number to page index.
                    var highestPageIndex = (totalRowCount - 1) / maximumRows;
                    if (currentQsPageIndex >= 0 && currentQsPageIndex <= highestPageIndex)
                    {
                        currentPageIndex = currentQsPageIndex;
                        startRowIndex = currentPageIndex * maximumRows;
                        resetProperties = true;
                    }
                }
            }

            var firstButtonIndex = startRowIndex / (ButtonCount * maximumRows) * ButtonCount;
            var lastButtonIndex = firstButtonIndex + ButtonCount - 1;
            var lastRecordIndex = (lastButtonIndex + 1) * maximumRows - 1;

            if (firstButtonIndex != 0)
            {
                container.Controls.Add(CreateNextPrevLink(PreviousPageText, firstButtonIndex - 1,
                    PreviousPageImageUrl));
                AddNonBreakingSpace(container);
            }

            for (var i = 0; i < ButtonCount && totalRowCount > (i + firstButtonIndex) * maximumRows; i++)
            {
                if (i + firstButtonIndex == currentPageIndex)
                {
                    var pageNumber = new Label();
                    pageNumber.Text = (i + firstButtonIndex + 1).ToString(CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(CurrentPageLabelCssClass)) pageNumber.CssClass = CurrentPageLabelCssClass;
                    container.Controls.Add(pageNumber);
                }
                else
                {
                    container.Controls.Add(CreateNumericLink(i + firstButtonIndex));
                }

                AddNonBreakingSpace(container);
            }

            if (lastRecordIndex < totalRowCount - 1)
            {
                AddNonBreakingSpace(container);
                container.Controls.Add(CreateNextPrevLink(NextPageText, firstButtonIndex + ButtonCount,
                    NextPageImageUrl));
                AddNonBreakingSpace(container);
            }

            if (resetProperties) DataPager.SetPageProperties(startRowIndex, maximumRows, true);
        }

        // Required for design-time support (DesignerPagerStyle)
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override bool Equals(object o)
        {
            if (!(o is NumericPagerField field)) return false;
            return Equals(field.ButtonCount, ButtonCount) &&
                   field.ButtonType == ButtonType &&
                   string.Equals(field.CurrentPageLabelCssClass, CurrentPageLabelCssClass) &&
                   string.Equals(field.NextPageImageUrl, NextPageImageUrl) &&
                   string.Equals(field.NextPageText, NextPageText) &&
                   string.Equals(field.NextPreviousButtonCssClass, NextPreviousButtonCssClass) &&
                   string.Equals(field.NumericButtonCssClass, NumericButtonCssClass) &&
                   string.Equals(field.PreviousPageImageUrl, PreviousPageImageUrl) &&
                   string.Equals(field.PreviousPageText, PreviousPageText);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override int GetHashCode()
        {
            return
                ButtonCount.GetHashCode() |
                ButtonType.GetHashCode() |
                CurrentPageLabelCssClass.GetHashCode() |
                NextPageImageUrl.GetHashCode() |
                NextPageText.GetHashCode() |
                NextPreviousButtonCssClass.GetHashCode() |
                NumericButtonCssClass.GetHashCode() |
                PreviousPageImageUrl.GetHashCode() |
                PreviousPageText.GetHashCode();
        }
    }
}