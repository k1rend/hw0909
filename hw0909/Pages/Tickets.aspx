<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Tickets.aspx.cs" Inherits="hw0909.Pages.Tickets" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Заявки — операторская консоль</title>
    <style>
        body { font-family: Arial; margin: 20px; }
        .filters { background: #f4f4f4; padding: 10px; margin-bottom: 10px; }
        .filters input, .filters select { margin: 4px; }
        .log { background: #fafafa; border: 1px solid #ccc; padding: 8px; max-height: 200px;
               overflow: auto; font-family: monospace; font-size: 11px; }
        .priority-critical { background: #ffdddd; }
        .priority-high     { background: #ffe8cc; }
        .priority-low      { background: #eef7ee; }
        .success { color: green; }
        .error   { color: red; }
        .msg     { padding: 8px; margin: 8px 0; display:block; }
        .linkbtn { margin-right: 8px; }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" />

    <h2>Операторская консоль Service Desk</h2>

    <asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">

        <%-- VIEW 0: СПИСОК --%>
        <asp:View ID="ViewList" runat="server">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="filters">
                        <label>Поиск (ФИО/Email/описание):</label>
                        <asp:TextBox ID="txtSearch" runat="server" />
                        <label>Статус:</label>
                        <asp:DropDownList ID="ddlStatusFilter" runat="server">
                            <asp:ListItem Text="Все" Value="" />
                            <asp:ListItem Text="New" Value="New" />
                            <asp:ListItem Text="InProgress" Value="InProgress" />
                            <asp:ListItem Text="WaitingForUser" Value="WaitingForUser" />
                            <asp:ListItem Text="Closed" Value="Closed" />
                        </asp:DropDownList>
                        <label>Приоритет:</label>
                        <asp:DropDownList ID="ddlPriorityFilter" runat="server">
                            <asp:ListItem Text="Все" Value="" />
                            <asp:ListItem Text="Low" Value="Low" />
                            <asp:ListItem Text="Normal" Value="Normal" />
                            <asp:ListItem Text="High" Value="High" />
                            <asp:ListItem Text="Critical" Value="Critical" />
                        </asp:DropDownList>
                        <asp:Button ID="btnApply" runat="server" Text="Применить фильтр"
                            OnClick="btnApply_Click" />
                        <asp:Button ID="btnReset" runat="server" Text="Сбросить"
                            OnClick="btnReset_Click" />
                    </div>

                    <asp:Label ID="lblMessage" runat="server" CssClass="msg" />

                    <asp:GridView ID="gvTickets" runat="server"
                        AutoGenerateColumns="false"
                        AllowPaging="true" PageSize="5"
                        AllowSorting="true"
                        DataKeyNames="Id"
                        Width="100%"
                        OnRowCommand="gvTickets_RowCommand"
                        OnSorting="gvTickets_Sorting"
                        OnPageIndexChanging="gvTickets_PageIndexChanging"
                        OnRowDataBound="gvTickets_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="Id"            HeaderText="Id"        SortExpression="Id" />
                            <asp:BoundField DataField="ApplicantName" HeaderText="ФИО"       SortExpression="ApplicantName" />
                            <asp:BoundField DataField="Category"      HeaderText="Категория" SortExpression="Category" />
                            <asp:BoundField DataField="Priority"      HeaderText="Приоритет" SortExpression="Priority" />
                            <asp:BoundField DataField="Status"        HeaderText="Статус"    SortExpression="Status" />
                            <asp:BoundField DataField="CreatedAt"     HeaderText="Дата"
                                SortExpression="CreatedAt" DataFormatString="{0:dd.MM.yyyy HH:mm}" />
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkOpen" runat="server"
                                        CommandName="Open"
                                        CommandArgument='<%# Eval("Id") %>'
                                        Text="Открыть" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:View>

        <%-- VIEW 1: КАРТОЧКА --%>
        <asp:View ID="ViewDetails" runat="server">
            <asp:Label ID="lblEventMessage"  runat="server" CssClass="msg" ForeColor="Blue" />
            <asp:Label ID="lblStatusMessage" runat="server" CssClass="msg" />

            <asp:DetailsView ID="DetailsView1" runat="server"
                AutoGenerateRows="true" Width="100%" />

            <h3>Смена статуса</h3>
            <asp:LinkButton ID="lnkTakeInProgress" runat="server" Text="Взять в работу"
                CommandName="TakeInProgress" CommandArgument=""
                OnClick="lnkStatusChange_Click" CssClass="linkbtn" />
            <asp:LinkButton ID="lnkWaitUser" runat="server" Text="Ожидание пользователя"
                CommandName="WaitUser" CommandArgument=""
                OnClick="lnkStatusChange_Click" CssClass="linkbtn" />
            <asp:LinkButton ID="lnkClose" runat="server" Text="Закрыть заявку"
                CommandName="Close" CommandArgument=""
                OnClick="lnkStatusChange_Click" CssClass="linkbtn" />
            <asp:LinkButton ID="lnkReopen" runat="server" Text="Вернуть в работу"
                CommandName="Reopen" CommandArgument=""
                OnClick="lnkStatusChange_Click" CssClass="linkbtn" />

            <h3>История действий</h3>
            <asp:Repeater ID="rptHistory" runat="server">
                <HeaderTemplate>
                    <table border="1" cellpadding="4" style="border-collapse:collapse;">
                        <tr><th>Время</th><th>Событие</th><th>Детали</th></tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("CreatedAt", "{0:dd.MM.yyyy HH:mm:ss}") %></td>
                        <td><%#: Eval("EventName") %></td>
                        <td><%#: Eval("Details") %></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></table></FooterTemplate>
            </asp:Repeater>

            <br />
            <asp:Button ID="btnBack" runat="server" Text="Назад к списку"
                OnClick="btnBack_Click" />
            <asp:HiddenField ID="hfSelectedTicketId" runat="server" />
        </asp:View>
    </asp:MultiView>

    <h3>Журнал событий страницы</h3>
    <div class="log">
        <asp:Repeater ID="rptEventLog" runat="server">
            <ItemTemplate><div><%#: Container.DataItem %></div></ItemTemplate>
        </asp:Repeater>
    </div>
</form>
</body>
</html>