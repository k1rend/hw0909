using hw0909.Models;
using hw0909.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Infrastructure
{
    public class StrictPriorityPolicy : IPriorityPolicy
    {
        public TicketPriority Calculate(CreateTicketRequest r)
        {
            string d = (r.Description ?? "").ToLowerInvariant();
            string c = (r.Category ?? "").ToLowerInvariant();

            if (d.Contains("не работает") || d.Contains("блокирует")
             || d.Contains("нет доступа") || d.Contains("срочно")
             || d.Contains("авария") || d.Contains("не открывается"))
                return TicketPriority.Critical;

            if (c.Contains("сеть") || c.Contains("учетн") || c.Contains("учётн")
             || c.Contains("права") || c.Contains("вход")
             || c.Contains("оборудование") || c.Contains("база данных"))
                return TicketPriority.High;

            if (c.Contains("офис") || d.Contains("вопрос") || d.Contains("консультац"))
                return TicketPriority.Low;

            return TicketPriority.Normal;
        }
    }
}