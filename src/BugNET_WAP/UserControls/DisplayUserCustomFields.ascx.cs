﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using BugNET.BLL;
using BugNET.Common;
using BugNET.Entities;
using BugNET.UI;

namespace BugNET.UserControls
{
    public partial class DisplayUserCustomFields : BugNetUserControl
    {
        private const string FIELD_VALUE_NAME = "FieldValue";

        /// <summary>
        /// 
        /// </summary>
        public bool Required = true;

        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        /// <value>The data source.</value>
        public object DataSource
        {
            get => rptCustomFields.DataSource;
            set => rptCustomFields.DataSource = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is locked.
        /// </summary>
        /// <value><c>true</c> if this instance is locked; otherwise, <c>false</c>.</value>
        public bool IsLocked
        {
            get => ViewState.Get("IsLocked", false);
            set => ViewState.Set("IsLocked", value);
        }

        private static string GetAttributeFromField(string key, AttributeCollection list)
        {
            if (list.Count.Equals(0)) return string.Empty;

            return list.Keys.Cast<object>().Any(k => k.ToString().ToLower().Equals(key.ToLower()))
                ? list[key]
                : string.Empty;
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public List<UserCustomField> Values
        {
            get
            {
                var colFields = new List<UserCustomField>();

                for (var i = 0; i < rptCustomFields.Items.Count; i++)
                {
                    var item = rptCustomFields.Items[i];

                    if (item.ItemType != ListItemType.Item && item.ItemType != ListItemType.AlternatingItem) continue;

                    var id = (HiddenField) item.FindControl("Id");
                    var fieldName = (HiddenField) item.FindControl("Name");
                    var fieldId = int.Parse(id.Value);

                    var c = item.FindControl("FieldValue");

                    if (c == null) continue;

                    var fieldValue = string.Empty;

                    if (c.GetType() == typeof(DropDownList) && ((DropDownList) c).SelectedIndex != 0)
                        fieldValue = ((DropDownList) c).SelectedValue;

                    if (c.GetType() == typeof(TextBox))
                    {
                        var textBox = (TextBox) c;
                        fieldValue = textBox.Text;

                        var dataType = GetAttributeFromField("bn-data-type", textBox.Attributes).ToLower();

                        if (dataType.Equals("date"))
                        {
                            DateTime dt;
                            if (DateTime.TryParse(fieldValue, out dt)) fieldValue = dt.ToString("yyyy-MM-dd");
                        }
                    }

                    if (c.GetType() == typeof(CheckBox))
                        fieldValue = ((CheckBox) c).Checked.ToString();

                    if (c.GetType() == typeof(HtmlEditor))
                        fieldValue = ((HtmlEditor) c).Text;

                    colFields.Add(new UserCustomField {Id = fieldId, Value = fieldValue, Name = fieldName.Value});
                }

                return colFields;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable validation].
        /// </summary>
        /// <value><c>true</c> if [enable validation]; otherwise, <c>false</c>.</value>
        public bool EnableValidation
        {
            get => ViewState.Get("EnableValidation", false);
            set => ViewState.Set("EnableValidation", value);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Binds a data source to the invoked server control and all its child controls.
        /// </summary>
        public override void DataBind()
        {
            rptCustomFields.DataBind();
        }


        protected void rptCustomFields_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var currentField = (UserCustomField) e.Item.DataItem;

            var ph = (PlaceHolder) e.Item.FindControl("PlaceHolder");
            var id = (HiddenField) e.Item.FindControl("Id");
            var name = (HiddenField) e.Item.FindControl("Name");

            id.Value = currentField.Id.ToString();
            name.Value = currentField.Name;

            switch (currentField.FieldType)
            {
                case CustomFieldType.DropDownList:

                    var ddl = new DropDownList
                    {
                        ID = FIELD_VALUE_NAME,
                        DataSource = UserCustomFieldSelectionManager.GetByCustomFieldId(currentField.Id),
                        DataTextField = "Name",
                        DataValueField = "Value",
                        CssClass = "form-control"
                    };

                    ddl.DataBind();
                    ddl.Items.Insert(0, new ListItem("-- Select One --", string.Empty));
                    ddl.SelectedValue = currentField.Value;

                    ph.Controls.Add(ddl);

                    if (IsLocked) ddl.Enabled = false;

                    break;
                case CustomFieldType.Date:

                    var fieldValue1 = new TextBox();
                    fieldValue1.Attributes.Add("bn-data-type", "date");
                    var cal = new CalendarExtender();

                    var img = new Image {ID = "calImage", CssClass = "icon", ImageUrl = "~/images/calendar.gif"};

                    cal.PopupButtonID = "calImage";
                    cal.TargetControlID = FIELD_VALUE_NAME;
                    cal.ID = "Calendar1";

                    fieldValue1.ID = "FieldValue";
                    fieldValue1.Width = 80;

                    ph.Controls.Add(fieldValue1);
                    ph.Controls.Add(img);
                    ph.Controls.Add(new LiteralControl("&nbsp"));

                    DateTime dt;
                    var dateTimeValue = currentField.Value;

                    if (DateTime.TryParse(dateTimeValue, out dt)) dateTimeValue = dt.ToShortDateString();

                    fieldValue1.Text = dateTimeValue;

                    ph.Controls.Add(cal);

                    if (IsLocked)
                    {
                        cal.Enabled = false;
                        fieldValue1.Enabled = false;
                        img.Visible = false;
                    }

                    break;
                case CustomFieldType.Text:

                    var fieldValue = new TextBox
                    {
                        ID = FIELD_VALUE_NAME,
                        Text = currentField.Value,
                        CssClass = "form-control"
                    };
                    fieldValue.Attributes.Add("bn-data-type", "text");

                    ph.Controls.Add(fieldValue);

                    if (currentField.Value.Trim().ToLower().StartsWith("http"))
                    {
                        var url = new HyperLink
                            {Target = "_blank", NavigateUrl = currentField.Value, Text = "&nbsp;GOTO >>"};
                        ph.Controls.Add(url);
                    }

                    if (IsLocked) fieldValue.Enabled = false;
                    break;
                case CustomFieldType.YesNo:

                    var chk = new CheckBox {ID = FIELD_VALUE_NAME};

                    if (!string.IsNullOrEmpty(currentField.Value)) chk.Checked = bool.Parse(currentField.Value);

                    ph.Controls.Add(new LiteralControl("<div class=\"checkbox\">"));
                    ph.Controls.Add(chk);
                    ph.Controls.Add(new LiteralControl("</div>"));

                    if (IsLocked) chk.Enabled = false;

                    break;
                case CustomFieldType.RichText:

                    var editor = new HtmlEditor {ID = FIELD_VALUE_NAME};
                    editor.Attributes.Add("bn-data-type", "html");

                    ph.Controls.Add(editor);

                    editor.Text = currentField.Value;

                    break;
                case CustomFieldType.UserList:

                    ddl = new DropDownList
                    {
                        ID = FIELD_VALUE_NAME,
                        DataSource = UserManager.GetAllUsers(),
                        DataTextField = "DisplayName",
                        DataValueField = "UserName",
                        CssClass = "form-control"
                    };

                    ddl.DataBind();

                    ddl.Items.Insert(0,
                        new ListItem(GetGlobalString("SharedResources", "DropDown_SelectOne"), string.Empty));
                    ddl.SelectedValue = currentField.Value;

                    ph.Controls.Add(ddl);

                    if (IsLocked) ddl.Enabled = false;

                    break;
            }

            var lblFieldName = (Label) e.Item.FindControl("lblFieldName");
            lblFieldName.AssociatedControlID = FIELD_VALUE_NAME;
            lblFieldName.Text = currentField.Name;

            if (EnableValidation)
            {
                //if required dynamically add a required field validator
                if (currentField.Required && currentField.FieldType != CustomFieldType.YesNo)
                {
                    var valReq = new RequiredFieldValidator
                    {
                        ControlToValidate = FIELD_VALUE_NAME,
                        Text = $" ({GetGlobalString("SharedResources", "Required")})".ToLower(),
                        Display = ValidatorDisplay.Dynamic,
                        CssClass = "text-danger validation-error",
                        SetFocusOnError = true
                    };

                    if (currentField.FieldType == CustomFieldType.DropDownList)
                        valReq.InitialValue = string.Empty;

                    ph.Controls.Add(valReq);
                }

                //create data type check validator
                if (currentField.FieldType != CustomFieldType.YesNo)
                {
                    var valCompare = new CompareValidator
                    {
                        Type = currentField.DataType,
                        Text = $"({currentField.DataType})",
                        Operator = ValidationCompareOperator.DataTypeCheck,
                        Display = ValidatorDisplay.Dynamic,
                        ControlToValidate = FIELD_VALUE_NAME
                    };
                    ph.Controls.Add(valCompare);
                }
            }
        }
    }
}