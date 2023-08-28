using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.UserControls;

namespace BugNET.UI
{
    /// <summary>
    /// Exports a grid-view to excel
    /// </summary>
    public static class GridViewExportUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="gv"></param>
        public static void Export(string fileName, GridView gv)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader("content-disposition", $"attachment; filename={fileName}");
            HttpContext.Current.Response.ContentType = "application/ms-excel";
            HttpContext.Current.Response.Charset = "utf-8";

            using (var sw = new StringWriter())
            {
                using (var htw = new HtmlTextWriter(sw))
                {
                    //  Create a table to contain the grid
                    var table = new Table {GridLines = gv.GridLines};

                    //  include the grid-line settings

                    //  add the header row to the table
                    if (gv.HeaderRow != null)
                    {
                        PrepareControlForExport(gv.HeaderRow);
                        table.Rows.Add(gv.HeaderRow);
                    }

                    //  add each of the data rows to the table
                    foreach (GridViewRow row in gv.Rows)
                    {
                        PrepareControlForExport(row);
                        table.Rows.Add(row);
                    }

                    //  add the footer row to the table
                    if (gv.FooterRow != null)
                    {
                        PrepareControlForExport(gv.FooterRow);
                        table.Rows.Add(gv.FooterRow);
                    }

                    for (var j = 0; j < gv.Columns.Count; j++)
                    {
                        if (gv.Columns[j].Visible) continue;
                        for (var i = 0; i < table.Rows.Count; i++)
                        {
                            //First 2 columns should be hidden by default.                              
                            table.Rows[i].Cells[0].Visible = false;
                            table.Rows[i].Cells[1].Visible = false;

                            table.Rows[i].Cells[j].Visible = false;
                        }
                    }

                    //  render the table into the html-writer
                    table.RenderControl(htw);

                    //  render the html-writer into the response
                    HttpContext.Current.Response.Write(sw.ToString());
                    HttpContext.Current.Response.End();
                }
            }
        }

        /// <summary>
        /// Replace any of the contained controls with literals
        /// </summary>
        /// <param name="control"></param>
        private static void PrepareControlForExport(Control control)
        {
            for (var i = 0; i < control.Controls.Count; i++)
            {
                var current = control.Controls[i];

                switch (current)
                {
                    case LinkButton button:
                        control.Controls.Remove(button);
                        control.Controls.AddAt(i, new LiteralControl(button.Text));
                        break;
                    case ImageButton button:
                        control.Controls.Remove(button);
                        control.Controls.AddAt(i, new LiteralControl(button.AlternateText));
                        break;
                    case HyperLink link:
                        link.NavigateUrl = HostSettingManager.Get(HostSettingNames.DefaultUrl) +
                                           link.NavigateUrl.Substring(2);
                        break;
                    case DropDownList list:
                        control.Controls.Remove(list);
                        control.Controls.AddAt(i, new LiteralControl(list.SelectedItem.Text));
                        break;
                    case CheckBox box:
                        control.Controls.Remove(box);
                        control.Controls.AddAt(i, new LiteralControl(box.Checked ? "True" : "False"));
                        break;
                    case TextImage image:
                        control.Controls.Remove(image);
                        control.Controls.AddAt(i, new LiteralControl(image.Text));
                        break;
                    case Image image:
                        control.Controls.Remove(image);
                        control.Controls.AddAt(i, new LiteralControl(image.AlternateText));
                        break;
                    case LiteralControl literalControl:
                    {
                        var text = Regex.Replace(literalControl.Text, "<.*?>", string.Empty);
                        control.Controls.Remove(literalControl);
                        control.Controls.AddAt(i, new LiteralControl(text));
                        break;
                    }
                    default:
                    {
                        switch (current.ID)
                        {
                            case "Progress":
                                control.Controls.Remove(current);
                                control.Controls.AddAt(i,
                                    new LiteralControl(Regex.Replace(
                                        (current.Controls[1] as System.Web.UI.HtmlControls.HtmlGenericControl)
                                        .InnerText, "<.*?>", string.Empty)));
                                break;
                            case "PrivateIssue":
                            {
                                control.Controls.Remove(current);
                                if (current.Visible)
                                    control.Controls.AddAt(i,
                                        new LiteralControl(Regex.Replace(
                                            (current as System.Web.UI.HtmlControls.HtmlGenericControl).InnerText,
                                            "<.*?>", string.Empty)));

                                break;
                            }
                        }

                        break;
                    }
                }

                if (current.HasControls()) PrepareControlForExport(current);
            }
        }
    }
}