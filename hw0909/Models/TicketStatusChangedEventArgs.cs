using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Models
{
    public class TicketStatusChangedEventArgs : EventArgs
    {
        public int TicketId { get; private set; }
        public TicketStatus OldStatus { get; private set; }
        public TicketStatus NewStatus { get; private set; }
        public TicketStatusChangedEventArgs(int ticketId, TicketStatus old, TicketStatus next)
        { TicketId = ticketId; OldStatus = old; NewStatus = next; }
    }
}