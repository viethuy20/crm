using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PQT.Domain.Entities;

namespace PQT.Web.Models
{
    public class BookingViewModel
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int EventID { get; set; }
        public IEnumerable<Event> Events { get; set; }
        public BookingViewModel()
        {
            Events = new HashSet<Event>();
        }
    }
}