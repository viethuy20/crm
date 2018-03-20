using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NS;
using PQT.Domain;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Models
{
    public class LeadModel
    {
        public int id { get; set; }
        public string requestType { get; set; }
        public HttpPostedFileBase AttachmentFile { get; set; }
        public string Reason { get; set; }

        public LeadModel()
        {
        }

        public LeadModel(int leadId, string type)
        {
            id = leadId;
            requestType = type;
        }

        public string BlockLead()
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var lead = leadRepo.GetLead(id);
            if (lead == null) return "Block failed";
            if (lead.LeadStatusRecord == LeadStatus.Booked)
                return "Cannot process ... This item has been booked";
            var leads = leadRepo.GetAllLeads(m => m.EventID == lead.EventID);
            var maxBlock = Settings.Lead.MaxBlockeds();
            if (leads.Count(m => m.UserID == CurrentUser.Identity.ID && m.LeadStatusRecord == LeadStatus.Blocked) >= maxBlock)
                return "Limit blocked is not exceed " + maxBlock;
            if (leads.Any(m => m.UserID != CurrentUser.Identity.ID && m.CompanyID == lead.CompanyID && m.LeadStatusRecord != LeadStatus.Initial && m.LeadStatusRecord != LeadStatus.Reject))
                return "Cannot block this company... Company is requesting to NCL or exists in NCL";

            if (lead.LeadStatusRecord == LeadStatus.Blocked) return "Block failed";
            lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.Blocked, CurrentUser.Identity.ID);
            if (!leadRepo.UpdateLead(lead)) return "Block failed";
            var users = new List<User>();
            users.AddRange(lead.Event.Users);
            users.AddRange(lead.Event.SalesGroups.SelectMany(m => m.Users));
            //users.Add(lead.Event.User);//notify for manager
            LeadNotificator.NotifyUser(users, lead);
            return "";

        }

        public string UnblockLead()
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var lead = leadRepo.GetLead(id);
            if (lead == null) return "Unblock failed";
            if (lead.LeadStatusRecord != LeadStatus.Blocked) return "Unblock failed";
            if (lead.LeadStatusRecord == LeadStatus.Booked)
                return "Cannot process ... This item has been booked";
            lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.Initial, CurrentUser.Identity.ID,
                "action unblock");
            return leadRepo.UpdateLead(lead) ? "" : "Unblock failed";
        }

        public string RequestAction()
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var lead = leadRepo.GetLead(id);
            if (lead == null) return "Submit failed";
            if (lead.LeadStatusRecord == LeadStatus.Booked)
                return "Cannot process ... This item has been booked";
            if (string.IsNullOrEmpty(requestType))
            {
                if (AttachmentFile != null)
                {
                    string uploadPicture = FileUpload.Upload(FileUploadType.Lead, AttachmentFile);
                    if (!string.IsNullOrEmpty(uploadPicture))
                    {
                        lead.LeadStatusRecord.Attachment = uploadPicture;
                        return leadRepo.UpdateAttachment(lead.LeadStatusRecord) ? "" : "Update failed";
                    }
                }
                return "Please check your input";
            }

            if (lead.LeadStatusRecord != LeadStatus.Initial && lead.LeadStatusRecord != LeadStatus.Reject) return "Submit failed";
            string fileName = null;
            if (AttachmentFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Lead, AttachmentFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    fileName = uploadPicture;
                }
            }
            lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, Enumeration.FromValue<LeadStatus>(requestType), CurrentUser.Identity.ID, fileName);
            if (!leadRepo.UpdateLead(lead)) return "Submit failed";
            LeadNotificator.NotifyUser(new List<User> { lead.Event.User }, lead); // notify for manager
            return "";
        }

        public string CancelRequest()
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var lead = leadRepo.GetLead(id);
            if (lead == null) return "Cancel failed";
            if (lead.LeadStatusRecord == LeadStatus.Booked)
                return "Cannot process ... This item has been booked";
            if (lead.LeadStatusRecord != LeadStatus.RequestLOI &&
                lead.LeadStatusRecord != LeadStatus.RequestNCL &&
                lead.LeadStatusRecord != LeadStatus.RequestBook)
                return "Cancel failed";
            var titleNotify = lead.LeadStatusRecord.Status.DisplayName + " cancelled";
            lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.Initial, CurrentUser.Identity.ID);
            if (!leadRepo.UpdateLead(lead)) return "Cancel failed";
            LeadNotificator.NotifyUser(new List<User> { lead.Event.User }, lead, titleNotify); // notify for manager
            return "";
        }
        public string ApprovalRequest()
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var lead = leadRepo.GetLead(id);
            if (lead == null) return "Approval failed";
            if (lead.LeadStatusRecord == LeadStatus.Booked)
                return "Cannot process ... This item has been booked";
            if (lead.LeadStatusRecord != LeadStatus.RequestLOI &&
                lead.LeadStatusRecord != LeadStatus.RequestNCL &&
                lead.LeadStatusRecord != LeadStatus.RequestBook)
                return "Approval failed";
            var titleNotify = lead.LeadStatusRecord.Status.DisplayName + " approved";
            if (lead.LeadStatusRecord == LeadStatus.RequestNCL)
            {
                lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.Live, CurrentUser.Identity.ID);
            }
            else if (lead.LeadStatusRecord == LeadStatus.RequestLOI)
            {
                lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.LOI, CurrentUser.Identity.ID);
            }
            else if (lead.LeadStatusRecord == LeadStatus.RequestBook)
            {
                lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.Booked, CurrentUser.Identity.ID);
            }
            if (!leadRepo.UpdateLead(lead)) return "Approval failed";
            LeadNotificator.NotifyUser(new List<User> { lead.User }, lead, titleNotify); // notify for manager
            return "";
        }
        public string RejectRequest()
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var lead = leadRepo.GetLead(id);
            if (lead == null) return "Reject failed";
            if (lead.LeadStatusRecord == LeadStatus.Booked)
                return "Cannot process ... This item has been booked";
            if (lead.LeadStatusRecord != LeadStatus.RequestLOI &&
                lead.LeadStatusRecord != LeadStatus.RequestNCL &&
                lead.LeadStatusRecord != LeadStatus.RequestBook)
                return "Reject failed";
            var titleNotify = lead.LeadStatusRecord.Status.DisplayName + " rejected";
            lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.Initial, CurrentUser.Identity.ID);
            if (!leadRepo.UpdateLead(lead)) return "Reject failed";
            LeadNotificator.NotifyUser(new List<User> { lead.User }, lead, titleNotify); // notify for manager
            return "";
        }
    }
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
                    //LeadNotificator.NotifyUser(result.Event.Users, result);
                    //LeadNotificator.NotifyUser(result.Event.SalesGroups.SelectMany(m => m.Users), result);
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