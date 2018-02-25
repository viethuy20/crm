using System;
using System.Collections.Generic;
using System.Linq;
using NS;

namespace PQT.Domain.Entities
{
    public class Menu : EntityBase
    {
        public Menu()
        {
            Roles = new HashSet<Role>();
        }

        public string Title { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public int ParentID { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
    }

}
