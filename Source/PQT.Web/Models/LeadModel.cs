using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PQT.Domain;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Models
{
    public class CallSheetModel
    {
        public Lead Lead { get; set; }
        public string TypeSubmit { get; set; }
        public IEnumerable<Company> Companies { get; set; }
        public CallSheetModel()
        {

        }
        public CallSheetModel(int eventId)
        {
            Lead = new Lead { EventID = eventId, UserID = CurrentUser.Identity.ID };
            var eventRepo = DependencyHelper.GetService<IEventService>();
            var eventLead = eventRepo.GetEvent(eventId);
            if (eventLead != null)
            {
                Companies = eventLead.Companies;
            }
            else
            {
                Companies = new List<Company>();
            }
        }

        public bool Save()
        {
            return TransactionWrapper.Do(() =>
            {
                var leadRepo = DependencyHelper.GetService<ILeadService>();
                var eventRepo = DependencyHelper.GetService<IEventService>();
                var comRepo = DependencyHelper.GetService<ICompanyRepository>();
                var result = leadRepo.CreateLead(Lead);
                if (result != null)
                {
                    result.LeadStatusRecord = new LeadStatusRecord(result.ID, LeadStatus.Initial, CurrentUser.Identity.ID);
                    leadRepo.UpdateLead(result);
                    result.Company = comRepo.GetCompany(result.CompanyID);
                    result.Event = eventRepo.GetEvent(result.EventID);
                    Lead = result;
                    LeadNotificator.NotifyAll(result);
                    return true;
                }
                return false;
            });
        }
    }
    public class CallingModel
    {
        public Lead Lead { get; set; }
        public string TypeSubmit { get; set; }
        public PhoneCall PhoneCall { get; set; }
        public CallingModel()
        {
            PhoneCall = new PhoneCall();
        }
        public CallingModel(int leadId)
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            Lead = leadRepo.GetLead(leadId);
            PhoneCall = new PhoneCall { LeadID = leadId };
        }

        public bool Save()
        {
            return TransactionWrapper.Do(() =>
            {
                var leadRepo = DependencyHelper.GetService<ILeadService>();
                PhoneCall.EndTime = DateTime.Now;
                var result = leadRepo.CreatePhoneCall(PhoneCall);
                if (result != null)
                {
                    return true;
                }
                return false;
            });
        }
    }
}