using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class OfficeLocation : Entity
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
