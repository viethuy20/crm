using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Delegate : Entity
    {
        public string Name { get; set; }
        public string Destination { get; set; }
        public string Tel { get; set; }
        public string Mail { get; set; }
    }
}
