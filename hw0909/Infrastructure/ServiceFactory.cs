using hw0909.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Infrastructure
{
    public static class ServiceFactory
    {
        // Единственное место, где собираются конкретные реализации.
        // Здесь же — переключение реализаций для проверки взаимозаменяемости.
        public static TicketService CreateTicketService()
        {
            return new TicketService(
                new MemoryTicketRepository(),
                new DefaultPriorityPolicy(),       // ⟵ StrictPriorityPolicy
                new ScreenNotificationService(),   // ⟵ SilentNotificationService
                new MemoryAuditService(),
                new LocalAttachmentStorage()       // ⟵ NullAttachmentStorage
            );
        }
    }
}