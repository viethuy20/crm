using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Event : Entity
    {
        public Event()
        {
            StartDate=DateTime.Today;
            EndDate=DateTime.Today.AddMonths(1);
            var rand = new Random((int)DateTime.Now.Ticks).Next(0x1000000);
            EventSessions = new HashSet<EventSession>();
            SalesGroups = new HashSet<SalesGroup>();
            //Users = new HashSet<User>();
            EventCompanies = new HashSet<EventCompany>();
            //Trainers = new HashSet<Trainer>();
            BackgroundColor = string.Format("#{0:X6}", rand);
        }
        public string EventCode { get; set; }
        public string EventName { get; set; }
        public string BackgroundColor { get; set; }
        public string VenueOfEvent { get; set; }//venue of event
        public string SalesRules { get; set; }//Sales Rules
        public string Sectors { get; set; }//our sector
        public string CallFilterKeywords { get; set; }
        public string Remark { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateOfConfirmation { get; set; }
        [DataType(DataType.Date)]
        public DateTime ClosingDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }//Sales start
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }//Sales end
        public bool IsEventEnd { get; set; }
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        public virtual ICollection<EventSession> EventSessions { get; set; }
        //for assign sales group
        public virtual ICollection<SalesGroup> SalesGroups { get; set; }
        //for assign salesman
        public virtual ICollection<User> ManagerUsers { get; set; }
        public virtual ICollection<EventCompany> EventCompanies { get; set; }
        //public virtual ICollection<Trainer> Trainers { get; set; }
        [NotMapped]
        public IEnumerable<UserNotification> Notifications { get; set; }
    }
}
