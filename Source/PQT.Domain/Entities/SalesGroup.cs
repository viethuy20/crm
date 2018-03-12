using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class SalesGroup : Entity
    {
        public SalesGroup()
        {
            Users = new HashSet<User>();
        }
        public string GroupName { get; set; }
        public virtual ICollection<User> Users { get; set; }

        public string DisplayUsers
        {
            get { return string.Join(";", Users.Select(m => m.DisplayName)); }
        }
    }
}
