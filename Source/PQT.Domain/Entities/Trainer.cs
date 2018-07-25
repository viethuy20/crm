using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Trainer : Entity
    {
        public Trainer()
        {
            TrainerBanks = new HashSet<TrainerBank>();
        }
        public string Name { get; set; }
        public string Passport { get; set; }
        public string Email { get; set; }
        public string BusinessPhone { get; set; }
        public string MobilePhone { get; set; }
        public string Address { get; set; }
        public string NearestInternationalAirport { get; set; }
        public string AirportIATACode { get; set; }
        public string Picture { get; set; }
        public virtual ICollection<TrainerBank> TrainerBanks { get; set; }
    }
}
