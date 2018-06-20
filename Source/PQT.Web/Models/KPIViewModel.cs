using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PQT.Domain.Entities;

namespace PQT.Web.Models
{
    public class KPIViewModel
    {

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int EventID { get; set; }
        public int UserID { get; set; }
        public string StatusCode { get; set; }
        public IEnumerable<Event> Events { get; set; }
        public IEnumerable<User> Users { get; set; }
        public KPIViewModel()
        {
            DateTo = DateTime.Today;
            DateFrom = DateTime.Today.AddDays(-30);
            Events= new HashSet<Event>();
            Users = new HashSet<User>();
        }

    }
    
}