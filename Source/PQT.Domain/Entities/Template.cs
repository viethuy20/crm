using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Template : Entity
    {
        public string GroupName { get; set; }
        public string FileName { get; set; }
    }
}
