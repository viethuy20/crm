using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Delegate : Entity
    {
        public string Name { get; set; }
        public string Designation { get; set; }
        public string LandLine { get; set; }
        public string Mobile1 { get; set; }
        public string Mobile2 { get; set; }
        public string Mobile3 { get; set; }
        public string Mail { get; set; }
    }
}
