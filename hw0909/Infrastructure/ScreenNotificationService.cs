using hw0909.Models;
using hw0909.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Infrastructure
{
    public class ScreenNotificationService : INotificationService
    {
        public string Send(Ticket t, string evt)
            => $"[{evt}] Заявка #{t.Id} «{t.ApplicantName}» — статус: {t.Status}, приоритет: {t.Priority}";
    }
}