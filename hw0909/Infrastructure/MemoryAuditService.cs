using hw0909.Models;
using hw0909.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Infrastructure
{
    public class MemoryAuditService : IAuditService
    {
        private static readonly List<AuditRecord> _records = new List<AuditRecord>();
        private static readonly object _lock = new object();

        public void Write(int ticketId, string evt, string details)
        {
            lock (_lock)
                _records.Add(new AuditRecord
                { TicketId = ticketId, CreatedAt = DateTime.Now, EventName = evt, Details = details });
        }

        public IEnumerable<AuditRecord> GetByTicketId(int id)
        {
            lock (_lock) return _records.Where(r => r.TicketId == id).ToList();
        }
    }
}