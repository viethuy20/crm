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
                var leads = leadService.GetAllLeadsOfUserForMerge(userId);
                var count = 0;
                foreach (var lead in leads)
                {
                    count++;
                    var existResource =
                        comService.GetAllCompanyResourcesCheckPhoneForMerge(lead.JobTitle, lead.DirectLine, lead.MobilePhone1, lead.MobilePhone2, lead.MobilePhone3);
                    //var eventCompany = _repo.GetEventCompany(lead.EventID, lead.CompanyID);
                    if (existResource != null)
                    {
                        existResource.CompanyID = lead.CompanyID;
                        existResource.CountryID = lead.Company.CountryID;
                        existResource.Country = lead.Company.CountryCode;
                        existResource.FirstName = lead.FirstName;
                        existResource.LastName = lead.LastName;
                        existResource.Organisation = lead.CompanyName;
                        existResource.PersonalEmail = lead.PersonalEmail;
                        existResource.Role = lead.JobTitle;
                        existResource.Salutation = lead.Salutation;
                        existResource.WorkEmail = lead.WorkEmail;
                        existResource.DirectLine = lead.DirectLine;
                        existResource.MobilePhone1 = lead.MobilePhone1;
                        existResource.MobilePhone2 = lead.MobilePhone2;
                        existResource.MobilePhone3 = lead.MobilePhone3;
                        comService.UpdateCompanyResource(existResource);
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


        public static void MakeExpiredLead()
        {
            new Thread(() =>
            {
                var leadService = DependencyHelper.GetService<ILeadService>();
                var auditService = DependencyHelper.GetService<IAuditTracker>();
                var leads = leadService.GetAllLeadsForMakeExpiredLead();
                foreach (var lead in leads)
                {
                    lead.ExpiredForReopen = true;
                    leadService.UpdateLead(lead);
                }
                var record = new Audit
                {
                    Username = "",
                    Email = "",
                    IPAddress = "",
                    UrlAccessed = "",
                    TimeAccessed = DateTime.Now,
                    SessionId = "",
                    Data = "Count Leads:" + leads.Count(),
                    Message = "Make Expired Lead",
                    Type = (int)AuditType.Auto,
                    ActionId = 0
                };
                auditService.CreateRecord(record);

            }).Start();
        }
    }
}