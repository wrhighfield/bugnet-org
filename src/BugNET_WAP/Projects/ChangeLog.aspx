<%@ Page Language="c#" Inherits="BugNET.Projects.ChangeLog" MasterPageFile="~/Site.master"
meta:resourcekey="Page" Title="Change Log" CodeBehind="ChangeLog.aspx.cs" %>

<%@ Register TagPrefix="it" TagName="TextImage" Src="~/UserControls/TextImage.ascx" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="content1" runat="server">
    <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
        <Scripts>
            <asp:ScriptReference Path="~/Scripts/jquery.cookie.js"/>
        </Scripts>
    </asp:ScriptManagerProxy>
    <script type="text/javascript">
       $(document).ready(function () {
           $('.milestone-content').on('hidden.bs.collapse', function () {
               $.cookie($(this).attr('id'), null);
           })
           $('.milestone-content').on('shown.bs.collapse', function () {
               $.cookie($(this).attr('id'), 'expanded');
           })
           $(".milestone-content").each(function () {
               var isExpanded = $.cookie($(this).attr('id'));
               if (isExpanded == 'expanded') {
                   $(this).collapse('show');
               }
           });
       });
    </script>
    <div class="page-header">
        <h1 class="page-title">
            <asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:SharedResources, ChangeLog %>"/>
            <small>
                <asp:Literal ID="ltProject" runat="server"/>
                <span>(<asp:Literal ID="litProjectCode" runat="Server"></asp:Literal>)</span>
            </small>
        </h1>
    </div>
    <div class="pull-right">
        <asp:Localize runat="server" ID="Sort" Text="Sort:" meta:resourcekey="SortLabel"/>
        <asp:LinkButton ID="Linkbutton7" runat="server" OnClick="SortMilestone_Click" CommandArgument="false"
                        meta:resourcekey="MostRecent" Text="Most Recent">
        </asp:LinkButton>
        |
        <asp:LinkButton ID="Linkbutton9" runat="server" CommandArgument="true" meta:resourcekey="FirstToLast"
                        OnClick="SortMilestone_Click" Text="First To Last">
        </asp:LinkButton>
    </div>
    <div>
        <asp:LinkButton ID="PreviousMilestones" runat="server" OnClick="SwitchViewMode_Click"
                        CommandArgument="1" meta:resourcekey="PreviousMilestones" Text="Top 5 milestones">
        </asp:LinkButton>
        <asp:LinkButton ID="Linkbutton5" runat="server" OnClick="SwitchViewMode_Click" CommandArgument="2"
                        meta:resourcekey="AllMilestones" Text="All milestones">
        </asp:LinkButton>
    </div>
    <asp:Repeater runat="server" ID="ChangeLogRepeater" OnItemDataBound="ChangeLogRepeater_ItemDataBound">
        <ItemTemplate>
            <div class="panel panel-default">
                <div class="milestone-group-header panel-heading" data-toggle="collapse" data-target="#milestone-content-<%#DataBinder.Eval(Container.DataItem, "Id") %>">
                    <div class="pull-right">
                        <asp:HyperLink ID="IssuesCount" runat="server"/>
                    </div>
                    <h4 class="panel-title">
                        <asp:HyperLink ID="MilestoneLink" CssClass="milestoneName" runat="server"/>
                        <small>
                            <asp:Label CssClass="milestoneNotes" ID="MilestoneNotes" runat="Server"></asp:Label>
                        </small>
                    </h4>
                    <span class="milestone-release-date">
                        <asp:Label ID="lblReleaseDate" runat="Server"></asp:Label>
                    </span>
                    <span style="font-weight: normal;">
                        <asp:HyperLink ID="ReleaseNotes" runat="server" Text="Release Notes" meta:resourcekey="ReleaseNotes"/>
                    </span>
                </div>
                <div class="milestone-content panel-collapse collapse" id="milestone-content-<%#DataBinder.Eval(Container.DataItem, "Id") %>">
                    <div class="panel-body">
                        <asp:Repeater ID="IssuesList" OnItemCreated="rptChangeLog_ItemCreated" runat="server">
                            <HeaderTemplate>
                                <div class="table-responsive">
                                <table class="table table-hover table-striped">
                                <tr>
                                    <th runat="server" id="tdId" class="gridHeader">
                                        <asp:LinkButton ID="LinkButton2" runat="server" Text="<%$ Resources:SharedResources, Id %>" CommandName="Id" CommandArgument="iv.[IssueId]" OnClick="SortIssueClick"/>
                                    </th>
                                    <th runat="server" id="tdTitle" class="gridHeader">
                                        <asp:LinkButton ID="LinkButton3" runat="server" Text="<%$ Resources:SharedResources, Title %>" CommandName="Title" CommandArgument="iv.[IssueTitle]" OnClick="SortIssueClick"/>
                                    </th>
                                    <th runat="server" id="tdCategory" class="gridHeader">
                                        <asp:LinkButton ID="lnkCategory" runat="server" Text="<%$ Resources:SharedResources, Category %>" CommandName="Category" CommandArgument="iv.[CategoryName]" OnClick="SortIssueClick"/>
                                    </th>
                                    <th runat="server" id="tdAssigned" class="gridHeader">
                                        <asp:LinkButton ID="LinkButton4" runat="server" Text="<%$ Resources:SharedResources, AssignedTo %>" CommandName="Assigned" CommandArgument="iv.[AssignedDisplayName]" OnClick="SortIssueClick"/>
                                    </th>
                                    <th runat="server" id="tdIssueType" class="gridHeader" style="text-align: center;">
                                        <asp:LinkButton ID="LinkButton1" runat="server" Text="<%$ Resources:SharedResources, Type %>" CommandName="IssueType" CommandArgument="iv.[IssueTypeSortOrder]" OnClick="SortIssueClick"/>
                                    </th>
                                    <th runat="server" id="tdStatus" class="gridHeader" style="text-align: center;">
                                        <asp:LinkButton ID="LinkButton6" runat="server" Text="<%$ Resources:SharedResources, Status %>" CommandName="Status" CommandArgument="iv.[StatusSortOrder]" OnClick="SortIssueClick"/>
                                    </th>
                                    <th runat="server" id="tdPriority" class="gridHeader" style="text-align: center;">
                                        <asp:LinkButton ID="LinkButton8" runat="server" Text="<%$ Resources:SharedResources, Priority %>" CommandName="Priority" CommandArgument="iv.[PrioritySortOrder]" OnClick="SortIssueClick"/>
                                    </th>
                                    <th runat="server" id="tdResolution" class="gridHeader" style="text-align: center;">
                                        <asp:LinkButton ID="LinkButton10" runat="server" Text="<%$ Resources:SharedResources, Resolution %>" CommandName="Resolution" CommandArgument="iv.[ResolutionSortOrder]" OnClick="SortIssueClick"/>
                                    </th>
                                </tr>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <asp:HyperLink runat="server" NavigateUrl='<%# string.Format("~/Issues/IssueDetail.aspx?id={0}", DataBinder.Eval(Container.DataItem, "Id")) %>'><%#DataBinder.Eval(Container.DataItem, "FullId") %></asp:HyperLink>
                                    </td>
                                    <td>
                                        <asp:HyperLink runat="server" NavigateUrl='<%# string.Format("~/Issues/IssueDetail.aspx?id={0}", DataBinder.Eval(Container.DataItem, "Id")) %>'><%#DataBinder.Eval(Container.DataItem, "Title") %></asp:HyperLink>
                                    </td>
                                    <td>
                                        <asp:Label ID="lblComponent" Text='<%# DataBinder.Eval(Container.DataItem, "CategoryName") %>'
                                                   runat="Server">
                                        </asp:Label>
                                    </td>
                                    <td>
                                        <asp:Label ID="lblAssignedTo" Text='<%# DataBinder.Eval(Container.DataItem, "AssignedDisplayName") %>'
                                                   runat="Server">
                                        </asp:Label>
                                    </td>
                                    <td style="text-align: center;">
                                        <it:TextImage ID="ctlType" ImageDirectory="/IssueType" Text='<%# DataBinder.Eval(Container.DataItem, "IssueTypeName") %>'
                                                      ImageUrl='<%# DataBinder.Eval(Container.DataItem, "IssueTypeImageUrl") %>' runat="server"/>
                                    </td>
                                    <td style="text-align: center;">
                                        <it:TextImage ID="ctlStatus" ImageDirectory="/Status" Text='<%# DataBinder.Eval(Container.DataItem, "StatusName") %>'
                                                      ImageUrl='<%# DataBinder.Eval(Container.DataItem, "StatusImageUrl") %>' runat="server"/>
                                    </td>
                                    <td style="text-align: center;">
                                        <it:TextImage ID="ctlPriority" ImageDirectory="/Priority" Text='<%# DataBinder.Eval(Container.DataItem, "PriorityName") %>'
                                                      ImageUrl='<%# DataBinder.Eval(Container.DataItem, "PriorityImageUrl") %>' runat="server"/>
                                    </td>
                                    <td style="text-align: center;">
                                        <it:TextImage ID="ctlResolution" ImageDirectory="/Resolution" Text='<%# DataBinder.Eval(Container.DataItem, "ResolutionName") %>'
                                                      ImageUrl='<%# DataBinder.Eval(Container.DataItem, "ResolutionImageUrl") %>' runat="server"/>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                            </table>
                            </div>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</asp:Content>