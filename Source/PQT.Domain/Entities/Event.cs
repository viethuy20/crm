using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Event : Entity
    {
        public Event()
        {
            var rand = new Random((int)DateTime.Now.Ticks).Next(0x1000000);
            EventSessions = new HashSet<EventSession>();
            SalesGroups = new HashSet<SalesGroup>();
            Users = new HashSet<User>();
            Companies = new HashSet<Company>();
            //Trainers = new HashSet<Trainer>();
            BackgroundColor = string.Format("#{0:X6}", rand);
        }
        public string EventCode { get; set; }
        public string EventName { get; set; }
        public string BackgroundColor { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string CallFilterKeywords { get; set; }
        public string Remark { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        public virtual ICollection<EventSession> EventSessions { get; set; }
        //for assign sales group
        public virtual ICollection<SalesGroup> SalesGroups { get; set; }
        //for assign salesman
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Company> Companies { get; set; }
        //public virtual ICollection<Trainer> Trainers { get; set; }
    }
}
