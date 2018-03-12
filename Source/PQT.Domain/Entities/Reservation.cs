using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Reservation : Entity
    {
        public Reservation()
        {
            ProjectSessions = new HashSet<ProjectSession>();
        }
        public DateTime ReservationDate { get; set; }
        public int? ReservationStatusID { get; set; }
        [ForeignKey("ReservationStatusID")]
        public virtual ReservationStatusRecord ReservationStatusRecord { get; set; }
        public int LeadID { get; set; }
        [ForeignKey("LeadID")]
        public virtual Lead Lead { get; set; }

        public virtual ICollection<ProjectSession> ProjectSessions { get; set; }
    }
}
