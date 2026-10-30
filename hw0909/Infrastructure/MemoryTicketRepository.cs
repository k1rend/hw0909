using hw0909.Models;
using hw0909.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Infrastructure
{
    public class MemoryTicketRepository : ITicketRepository
    {
        private static readonly List<Ticket> _tickets = new List<Ticket>();
        private static readonly object _lock = new object();

        public IEnumerable<Ticket> GetAll() { lock (_lock) return _tickets.ToList(); }
        public Ticket GetById(int id) { lock (_lock) return _tickets.FirstOrDefault(t => t.Id == id); }
        public void Add(Ticket t) { lock (_lock) _tickets.Add(t); }

        public void Update(Ticket t)
        {
            lock (_lock)
            {
                var existing = _tickets.FirstOrDefault(x => x.Id == t.Id);
                if (existing == null) return;
                existing.ApplicantName = t.ApplicantName;
                existing.Email = t.Email;
                existing.Department = t.Department;
                existing.Category = t.Category;
                existing.Description = t.Description;
                existing.Priority = t.Priority;
                existing.Status = t.Status;
                existing.CreatedAt = t.CreatedAt;
                existing.AttachmentPath = t.AttachmentPath;
            }
        }
    }
}