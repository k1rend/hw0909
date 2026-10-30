using hw0909.Models;
using hw0909.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Infrastructure
{
    public class DefaultPriorityPolicy : IPriorityPolicy
    {
        public TicketPriority Calculate(CreateTicketRequest r)
        {
            string d = (r.Description ?? "").ToLowerInvariant();
            string c = (r.Category ?? "").ToLowerInvariant();

            bool infra = c.Contains("сеть") || c.Contains("учетн") || c.Contains("учётн")
                       || c.Contains("права") || c.Contains("вход");

            bool criticalWords = d.Contains("не работает") || d.Contains("блокирует работу")
                               || d.Contains("нет доступа");

            if (criticalWords && infra) return TicketPriority.Critical;
            if (infra) return TicketPriority.High;
            if (c.Contains("офис") || d.Contains("информац")) return TicketPriority.Low;
            return TicketPriority.Normal;
        }
    }
}