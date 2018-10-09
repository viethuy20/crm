using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PQT.Domain.Entities;

namespace PQT.Web.Models
{
    public class HomeModel
    {
        public HomeModel()
        {
            Events = new HashSet<Event>();
            CrossSellEvents = new HashSet<Event>();
        }
        public IEnumerable<Event> Events { get; set; }
        public IEnumerable<Event> CrossSellEvents { get; set; }
    }
}