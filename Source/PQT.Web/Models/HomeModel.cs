using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PQT.Domain.Entities;

namespace PQT.Web.Models
{
    public class HomeModel
    {
        public IEnumerable<Event> Events { get; set; }
    }
}