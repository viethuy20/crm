using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class EmailTemplate : Entity
    {
        public string Content { get; set; }
        public string Subject { get; set; }
    }

    public class EmailSignature : Entity
    {
        public EmailSignature()
        {
            AssignUsers = new HashSet<User>();
        }
        public string Description { get; set; }
        public string Content { get; set; }
        public virtual ICollection<User> AssignUsers { get; set; }
    }
}
