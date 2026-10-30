using hw0909.Infrastructure;
using hw0909.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace hw0909.Pages
{
    public partial class CreateTicket : System.Web.UI.Page
    {
        private static readonly string[] Allowed = { ".txt", ".pdf", ".png", ".jpg", ".jpeg" };
        private const int MaxFileSize = 2 * 1024 * 1024;

        protected void Page_Load(object sender, EventArgs e) { /* IoC-демо: вызывается фреймворком */ }

        protected void Wizard1_ActiveStepChanged(object sender, EventArgs e)
        {
            if (Wizard1.ActiveStepIndex == 3)
            {
                lblSummaryName.Text = Server.HtmlEncode(txtName.Text);
                lblSummaryEmail.Text = Server.HtmlEncode(txtEmail.Text);
                lblSummaryDept.Text = Server.HtmlEncode(ddlDepartment.SelectedValue);
                lblSummaryCategory.Text = Server.HtmlEncode(ViewState["SelectedCategory"] as string ?? "");
                lblSummaryDescription.Text = Server.HtmlEncode(txtDescription.Text);
            }
        }

        protected void TreeCategories_SelectedNodeChanged(object sender, EventArgs e)
        {
            var node = TreeCategories.SelectedNode;
            if (node == null) return;

            if (node.ChildNodes.Count > 0)
            {
                ViewState["SelectedCategory"] = null;
                lblSelectedCategory.Text = "Выберите конечную подкатегорию";
                lblCategoryHint.Text = "";
                return;
            }

            ViewState["SelectedCategory"] = node.Value;
            lblSelectedCategory.Text = "Выбрано: " + Server.HtmlEncode(node.Text);
            lblCategoryHint.Text = CategoryHint(node.Value);
        }

        private string CategoryHint(string value)
        {
            var parts = value.Split('/');
            if (parts.Length < 2) return "";
            switch (parts[0])
            {
                case "software": return "Проблемы с приложениями и системами.";
                case "hardware": return "Сбои ПК, периферии, сети.";
                case "account": return "Вход в систему и права доступа.";
                default: return "";
            }
        }

        protected void cvCategory_ServerValidate(object s, ServerValidateEventArgs a)
            => a.IsValid = !string.IsNullOrEmpty(ViewState["SelectedCategory"] as string);

        protected void cvDescriptionLength_ServerValidate(object s, ServerValidateEventArgs a)
            => a.IsValid = (a.Value ?? "").Trim().Length >= 20;

        protected void cvAttachment_ServerValidate(object s, ServerValidateEventArgs a)
        {
            if (!fuAttachment.HasFile) { a.IsValid = true; return; }
            string err = ValidateAttachment();
            if (err != null) { cvAttachment.ErrorMessage = err; a.IsValid = false; return; }
            a.IsValid = true;
        }

        private string ValidateAttachment()
        {
            if (!fuAttachment.HasFile) return null;
            string ext = Path.GetExtension(fuAttachment.FileName).ToLowerInvariant();
            if (!Allowed.Contains(ext))
                return "Недопустимый тип файла. Разрешены: .txt, .pdf, .png, .jpg, .jpeg";
            if (fuAttachment.PostedFile.ContentLength > MaxFileSize)
                return "Файл превышает 2 МБ";
            return null;
        }

        protected void Wizard1_NextButtonClick(object sender, WizardNavigationEventArgs e)
        {
            if (e.CurrentStepIndex == 1 && string.IsNullOrEmpty(ViewState["SelectedCategory"] as string))
            {
                e.Cancel = true;
                lblSelectedCategory.Text = "Выберите конечную категорию, чтобы продолжить.";
                return;
            }

            if (e.CurrentStepIndex == 2)
            {
                string err = ValidateAttachment();
                if (err != null) { e.Cancel = true; lblStep3Error.Text = err; }
                else lblStep3Error.Text = "";
            }
        }

        protected void Wizard1_FinishButtonClick(object sender, WizardNavigationEventArgs e)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(txtName.Text)) errors.Add("ФИО обязательно");
            if (string.IsNullOrWhiteSpace(txtEmail.Text)) errors.Add("Email обязателен");
            else if (!Regex.IsMatch(txtEmail.Text.Trim(), @"^[\w\.\-]+@[\w\-]+\.[\w\.\-]+$"))
                errors.Add("Некорректный Email");
            if (string.IsNullOrEmpty(ViewState["SelectedCategory"] as string))
                errors.Add("Не выбрана категория");
            if ((txtDescription.Text ?? "").Trim().Length < 20)
                errors.Add("Описание должно содержать минимум 20 символов");

            string attachErr = ValidateAttachment();
            if (attachErr != null) errors.Add(attachErr);

            if (errors.Count > 0)
            {
                e.Cancel = true;
                lblResult.Text = "Не удалось создать заявку: " + string.Join("; ", errors);
                return;
            }

            var request = new CreateTicketRequest
            {
                ApplicantName = txtName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Department = ddlDepartment.SelectedValue,
                Category = ViewState["SelectedCategory"] as string,
                Description = txtDescription.Text.Trim()
            };

            byte[] content = null; string fileName = null;
            if (fuAttachment.HasFile)
            {
                content = fuAttachment.FileBytes;
                fileName = Path.GetFileName(fuAttachment.FileName);
            }

            try
            {
                var service = ServiceFactory.CreateTicketService();
                var ticket = service.CreateTicket(request, fileName, content);
                lblResult.Text = $"Заявка #{ticket.Id} создана. Приоритет: {ticket.Priority}. Статус: {ticket.Status}.";
            }
            catch (Exception ex)
            {
                lblResult.Text = "Ошибка создания заявки: " + Server.HtmlEncode(ex.Message);
            }
        }

        protected void Wizard1_CancelButtonClick(object sender, EventArgs e)
            => Response.Redirect("Tickets.aspx");
    }
}

