using hw0909.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Services
{
    public interface IAuditService
    {
        void Write(int ticketId, string eventName, string details);
        IEnumerable<AuditRecord> GetByTicketId(int ticketId);
    }
}