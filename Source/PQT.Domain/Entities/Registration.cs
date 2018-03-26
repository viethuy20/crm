using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Registration : Entity
    {
        public int? LeadID { get; set; }
        [ForeignKey("LeadID")]
        public virtual Lead Lead { get; set; }
    }
}
