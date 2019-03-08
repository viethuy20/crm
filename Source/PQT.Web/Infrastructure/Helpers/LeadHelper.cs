using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using NS.Mail;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Infrastructure.Helpers
{
    public static class LeadHelper
    {
        public static bool CheckPossibleBlock(this Lead lead)
        {
            if (lead.LeadStatusRecord == LeadStatus.Booked)
                return false;
            return true;
        }
        public static bool NCLExpired(this Lead lead, int daysExpired)
        {
            return lead.CheckNCLExpired(daysExpired);
        }

        public static void MakeMergeNoCallListToComResource(int userId)
        {
            new Thread(() =>
            {
                var leadService = DependencyHelper.GetService<ILeadService>();
                var comService = DependencyHelper.GetService<ICompanyRepository>();
                var auditService = DependencyHelper.GetService<IAuditTracker>();
                var record = new Audit
                {
                    Username = "",
                    Email = "",
                    IPAddress = "",
                    UrlAccessed = "UserId: " + userId,
                    TimeAccessed = DateTime.Now,
                    SessionId = "",
                    Message = "Start Auto Merge NCL",
                    Type = (int)AuditType.Auto,
                    ActionId = 0
                };
                auditService.CreateRecord(record);
                var leads = leadService.GetAllLeads(m =>
                    m.UserID == userId &&
                    m.Event.EventStatus != EventStatus.Completed);
                var count = 0;
                foreach (var lead in leads)
                {
                    count++;
                    var existResources =
                        comService.GetAllCompanyResources(
                            m =>
                                m.Role == lead.JobTitle && (
                                (!string.IsNullOrEmpty(m.DirectLine) && !string.IsNullOrEmpty(lead.DirectLine) &&
                                 m.DirectLine == lead.DirectLine) ||
                                (!string.IsNullOrEmpty(m.MobilePhone1) && !string.IsNullOrEmpty(lead.MobilePhone1) &&
                                 m.MobilePhone1 == lead.MobilePhone1) ||
                                (!string.IsNullOrEmpty(m.MobilePhone2) && !string.IsNullOrEmpty(lead.MobilePhone2) &&
                                 m.MobilePhone2 == lead.MobilePhone2) ||
                                (!string.IsNullOrEmpty(m.MobilePhone3) && !string.IsNullOrEmpty(lead.MobilePhone3) &&
                                 m.MobilePhone3 == lead.MobilePhone3))).ToList();
                    //var eventCompany = _repo.GetEventCompany(lead.EventID, lead.CompanyID);
                    if (existResources.Any())
                    {
                        foreach (var item in existResources)
                        {
                            item.CompanyID = lead.CompanyID;
                            item.CountryID = lead.Company.CountryID;
                            item.Country = lead.Company.CountryCode;
                            item.FirstName = lead.FirstName;
                            item.LastName = lead.LastName;
                            item.Organisation = lead.CompanyName;
                            item.PersonalEmail = lead.PersonalEmail;
                            item.Role = lead.JobTitle;
                            item.Salutation = lead.Salutation;
                            item.WorkEmail = lead.WorkEmail;
                            item.DirectLine = lead.DirectLine;
                            item.MobilePhone1 = lead.MobilePhone1;
                            item.MobilePhone2 = lead.MobilePhone2;
                            item.MobilePhone3 = lead.MobilePhone3;
                            comService.UpdateCompanyResource(item);
                        }
                    }
                    else
                    {
                        var item = new CompanyResource()
                        {
                            CompanyID = lead.CompanyID,
                            CountryID = lead.Company.CountryID,
                            Country = lead.Company.CountryCode,
                            FirstName = lead.FirstName,
                            LastName = lead.LastName,
                            DirectLine = lead.DirectLine,
                            MobilePhone1 = lead.MobilePhone1,
                            MobilePhone2 = lead.MobilePhone2,
                            MobilePhone3 = lead.MobilePhone3,
                            Organisation = lead.CompanyName,
                            PersonalEmail = lead.PersonalEmail,
                            Role = lead.JobTitle,
                            Salutation = lead.Salutation,
                            WorkEmail = lead.WorkEmail,
                            Auto = true
                        };
                        comService.CreateCompanyResource(item);
                    }
                }
                record = new Audit
                {
                    Username = "",
                    Email = "",
                    IPAddress = "",
                    UrlAccessed = "UserId: " + userId,
                    TimeAccessed = DateTime.Now,
                    SessionId = "",
                    Data = "Count Leads:" + count,
                    Message = "End Auto Merge NCL",
                    Type = (int)AuditType.Auto,
                    ActionId = 0
                };
                auditService.CreateRecord(record);

            }).Start();
        }
    }
}