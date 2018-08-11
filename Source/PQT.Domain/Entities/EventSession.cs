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
        public EventSession(EventSession info)
        {
            ID = info.ID;
            SessionTitle = info.SessionTitle;
            ShortDescription = info.ShortDescription;
            Description = info.Description;
            Address = info.Address;
            StartDate = info.StartDate;
            EndDate = info.EndDate;
            EventID = info.EventID;
            TrainerID = info.TrainerID;
            Trainer = info.Trainer;
            CreatedTime = info.CreatedTime;
            UpdatedTime = info.UpdatedTime;
        }
        public string SessionTitle { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? EventID { get; set; }
        [ForeignKey("EventID")]
        public Event Event { get; set; }

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
