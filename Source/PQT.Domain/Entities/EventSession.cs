using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class EventSession : Entity
    {
        public EventSession()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
        }
        public string SessionTitle { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? EventID { get; set; }
        [ForeignKey("EventID")]
        public virtual Event Event { get; set; }

        public int? TrainerID { get; set; }
        [ForeignKey("TrainerID")]
        public virtual Trainer Trainer { get; set; }

        public string TrainerName
        {
            get
            {
                if (Trainer!=null)
                {
                    return Trainer.Name;
                }

                return "";
            }
        }
    }
}
