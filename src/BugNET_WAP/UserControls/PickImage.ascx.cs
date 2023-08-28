using BugNET.UI;

namespace BugNET.UserControls
{
    using System.Collections.Generic;
    using System.IO;
    using System.Web.UI.WebControls;

    /// <summary>
    ///		Summary description for PickImage.
    /// </summary>
    public partial class PickImage : BugNetUserControl
    {
        //*********************************************************************
        //
        // PickImage.ascx
        //
        // This user control displays a list of images in a radiobutton list.
        // The control is used by the Status.ascx, Priority.ascx, and Milestone.ascx
        // controls.
        //
        //*********************************************************************


        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>The CSS class.</value>
        public string CssClass
        {
            get => lstImages.CssClass;
            set => lstImages.CssClass = value;
        }


        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>The selected value.</value>
        public string SelectedValue
        {
            get => lstImages.SelectedValue;
            set => lstImages.SelectedValue = value;
        }


        /// <summary>
        /// 
        /// </summary>
        public string ImageDirectory = string.Empty;


        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            var objDir = new DirectoryInfo(MapPath("~/Images" + ImageDirectory));
            //FileInfo[] files = objDir.GetFiles("*.png");
            var filesList = new List<FileInfo>();

            //Add the files of the directory to the list
            filesList.AddRange(objDir.GetFiles());

            //Find the files on the list using a delegate
            filesList = filesList.FindAll(delegate(FileInfo f)
            {
                return f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".jpg" ||
                       f.Extension.ToLower() == ".gif";
            });

            var formatString = "<img valign=\"bottom\" src=\"" + ResolveUrl("~/Images") + ImageDirectory + "/{0}\" />";

            lstImages.DataSource = filesList;
            lstImages.DataTextField = "Name";
            lstImages.DataTextFormatString = formatString;
            lstImages.DataValueField = "Name";
            lstImages.DataBind();

            lstImages.Items.Insert(0, new ListItem(GetLocalString("None"), string.Empty));
            lstImages.SelectedIndex = 0;
        }
    }
}