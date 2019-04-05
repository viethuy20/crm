using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class EmailCompany : Entity
    {
        public EmailCompany()
        {
        }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Remarks { get; set; }
        public int NumberSentForToday { get; set; }
        public int AssignUserID { get; set; }
        [ForeignKey("AssignUserID")]
        public virtual User AssignUser { get; set; }
    }
}
