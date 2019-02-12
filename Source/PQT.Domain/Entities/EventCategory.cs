using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class EventCategory : Entity
    {
        public EventCategory()
        {
        }
        public EventCategory(EventCategory info)
        {
            Name = info.Name;
        }
        public string Name { get; set; }
    }
}
