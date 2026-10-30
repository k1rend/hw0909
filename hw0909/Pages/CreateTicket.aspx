<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateTicket.aspx.cs" Inherits="hw0909.Pages.CreateTicket" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Создание заявки</title>
    <style>
        body { font-family: Arial; max-width: 900px; margin: 20px auto; }
        .step { padding: 10px; }
        .info { background: #eef; padding: 8px; margin: 8px 0; border-left: 3px solid #48f; }
        table.summary td { padding: 4px 12px 4px 0; vertical-align: top; }
        .err { color: red; }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <h2>Создание заявки в Service Desk</h2>

    <asp:ValidationSummary ID="ValidationSummary1" runat="server"
        ValidationGroup="WizardGroup"
        ForeColor="Red" HeaderText="Исправьте следующие ошибки:" />

    <asp:Wizard ID="Wizard1" runat="server"
        ActiveStepIndex="0"
        ValidationGroup="WizardGroup"
        DisplayCancelButton="true"
        OnActiveStepChanged="Wizard1_ActiveStepChanged"
        OnNextButtonClick="Wizard1_NextButtonClick"
        OnFinishButtonClick="Wizard1_FinishButtonClick"
        OnCancelButtonClick="Wizard1_CancelButtonClick">
        <WizardSteps>

            <asp:WizardStep runat="server" Title="Контакты" StepType="Start">
                <div class="step">
                    <h3>Шаг 1. Контактные данные</h3>

                    <div>
                        <label>ФИО *</label><br />
                        <asp:TextBox ID="txtName" runat="server" Width="300" />
                        <asp:RequiredFieldValidator ID="rfvName" runat="server"
                            ControlToValidate="txtName"
                            ErrorMessage="Укажите ФИО"
                            ValidationGroup="WizardGroup"
                            Display="Dynamic" ForeColor="Red" />
                    </div>

                    <div>
                        <label>Email *</label><br />
                        <asp:TextBox ID="txtEmail" runat="server" Width="300" />
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                            ControlToValidate="txtEmail"
                            ErrorMessage="Укажите Email"
                            ValidationGroup="WizardGroup"
                            Display="Dynamic" ForeColor="Red" />
                        <asp:RegularExpressionValidator ID="revEmail" runat="server"
                            ControlToValidate="txtEmail"
                            ValidationExpression="^[\w\.\-]+@[\w\-]+\.[\w\.\-]+$"
                            ErrorMessage="Некорректный формат Email"
                            ValidationGroup="WizardGroup"
                            Display="Dynamic" ForeColor="Red" />
                    </div>

                    <div>
                        <label>Отдел</label><br />
                        <asp:DropDownList ID="ddlDepartment" runat="server">
                            <asp:ListItem Text="-- не выбран --" Value="" />
                            <asp:ListItem Text="IT" Value="IT" />
                            <asp:ListItem Text="Бухгалтерия" Value="Бухгалтерия" />
                            <asp:ListItem Text="Отдел кадров" Value="Отдел кадров" />
                            <asp:ListItem Text="Продажи" Value="Продажи" />
                            <asp:ListItem Text="Производство" Value="Производство" />
                        </asp:DropDownList>
                    </div>
                </div>
            </asp:WizardStep>

            <asp:WizardStep runat="server" Title="Категория">
                <div class="step">
                    <h3>Шаг 2. Категория</h3>
                    <asp:TreeView ID="TreeCategories" runat="server"
                        ShowLines="true"
                        OnSelectedNodeChanged="TreeCategories_SelectedNodeChanged">
                        <Nodes>
                            <asp:TreeNode Text="Программное обеспечение" Value="software" SelectAction="Expand">
                                <asp:TreeNode Text="ASP.NET"            Value="software/aspnet" />
                                <asp:TreeNode Text="База данных"        Value="software/db" />
                                <asp:TreeNode Text="Офисные программы"  Value="software/office" />
                            </asp:TreeNode>
                            <asp:TreeNode Text="Оборудование" Value="hardware" SelectAction="Expand">
                                <asp:TreeNode Text="Компьютер"    Value="hardware/pc" />
                                <asp:TreeNode Text="Периферия"    Value="hardware/periph" />
                                <asp:TreeNode Text="Сеть"         Value="hardware/network" />
                            </asp:TreeNode>
                            <asp:TreeNode Text="Учётная запись" Value="account" SelectAction="Expand">
                                <asp:TreeNode Text="Вход в систему" Value="account/login" />
                                <asp:TreeNode Text="Права доступа"  Value="account/rights" />
                            </asp:TreeNode>
                        </Nodes>
                    </asp:TreeView>

                    <asp:CustomValidator ID="cvCategory" runat="server"
                        ErrorMessage="Выберите конечную категорию"
                        ValidationGroup="WizardGroup"
                        OnServerValidate="cvCategory_ServerValidate"
                        Display="Dynamic" ForeColor="Red" />

                    <div class="info">
                        <asp:Label ID="lblSelectedCategory" runat="server" />
                        <br />
                        <asp:Label ID="lblCategoryHint" runat="server" ForeColor="#555" />
                    </div>
                </div>
            </asp:WizardStep>

            <asp:WizardStep runat="server" Title="Описание">
                <div class="step">
                    <h3>Шаг 3. Описание и вложение</h3>

                    <div>
                        <label>Описание проблемы (мин. 20 символов) *</label><br />
                        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine"
                            Rows="6" Width="500" />
                        <asp:RequiredFieldValidator ID="rfvDescription" runat="server"
                            ControlToValidate="txtDescription"
                            ErrorMessage="Введите описание"
                            ValidationGroup="WizardGroup"
                            Display="Dynamic" ForeColor="Red" />
                        <asp:CustomValidator ID="cvDescriptionLength" runat="server"
                            ControlToValidate="txtDescription"
                            ErrorMessage="Описание должно содержать минимум 20 символов"
                            ValidationGroup="WizardGroup"
                            OnServerValidate="cvDescriptionLength_ServerValidate"
                            Display="Dynamic" ForeColor="Red" />
                    </div>

                    <div>
                        <label>Вложение (txt, pdf, png, jpg, jpeg, до 2 МБ)</label><br />
                        <asp:FileUpload ID="fuAttachment" runat="server" />
                        <asp:CustomValidator ID="cvAttachment" runat="server"
                            ErrorMessage="Недопустимое вложение"
                            ValidationGroup="WizardGroup"
                            OnServerValidate="cvAttachment_ServerValidate"
                            Display="Dynamic" ForeColor="Red" />
                    </div>

                    <asp:Label ID="lblStep3Error" runat="server" CssClass="err" />
                </div>
            </asp:WizardStep>

            <asp:WizardStep runat="server" Title="Подтверждение" StepType="Finish">
                <div class="step">
                    <h3>Шаг 4. Подтверждение</h3>
                    <table class="summary">
                        <tr><td>ФИО:</td><td><asp:Label ID="lblSummaryName" runat="server" /></td></tr>
                        <tr><td>Email:</td><td><asp:Label ID="lblSummaryEmail" runat="server" /></td></tr>
                        <tr><td>Отдел:</td><td><asp:Label ID="lblSummaryDept" runat="server" /></td></tr>
                        <tr><td>Категория:</td><td><asp:Label ID="lblSummaryCategory" runat="server" /></td></tr>
                        <tr><td>Описание:</td><td><asp:Label ID="lblSummaryDescription" runat="server" /></td></tr>
                    </table>
                </div>
            </asp:WizardStep>

        </WizardSteps>
    </asp:Wizard>

    <p><asp:Label ID="lblResult" runat="server" /></p>
    <p><a href="Tickets.aspx">Перейти к списку заявок</a></p>
</form>
</body>
</html>