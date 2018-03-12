using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class ProjectEvent : Entity
    {
        public ProjectEvent()
        {
            EventSessions = new HashSet<ProjectSession>();
            SalesGroups = new HashSet<SalesGroup>();
            Users = new HashSet<User>();
            Companies = new HashSet<Company>();
            //Trainers = new HashSet<Trainer>();
        }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Remark { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual ICollection<ProjectSession> EventSessions { get; set; }
        //for assign sales group
        public virtual ICollection<SalesGroup> SalesGroups { get; set; }
        //for assign salesman
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Company> Companies { get; set; }
        //public virtual ICollection<Trainer> Trainers { get; set; }
    }
}
