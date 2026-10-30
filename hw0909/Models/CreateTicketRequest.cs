using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Models
{
    public class CreateTicketRequest
    {
        public string ApplicantName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }
}