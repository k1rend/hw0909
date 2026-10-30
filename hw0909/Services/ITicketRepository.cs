using hw0909.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Services
{
    public interface ITicketRepository
    {
        IEnumerable<Ticket> GetAll();
        Ticket GetById(int id);
        void Add(Ticket ticket);
        void Update(Ticket ticket);
    }
}