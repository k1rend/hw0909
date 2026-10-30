using hw0909.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Services
{
    public interface INotificationService
    {
        string Send(Ticket ticket, string eventName);
    }
}