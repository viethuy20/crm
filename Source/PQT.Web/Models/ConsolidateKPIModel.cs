﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NS.Helpers;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

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
        public void Prepare(IEnumerable<Lead> leads, IEnumerable<LeadNew> leadNews, IEnumerable<Booking> bookings)
        {
            ConsolidateKpis = new List<ConsolidateKPI>();
            var users = leads.DistinctBy(m => m.UserID).Select(m => m.User).ToList();
            users.AddRange(leadNews.DistinctBy(m => m.UserID).Select(m => m.User));
            foreach (var user in users.DistinctBy(m => m.ID))
            {
                var item = new ConsolidateKPI
                {
                    User = user
                };
                item.Prepare(leads.Where(m => m.User.TransferUserID == user.ID || m.UserID == user.ID),
                    leadNews.Where(m => m.User.TransferUserID == user.ID || m.UserID == user.ID),
                    bookings.Where(m => m.Salesman.TransferUserID == user.ID || m.SalesmanID == user.ID));
                ConsolidateKpis.Add(item);
            }
        }
        public void Prepare(IEnumerable<Booking> bookings)
        {
            ConsolidateKpis = new List<ConsolidateKPI>();
            var users = bookings.DistinctBy(m => m.SalesmanID).Select(m => m.Salesman).ToList();
            foreach (var user in users.DistinctBy(m => m.ID))
            {
                var item = new ConsolidateKPI
                {
                    User = user
                };
                item.Prepare(bookings.Where(m => m.Salesman.TransferUserID == user.ID || m.SalesmanID == user.ID));
                ConsolidateKpis.Add(item);
            }
        }

    }
    public class HRConsolidateKPIModel
    {
        public List<HRConsolidateKPI> HrConsolidateKpis { get; set; }
        public string Date { get; set; }

        public HRConsolidateKPIModel()
        {
            Date = "All";
        }

        public void Prepare(IEnumerable<Candidate> leads)
        {
            HrConsolidateKpis = new List<HRConsolidateKPI>();
            var users = leads.DistinctBy(m => m.UserID).Select(m => m.User);
            foreach (var user in users)
            {
                var item = new HRConsolidateKPI
                {
                    User = user
                };
                item.Prepare(leads.Where(m => m.User.TransferUserID == user.ID || m.UserID == user.ID));
                HrConsolidateKpis.Add(item);
            }
        }
    }
    public class ConsolidateKPI
    {
        public User User { get; set; }
        public int NewEventRequest { get; set; }
        public decimal WrittenRevenue { get; set; }
        public decimal WrittenRevenue1 { get; set; }
        public decimal WrittenRevenue2 { get; set; }
        public decimal WrittenRevenue3 { get; set; }
        public decimal TotalWrittenRevenue { get; set; }
        public int KPI { get; set; }
        public int NoKPI { get; set; }
        public int NoCheck { get; set; }
        public void Prepare(IEnumerable<Lead> leads, IEnumerable<LeadNew> leadNews, IEnumerable<Booking> bookings)
        {
            NewEventRequest = leadNews.Count();
            WrittenRevenue = bookings.Sum(m => m.TotalWrittenRevenue);
            KPI = leads.Count(m => m.MarkKPI);
            NoKPI = leads.Count(m => !m.MarkKPI && !string.IsNullOrEmpty(m.FileNameImportKPI));
            NoCheck = leads.Count(m => string.IsNullOrEmpty(m.FileNameImportKPI));
        }
        public void Prepare(IEnumerable<Booking> bookings)
        {
            WrittenRevenue1 = bookings.Where(m => m.CreatedTime.Month >= DateTime.Today.AddMonths(-2).Month).Sum(m => m.TotalWrittenRevenue);
            WrittenRevenue2 = bookings.Where(m => m.CreatedTime.Month >= DateTime.Today.AddMonths(-1).Month).Sum(m => m.TotalWrittenRevenue);
            WrittenRevenue3 = bookings.Where(m => m.CreatedTime.Month >= DateTime.Today.Month).Sum(m => m.TotalWrittenRevenue);
            TotalWrittenRevenue = WrittenRevenue1 + WrittenRevenue2 + WrittenRevenue3;
        }
    }
    public class HRConsolidateKPI
    {
        public User User { get; set; }
        public int RecruitmentCallKPIs { get; set; }
        public int EmployeeKPIs { get; set; }
        public void Prepare(IEnumerable<Candidate> leads)
        {
            RecruitmentCallKPIs = leads.Count();
            EmployeeKPIs = leads.Count(m => m.CandidateStatusRecord == CandidateStatus.ApprovedEmployment);
        }
    }
}