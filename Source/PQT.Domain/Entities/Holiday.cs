using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Holiday : Entity
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}
