using hw0909.Infrastructure;
using hw0909.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace hw0909.Pages
{
    public partial class Tickets : System.Web.UI.Page
    {
        private bool _rowDataBoundLogged;

        private List<string> EventLog
        {
            get
            {
                var log = ViewState["EventLog"] as List<string>;
                if (log == null) { log = new List<string>(); ViewState["EventLog"] = log; }
                return log;
            }
        }

        private void LogEvent(string text)
        {
            EventLog.Add($"{DateTime.Now:HH:mm:ss.fff} — {text}");
            rptEventLog.DataSource = EventLog;
            rptEventLog.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            LogEvent($"Page_Load (IsPostBack={IsPostBack})");
            if (!IsPostBack) BindGrid();
        }

        protected void btnApply_Click(object sender, EventArgs e)
        {
            LogEvent($"Кнопка фильтра: search='{txtSearch.Text}' " +
                     $"status='{ddlStatusFilter.SelectedValue}' " +
                     $"priority='{ddlPriorityFilter.SelectedValue}'");
            gvTickets.PageIndex = 0;
            BindGrid();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            LogEvent("Сброс фильтра");
            txtSearch.Text = "";
            ddlStatusFilter.SelectedIndex = 0;
            ddlPriorityFilter.SelectedIndex = 0;
            gvTickets.PageIndex = 0;
            BindGrid();
        }

        protected void gvTickets_Sorting(object sender, GridViewSortEventArgs e)
        {
            LogEvent($"Sorting: {e.SortExpression}");
            string current = ViewState["SortExpression"] as string;
            string dir = ViewState["SortDirection"] as string ?? "DESC";

            if (current == e.SortExpression)
                dir = dir == "ASC" ? "DESC" : "ASC";
            else dir = "ASC";

            ViewState["SortExpression"] = e.SortExpression;
            ViewState["SortDirection"] = dir;
            BindGrid();
        }

        protected void gvTickets_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            LogEvent($"PageIndexChanging: {e.NewPageIndex}");
            gvTickets.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        protected void gvTickets_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            LogEvent($"RowCommand: {e.CommandName} arg={e.CommandArgument}");
            if (e.CommandName == "Open")
            {
                int id;
                if (int.TryParse(Convert.ToString(e.CommandArgument), out id))
                {
                    lblEventMessage.Text = "";
                    lblStatusMessage.Text = "";
                    ShowDetails(id);
                }
            }
        }

        protected void gvTickets_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (!_rowDataBoundLogged)
            {
                LogEvent("RowDataBound (первое связывание)");
                _rowDataBoundLogged = true;
            }

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var t = e.Row.DataItem as Ticket;
                if (t != null) e.Row.CssClass = "priority-" + t.Priority.ToString().ToLower();
            }
        }

        private void BindGrid()
        {
            _rowDataBoundLogged = false;

            var service = ServiceFactory.CreateTicketService();
            var q = service.GetAllTickets().AsEnumerable();

            string search = (txtSearch.Text ?? "").Trim();
            if (!string.IsNullOrEmpty(search))
                q = q.Where(t =>
                    (t.ApplicantName ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.Email ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.Description ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrEmpty(ddlStatusFilter.SelectedValue))
            {
                var st = (TicketStatus)Enum.Parse(typeof(TicketStatus), ddlStatusFilter.SelectedValue);
                q = q.Where(t => t.Status == st);
            }

            if (!string.IsNullOrEmpty(ddlPriorityFilter.SelectedValue))
            {
                var pr = (TicketPriority)Enum.Parse(typeof(TicketPriority), ddlPriorityFilter.SelectedValue);
                q = q.Where(t => t.Priority == pr);
            }

            string sort = ViewState["SortExpression"] as string ?? "CreatedAt";
            bool asc = (ViewState["SortDirection"] as string ?? "DESC") == "ASC";

            switch (sort)
            {
                case "Id": q = asc ? q.OrderBy(t => t.Id) : q.OrderByDescending(t => t.Id); break;
                case "ApplicantName": q = asc ? q.OrderBy(t => t.ApplicantName) : q.OrderByDescending(t => t.ApplicantName); break;
                case "Category": q = asc ? q.OrderBy(t => t.Category) : q.OrderByDescending(t => t.Category); break;
                case "Priority": q = asc ? q.OrderBy(t => (int)t.Priority) : q.OrderByDescending(t => (int)t.Priority); break;
                case "Status": q = asc ? q.OrderBy(t => t.Status) : q.OrderByDescending(t => t.Status); break;
                default: q = asc ? q.OrderBy(t => t.CreatedAt) : q.OrderByDescending(t => t.CreatedAt); break;
            }

            gvTickets.DataSource = q.ToList();
            gvTickets.DataBind();
        }

        private void ShowDetails(int id)
        {
            var service = ServiceFactory.CreateTicketService();
            var ticket = service.GetTicket(id);
            if (ticket == null) { lblMessage.Text = "Заявка не найдена"; return; }

            hfSelectedTicketId.Value = id.ToString();

            DetailsView1.DataSource = new[] { ticket };
            DetailsView1.DataBind();

            rptHistory.DataSource = service.GetHistory(id).ToList();
            rptHistory.DataBind();

            lnkTakeInProgress.CommandArgument = id.ToString();
            lnkWaitUser.CommandArgument = id.ToString();
            lnkClose.CommandArgument = id.ToString();
            lnkReopen.CommandArgument = id.ToString();

            UpdateStatusButtons(ticket.Status);

            MultiView1.ActiveViewIndex = 1;
        }

        private void UpdateStatusButtons(TicketStatus status)
        {
            lnkTakeInProgress.Visible = status == TicketStatus.New;
            lnkWaitUser.Visible = status == TicketStatus.InProgress;
            lnkClose.Visible = status == TicketStatus.InProgress
                                     || status == TicketStatus.WaitingForUser;
            lnkReopen.Visible = status == TicketStatus.WaitingForUser;
        }

        protected void lnkStatusChange_Click(object sender, EventArgs e)
        {
            var btn = (LinkButton)sender;
            int id;
            if (!int.TryParse(btn.CommandArgument, out id))
            {
                lblStatusMessage.Text = "Не удалось определить заявку";
                return;
            }

            LogEvent($"Нажата статус-команда: {btn.CommandName} для #{id}");

            TicketStatus target;
            switch (btn.CommandName)
            {
                case "TakeInProgress": target = TicketStatus.InProgress; break;
                case "WaitUser": target = TicketStatus.WaitingForUser; break;
                case "Close": target = TicketStatus.Closed; break;
                case "Reopen": target = TicketStatus.InProgress; break;
                default:
                    lblStatusMessage.Text = "Неизвестная команда"; return;
            }

            var service = ServiceFactory.CreateTicketService();
            service.StatusChanged += OnServiceStatusChanged;  // IoC через событие сервиса

            string message;
            bool ok = service.ChangeStatus(id, target, out message);

            string eventMsg = lblEventMessage.Text; // сохраняем до ShowDetails
            ShowDetails(id);

            lblEventMessage.Text = eventMsg;
            lblStatusMessage.Text = message;
            lblStatusMessage.CssClass = "msg " + (ok ? "success" : "error");
        }

        private void OnServiceStatusChanged(object sender, TicketStatusChangedEventArgs e)
        {
            lblEventMessage.Text =
                $"Событие StatusChanged: #{e.TicketId} {e.OldStatus} → {e.NewStatus}";
            LogEvent($"TicketService.StatusChanged: #{e.TicketId} {e.OldStatus}->{e.NewStatus}");
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            LogEvent("Возврат к списку");
            MultiView1.ActiveViewIndex = 0;
            BindGrid();
        }
    }
}