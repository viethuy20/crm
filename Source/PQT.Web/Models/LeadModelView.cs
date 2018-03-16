using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Infrastructure;

namespace PQT.Web.Models
{
    public class LeadModelView
    {
        public Event Event { get; set; }

        public void Prepare(int id)
        {
            var eventRepo = DependencyHelper.GetService<IEventService>();
            Event = eventRepo.GetEvent(id);
        }
    }
}