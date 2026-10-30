using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Models
{
    public class AuditRecord
    {
        public int TicketId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EventName { get; set; }
        public string Details { get; set; }
    }
}