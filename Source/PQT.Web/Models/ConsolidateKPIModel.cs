using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NS.Helpers;
using PQT.Domain.Entities;

namespace PQT.Web.Models
{
    public class ConsolidateKPIModel
    {
        public List<ConsolidateKPI> ConsolidateKpis { get; set; }
        public string EventName { get; set; }
        public string Date { get; set; }

        public ConsolidateKPIModel()
        {
            EventName = "All";
            Date = "All";
        }
        public void Prepare(IEnumerable<Lead> leads, IEnumerable<LeadNew> leadNews)
        {
            ConsolidateKpis = new List<ConsolidateKPI>();
            var users = leads.DistinctBy(m => m.UserID).Select(m => m.User) ;
            foreach (var user in users)
            {
                var item = new ConsolidateKPI
                {
                    User = user
                };
                item.Prepare(leads.Where(m => m.User.TransferUserID == user.ID || m.UserID == user.ID), leadNews.Where(m => m.User.TransferUserID == user.ID || m.UserID == user.ID));
                ConsolidateKpis.Add(item);
            }
        }
    }
    public class ConsolidateKPI
    {
        public User User { get; set; }
        public int NewEventRequest { get; set; }
        public int KPI { get; set; }
        public int NoKPI { get; set; }
        public int NoCheck { get; set; }
        public void Prepare(IEnumerable<Lead> leads,IEnumerable<LeadNew> leadNews)
        {
            NewEventRequest = leadNews.Count();
            KPI = leads.Count(m => m.MarkKPI);
            NoKPI = leads.Count(m => !m.MarkKPI && !string.IsNullOrEmpty(m.FileNameImportKPI));
            NoCheck = leads.Count(m => string.IsNullOrEmpty(m.FileNameImportKPI));
        }
    }
}