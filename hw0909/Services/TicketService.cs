using hw0909.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Services
{
    public class TicketService
    {
        private readonly ITicketRepository repository;
        private readonly IPriorityPolicy priorityPolicy;
        private readonly INotificationService notificationService;
        private readonly IAuditService auditService;
        private readonly IAttachmentStorage attachmentStorage;

        // static lock, чтобы защитить генерацию Id между экземплярами сервиса
        private static readonly object _idLock = new object();

        public event EventHandler<TicketStatusChangedEventArgs> StatusChanged;

        public TicketService(
            ITicketRepository repository,
            IPriorityPolicy priorityPolicy,
            INotificationService notificationService,
            IAuditService auditService,
            IAttachmentStorage attachmentStorage)
        {
            this.repository = repository;
            this.priorityPolicy = priorityPolicy;
            this.notificationService = notificationService;
            this.auditService = auditService;
            this.attachmentStorage = attachmentStorage;
        }

        public Ticket CreateTicket(CreateTicketRequest request, string fileName, byte[] content)
        {
            var priority = priorityPolicy.Calculate(request);

            string attachmentPath = null;
            if (!string.IsNullOrEmpty(fileName) && content != null && content.Length > 0)
            {
                try
                {
                    attachmentPath = attachmentStorage.Save(fileName, content);
                }
                catch (Exception ex)
                {
                    auditService.Write(0, "AttachmentError",
                        "Ошибка сохранения вложения: " + ex.Message);
                }
            }

            var ticket = new Ticket
            {
                ApplicantName = request.ApplicantName,
                Email = request.Email,
                Department = request.Department,
                Category = request.Category,
                Description = request.Description,
                Priority = priority,
                Status = TicketStatus.New,
                CreatedAt = DateTime.Now,
                AttachmentPath = attachmentPath
            };

            lock (_idLock)
            {
                var all = repository.GetAll().ToList();
                ticket.Id = all.Count > 0 ? all.Max(t => t.Id) + 1 : 1;
                repository.Add(ticket);
            }

            auditService.Write(ticket.Id, "Created",
                "Заявка создана. Заявитель: " + ticket.ApplicantName);
            auditService.Write(ticket.Id, "PriorityCalculated",
                "Рассчитан приоритет: " + priority);
            if (!string.IsNullOrEmpty(attachmentPath))
                auditService.Write(ticket.Id, "AttachmentSaved",
                    "Сохранено вложение: " + ticket.AttachmentPath);

            auditService.Write(ticket.Id, "Notification",
                notificationService.Send(ticket, "Created"));

            return ticket;
        }

        private static readonly Dictionary<TicketStatus, TicketStatus[]> Allowed =
            new Dictionary<TicketStatus, TicketStatus[]>
            {
                { TicketStatus.New,            new[] { TicketStatus.InProgress } },
                { TicketStatus.InProgress,     new[] { TicketStatus.WaitingForUser, TicketStatus.Closed } },
                { TicketStatus.WaitingForUser, new[] { TicketStatus.InProgress,     TicketStatus.Closed } },
                { TicketStatus.Closed,         new TicketStatus[0] }
            };

        public bool ChangeStatus(int ticketId, TicketStatus next, out string message)
        {
            var ticket = repository.GetById(ticketId);
            if (ticket == null) { message = "Заявка не найдена"; return false; }

            if (!(Allowed.ContainsKey(ticket.Status) && Allowed[ticket.Status].Contains(next)))
            {
                auditService.Write(ticketId, "InvalidTransition",
                    $"Недопустимый переход: {ticket.Status} -> {next}");
                message = $"Недопустимый переход статуса: {ticket.Status} -> {next}";
                return false;
            }

            var old = ticket.Status;
            ticket.Status = next;
            repository.Update(ticket);

            auditService.Write(ticketId, "StatusChanged", $"Статус: {old} -> {next}");
            auditService.Write(ticketId, "Notification",
                notificationService.Send(ticket, "StatusChanged"));

            OnStatusChanged(new TicketStatusChangedEventArgs(ticketId, old, next));
            message = $"Статус изменён: {old} -> {next}";
            return true;
        }

        public IEnumerable<Ticket> GetAllTickets() => repository.GetAll();
        public Ticket GetTicket(int id) => repository.GetById(id);
        public IEnumerable<AuditRecord> GetHistory(int id)
            => auditService.GetByTicketId(id).OrderBy(r => r.CreatedAt);

        protected virtual void OnStatusChanged(TicketStatusChangedEventArgs e)
        {
            var h = StatusChanged;
            if (h != null) h(this, e);
        }
    }
}