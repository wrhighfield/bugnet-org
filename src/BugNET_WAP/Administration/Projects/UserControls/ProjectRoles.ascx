<%@ Control Language="c#" Inherits="BugNET.Administration.Projects.UserControls.ProjectRoles" CodeBehind="ProjectRoles.ascx.cs" %>
<asp:HiddenField runat="server" ID="txtProjectID" Value="0"/>
<asp:Label ID="lblError" ForeColor="red" EnableViewState="false" runat="Server"/>
<h2>
    <asp:Literal ID="RolesTitle" runat="Server" meta:resourcekey="RolesTitle"/>
</h2>
<asp:Panel ID="Roles" Visible="True" CssClass="myform" runat="server">
    <p class="desc">
        <asp:Label ID="DescriptionLabel" runat="server" meta:resourcekey="DescriptionLabel"/>
    </p>
    <br/>
    <asp:GridView HorizontalAlign="Left" OnRowCommand="gvRoles_RowCommand"
                  UseAccessibleHeader="true"
                  GridLines="None"
                  CssClass="table table-striped"
                  ID="gvRoles" runat="server" AutoGenerateColumns="False"
                  DataSourceID="SecurityRoles">
        <Columns>
            <asp:TemplateField>
                <ItemStyle Width="20px"/>
                <ItemTemplate>
                    <asp:ImageButton ID="cmdEditRole" runat="server" CommandName="EditRole" CommandArgument='<%# Eval("Id") %>' ImageUrl="~\images\pencil.gif"
                                     ImageAlign="Top"/>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField HeaderStyle-HorizontalAlign="Left" ItemStyle-Width="200px" DataField="Name" HeaderText="<%$ Resources:SharedResources, Name %>"/>
            <asp:BoundField HeaderStyle-HorizontalAlign="Left" ItemStyle-Width="200px" DataField="Description" HeaderText="<%$ Resources:SharedResources, Description %>"/>
            <asp:CheckBoxField HeaderStyle-HorizontalAlign="Left" DataField="AutoAssign" HeaderText="Auto Assignment" meta:resourcekey="AutoAssignmentColumnHeader"/>
        </Columns>
    </asp:GridView>
    <div style="margin-top: 1em">
        <asp:LinkButton ID="cmdAddRole" OnClick="AddRole_Click" CssClass="btn btn-primary" runat="server" meta:resourcekey="AddNewRole" Text="Add New Role"/>
    </div>
    <asp:ObjectDataSource ID="SecurityRoles" runat="server" SelectMethod="GetByProjectId" TypeName="BugNET.BLL.RoleManager">
        <SelectParameters>
            <asp:ControlParameter ControlID="txtProjectID" Name="projectId" PropertyName="Value" Type="Int32"/>
        </SelectParameters>
    </asp:ObjectDataSource>
</asp:Panel>
<asp:Panel ID="AddRole" CssClass="form-horizontal" Visible="False" runat="server">
    <p>
        <asp:Label ID="Label6" runat="server" meta:resourcekey="NewRoleDescriptionLabel"/>
    </p>
    <asp:Label ID="Label1" ForeColor="Red" runat="server"></asp:Label>
    <div class="fieldgroup" style="border: none;">
        <div class="form-group">
            <asp:Label ID="Label2" CssClass="control-label col-md-2" AssociatedControlID="txtRoleName" meta:resourcekey="RoleName" runat="server" Text="Role Name:"></asp:Label>
            <div class="col-md-10">
                <asp:TextBox ID="txtRoleName" runat="server" CssClass="form-control"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvRoleName" runat="server" ControlToValidate="txtRoleName" ErrorMessage="(required)" SetFocusOnError="True"></asp:RequiredFieldValidator>
            </div>
        </div>
        <div class="form-group">
            <asp:Label ID="Label4" CssClass="control-label col-md-2" AssociatedControlID="txtDescription" Text="<%$ Resources:SharedResources, Description %>" runat="server"></asp:Label>
            <div class="col-md-10">
                <asp:TextBox ID="txtDescription" TextMode="multiLine" Rows="4" CssClass="form-control" runat="server"></asp:TextBox>
            </div>
        </div>
        <div class="form-group">
            <asp:Label ID="Label5" CssClass="control-label col-md-2" AssociatedControlID="chkAutoAssign" Text="Auto Assignment" meta:resourcekey="AutoAssignment" runat="server"></asp:Label>
            <div class="col-md-10">
                <div class="checkbox">
                    <asp:CheckBox ID="chkAutoAssign" runat="server"/>
                </div>
            </div>
        </div>
    </div>
    <br/>
    <br/>
    <h3>
        <asp:Label ID="Label3" meta:resourcekey="PermissionsTitle" runat="server"/>
    </h3>
    <div>
        <fieldset>
            <legend>
                <asp:Literal ID="Literal2" runat="Server" Text="Issue Tracking" meta:resourcekey="IssueTracking"></asp:Literal>
            </legend>
            <ul class="permissions">
                <li>
                    <asp:CheckBox ID="chkAddIssue" Text="Add issues" meta:resourcekey="AddIssues" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkEditIssue" Text="Edit issues" meta:resourcekey="EditIssues" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkDeleteIssue" Text="Delete issues" meta:resourcekey="DeleteIssues" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkEditIssueDescription" Text="Edit issue descriptions" meta:resourcekey="EditIssueDescriptions" runat="server">
                    </asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkEditIssueSummary" Text="Edit issue titles" meta:resourcekey="EditIssueTitles" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkChangeIssueStatus" Text="Change issue status" meta:resourcekey="ChangeIssueStatus" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkAddComment" Text="Add comments" meta:resourcekey="AddComments" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkDeleteComment" Text="Delete comments" meta:resourcekey="DeleteComments" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkEditComment" Text="Edit comments" meta:resourcekey="EditComments" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkEditOwnComment" Text="Edit own comments" meta:resourcekey="EditOwnComments" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkAddAttachment" Text="Add attachments" meta:resourcekey="AddAttachments" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkDeleteAttachment" Text="Delete attachments" meta:resourcekey="DeleteAttachments" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkAddSubIssue" Text="Add sub issues" meta:resourcekey="AddSubIssues" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkAddRelated" Text="Add related issues" meta:resourcekey="AddRelatedIssues" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkAddParentIssue" Text="Add parent issues" meta:resourcekey="AddParentIssues" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkDeleteSubIssue" Text="Delete sub issues" meta:resourcekey="DeleteSubIssues" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkDeleteRelated" Text="Delete related issues" meta:resourcekey="DeleteRelatedIssues" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkCloseIssue" Text="Close issues" meta:resourcekey="CloseIssues" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkAssignIssue" Text="Assign issues" meta:resourcekey="AssignIssues" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkSubscribeIssue" Text="Subscribe issues" meta:resourcekey="SubscribeIssues" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkAddTimeEntry" Text="Add time entries" meta:resourcekey="AddTimeEntries" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkDeleteTimeEntry" Text="Delete time entries" meta:resourcekey="DeleteTimeEntries" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkDeleteParentIssue" Text="Delete parent issues" meta:resourcekey="DeleteParentIssues" runat="server">
                    </asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkReOpenIssue" Text="Re-Open Issue" meta:resourcekey="ReOpenIssues" runat="server"></asp:CheckBox>
                </li>
            </ul>
        </fieldset>
        <fieldset>
            <legend>
                <asp:Literal ID="Literal1" runat="Server" Text="Queries" meta:resourcekey="Queries"></asp:Literal>
            </legend>
            <ul class="permissions">
                <li>
                    <asp:CheckBox ID="chkAddQuery" Text="Add queries" meta:resourcekey="AddQueries" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkEditQuery" Text="Edit queries" meta:resourcekey="EditQueries" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkDeleteQuery" Text="Delete queries" meta:resourcekey="DeleteQueries" runat="server"></asp:CheckBox>
                </li>
            </ul>
        </fieldset>
        <fieldset>
            <legend>
                <asp:Literal ID="lit1" runat="Server" Text="<%$ Resources:SharedResources,Project %>"></asp:Literal>
            </legend>
            <ul class="permissions">
                <li>
                    <asp:CheckBox ID="chkEditProject" Text="Edit project" meta:resourcekey="EditProject" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkDeleteProject" Text="Delete project" meta:resourcekey="DeleteProject" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkCloneProject" Text="Clone project" meta:resourcekey="CloneProject" runat="server"></asp:CheckBox>
                </li>
                <li>
                    <asp:CheckBox ID="chkCreateProject" Text="Create project" meta:resourcekey="CreateProject" runat="server"></asp:CheckBox>
                </li>
            </ul>
        </fieldset>
    </div>
    <br/>
    <br/>
    <div class="row">
        <asp:LinkButton ID="cmdAddUpdateRole" CssClass="btn btn-primary" OnClick="cmdAddUpdateRole_Click" runat="server" CausesValidation="True" meta:resourcekey="AddRoleButton"
                        Text="Add Role"/>
        <asp:LinkButton ID="cmdCancel" CssClass="btn btn-default" OnClick="cmdCancel_Click" runat="server" CausesValidation="False" Text="<%$ Resources:SharedResources, Cancel %>"/>
        <asp:ImageButton runat="server" OnClick="cmdDelete_Click" ID="cancel" CssClass="icon" ImageUrl="~/Images/shield_delete.gif"/>
        <asp:LinkButton ID="cmdDelete" CssClass="btn btn-danger" OnClick="cmdDelete_Click" runat="server" CausesValidation="False" meta:resourcekey="DeleteRoleButton"
                        Text="Delete Role"/>
    </div>
</asp:Panel>