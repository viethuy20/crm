using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class OfficeLocation : Entity
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int CountryID { get; set; }
        [ForeignKey("CountryID")]
        public virtual Country Country { get; set; }

        public string CountryName
        {
            get { return Country != null ? Country.Name : ""; }
        }
    }
}
