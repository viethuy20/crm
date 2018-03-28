using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Registration : Entity
    {
        public Registration()
        {
            Delegates = new HashSet<Delegate>();
        }
        public decimal FeePerDelegate { get; set; }
        public double DiscountPercent { get; set; }
        public int CompanyID { get; set; }
        [ForeignKey("CompanyID")]
        public virtual Company Company { get; set; }

        public virtual ICollection<Delegate> Delegates { get; set; }
    }
}
