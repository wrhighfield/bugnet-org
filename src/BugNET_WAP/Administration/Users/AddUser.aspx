﻿<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
CodeBehind="AddUser.aspx.cs" Inherits="BugNET.Administration.Users.AddUser" meta:resourceKey="Page" Async="true" %>
<%@ Register TagPrefix="it" TagName="DisplayUserCustomFields" Src="~/UserControls/DisplayUserCustomFields.ascx" %>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h1 class="page-title">
            <asp:Localize ID="Localize1" runat="server" Text="<%$ Resources:AddUser %>"></asp:Localize>
        </h1>
    </div>
    <asp:ValidationSummary ID="ValidationSummary1" runat="server" DisplayMode="BulletList" HeaderText="<%$ Resources:SharedResources, ValidationSummaryHeaderText %>" CssClass="text-danger"/>
    <bn:Message ID="MessageContainer" runat="server" Visible="false"/>
    <p>
        <asp:Label ID="DescriptionLabel" runat="server" meta:resourcekey="DescriptionLabel"/>
    </p>
    <div class="form-horizontal">
        <div class="form-group">
            <asp:Label ID="Label2" CssClass="control-label col-md-2" AssociatedControlID="UserName" runat="server" Text="<%$ Resources:SharedResources, Username %>"/>
            <div class="col-md-10">
                <asp:TextBox ID="UserName" CssClass="form-control" runat="server"/>
                <asp:RequiredFieldValidator ID="rfvUserName" runat="server" ErrorMessage="<%$ Resources:UserNameRequiredErrorMessage %>" CssClass="text-danger"
                                            Display="Dynamic" ControlToValidate="UserName">
                </asp:RequiredFieldValidator>
            </div>
        </div>
        <div class="form-group">
            <asp:Label ID="Label1" CssClass="control-label col-md-2" AssociatedControlID="FirstName" runat="server" Text="<%$ Resources:SharedResources,FirstName %>"/>
            <div class="col-md-10">
                <asp:TextBox ID="FirstName" CssClass="form-control" runat="server"/>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" CssClass="text-danger"
                                            ErrorMessage="<%$ Resources:FirstNameRequiredErrorMessage %>" ControlToValidate="FirstName" Display="Dynamic">
                </asp:RequiredFieldValidator>
            </div>
        </div>
        <div class="form-group">
            <asp:Label ID="Label3" CssClass="control-label col-md-2" AssociatedControlID="LastName" runat="server"
                       Text="<%$ Resources:SharedResources,LastName %>"/>
            <div class="col-md-10">
                <asp:TextBox ID="LastName" CssClass="form-control" runat="server"/>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server"
                                            ErrorMessage="<%$ Resources:LastNameRequiredErrorMessage %>" CssClass="text-danger" ControlToValidate="LastName" Display="Dynamic">
                </asp:RequiredFieldValidator>
            </div>
        </div>
        <div class="form-group">
            <asp:Label ID="Label5" CssClass="control-label col-md-2" AssociatedControlID="DisplayName" runat="server"
                       Text="<%$ Resources:SharedResources,DisplayName %>"/>
            <div class="col-md-10">
                <asp:TextBox ID="DisplayName" CssClass="form-control" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" CssClass="text-danger" ErrorMessage="<%$ Resources:DisplayNameRequiredErrorMessage %>"
                                            ControlToValidate="DisplayName" Display="Dynamic">
                </asp:RequiredFieldValidator>
            </div>
        </div>
        <div class="form-group">
            <asp:Label ID="Label40" CssClass="control-label col-md-2" AssociatedControlID="Email" runat="server"
                       Text="<%$ Resources:SharedResources,Email %>"/>
            <div class="col-md-10">
                <asp:TextBox ID="Email" CssClass="form-control" runat="server"/>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="<%$ Resources:SharedResources, EmailRequiredErrorMessage %>"
                                            ControlToValidate="Email" Display="Dynamic" CssClass="text-danger">
                </asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="regexEmailValid" runat="server"
                                                ValidationExpression="^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$"
                                                ControlToValidate="Email" ErrorMessage="<%$ Resources:SharedResources, EmailFormatErrorMessage %>"/>
            </div>
        </div>
        <it:DisplayUserCustomFields id="ctlUserCustomFields" EnableValidation="true" runat="server"/>
        <h3>
            <asp:Literal ID="Literal1" runat="Server" Text="<%$ Resources:SharedResources,Password %>"/>
        </h3>
        <p>
            <asp:Literal ID="Literal2" runat="Server" Text="<%$ Resources:PasswordDescription %>"/>
        </p>
        <div class="form-group">
            <asp:Label ID="Label10" AssociatedControlID="chkRandomPassword" CssClass="control-label col-md-2" runat="server" Text="<%$ Resources:RandomPassword %>"/>
            <div class="col-md-10">
                <div class="checkbox">
                    <asp:CheckBox ID="chkRandomPassword" runat="server" AutoPostBack="true" OnCheckedChanged="RandomPasswordCheckChanged"/>
                </div>
            </div>
        </div>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div class="form-group">
                    <asp:Label ID="Label42" AssociatedControlID="Password" CssClass="control-label col-md-2" runat="server" Text="<%$ Resources:SharedResources,Password %>"/>
                    <div class="col-md-10">
                        <asp:TextBox ID="Password" TextMode="password" CssClass="form-control" runat="server"/>
                        <asp:RequiredFieldValidator ID="rvPassword" runat="server" CssClass="text-danger" ErrorMessage="<%$ Resources:SharedResources, PasswordRequiredErrorMessage %>" EnableClientScript="true"
                                                    ControlToValidate="Password" Display="Dynamic">
                        </asp:RequiredFieldValidator>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Label ID="Label41" AssociatedControlID="ConfirmPassword" CssClass="control-label col-md-2" runat="server" Text="<%$ Resources:SharedResources, ConfirmPassword %>"/>
                    <div class="col-md-10">
                        <asp:TextBox ID="ConfirmPassword" TextMode="password" CssClass="form-control" runat="server"/>
                        <asp:RequiredFieldValidator ID="rvConfirmPassword" runat="server" CssClass="text-danger" ErrorMessage="<%$ Resources:SharedResources, ConfirmPasswordRequiredErrorMessage %>"
                                                    EnableClientScript="true" ControlToValidate="ConfirmPassword"
                                                    Display="Dynamic">
                        </asp:RequiredFieldValidator>
                        <asp:CompareValidator ID="cvPassword"
                                              Display="dynamic" ControlToCompare="ConfirmPassword" CssClass="text-danger" ControlToValidate="Password"
                                              runat="server" ErrorMessage="<%$ Resources:SharedResources, ConfirmPasswordMismatchErrorMessage %>">
                        </asp:CompareValidator>
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="chkRandomPassword"/>
            </Triggers>
        </asp:UpdatePanel>
        <div class="form-group">
            <asp:Label ID="Label7" runat="server" CssClass="control-label col-md-2" AssociatedControlID="ActiveUser" Text="<%$ Resources:ActiveUser %>"/>
            <div class="col-md-10">
                <div class="checkbox">
                    <asp:CheckBox runat="server" ID="ActiveUser" Text="" TabIndex="106" Checked="True"/>
                </div>
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-2">
                <asp:LinkButton ID="AddNewUserLink" CssClass="btn btn-primary" runat="server" Text="<%$ Resources:AddNewUser %>" OnClick="AddNewUserClick"/>
                <asp:HyperLink ID="ReturnLink" runat="server" CssClass="btn btn-default" NavigateUrl="~/Administration/Users/UserList.aspx" Text="<%$ Resources:BackToUserList %>"></asp:HyperLink>
            </div>
        </div>
    </div>
</asp:Content>