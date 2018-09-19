using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using NS;
using PQT.Domain;
using PQT.Domain.Abstract;
using PQT.Domain.Concrete;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Infrastructure.Utility;
using Resources;

namespace PQT.Web.Models
{
    public class LeadModel
    {
        public int id { get; set; }
        public string hubConnectionId { get; set; }
        public string requestType { get; set; }
        public HttpPostedFileBase AttachmentFile { get; set; }
        public string Reason { get; set; }
        public Lead Lead { get; set; }
        public LeadNew LeadNew { get; set; }

        public LeadModel()
        {
        }
        public LeadModel(int leadId)
        {
            id = leadId;
        }

        public LeadModel(int leadId, string type)
        {
            id = leadId;
            requestType = type;
        }

        public string DeleteLead()
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var lead = leadRepo.GetLead(id);
            if (lead.LeadStatusRecord == LeadStatus.Deleted)
                return "Cannot process ... Call status: " + lead.StatusDisplay;
            lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.Deleted, CurrentUser.Identity.ID, "delete call");
            return leadRepo.UpdateLead(lead) ? "" : "Delete failed";
        }
        public string BlockLead()
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var lead = leadRepo.GetLead(id);
            if (lead == null) return "Block failed";
            if (lead.LeadStatusRecord == LeadStatus.Booked)
                return "Cannot process ... This item has been booked";
            var leads = leadRepo.GetAllLeads(m => m.Event.EventStatus == EventStatus.Live || m.Event.EventStatus == EventStatus.Confirmed);
            var maxBlock = Settings.Lead.MaxBlockeds();
            var currentUser = CurrentUser.Identity;
            if (leads.Count(m => (m.User.TransferUserID == currentUser.ID || m.UserID == currentUser.ID) && m.LeadStatusRecord == LeadStatus.Blocked) >= maxBlock)
                return "Limit blocked is not exceed " + maxBlock;
            var daysExpired = Settings.Lead.NumberDaysExpired();
            if (leads.Any(m => m.User.TransferUserID != currentUser.ID && m.UserID != currentUser.ID &&
                               m.CompanyID == lead.CompanyID &&
                               m.LeadStatusRecord != LeadStatus.Initial && m.LeadStatusRecord != LeadStatus.Reject && !m.CheckNCLExpired(daysExpired)))
                return "Cannot block this company... Company is requesting to NCL or exists in NCL";

            if (lead.LeadStatusRecord == LeadStatus.Blocked) return "Block failed";
            lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.Blocked, currentUser.ID);
            if (!leadRepo.UpdateLead(lead)) return "Block failed";
            //var users = new List<User>();
            //users.AddRange(lead.Event.SalesGroups.SelectMany(m => m.Users));
            //users.AddRange(lead.Event.SalesGroups.SelectMany(m => m.Users));
            //users.Add(lead.Event.User);//notify for manager
            //LeadNotificator.NotifyUser(users, lead);
            LeadNotificator.NotifyUpdateNCL(lead.ID);
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
            if (!leadRepo.UpdateLead(lead)) return "Unblock failed";
            LeadNotificator.NotifyUpdateNCL(lead.ID);
            return "";
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
                if (AttachmentFile == null) return "Please check your input";
                string uploadPicture = FileUpload.Upload(FileUploadType.Lead, AttachmentFile);
                if (string.IsNullOrEmpty(uploadPicture)) return "Please check your input";
                lead.LeadStatusRecord.Attachment = uploadPicture;
                return leadRepo.UpdateAttachment(lead.LeadStatusRecord) ? "" : "Update failed";
            }

            //if (lead.LeadStatusRecord != LeadStatus.Initial && lead.LeadStatusRecord != LeadStatus.Reject) return "Submit failed";

            var daysExpired = Settings.Lead.NumberDaysExpired();
            var leads = leadRepo.GetAllLeads(m => m.EventID == lead.EventID);
            var currentUser = CurrentUser.Identity;
            if (leads.Any(m => m.User.TransferUserID != currentUser.ID && m.UserID != currentUser.ID &&
                               m.CompanyID == lead.CompanyID &&
                               m.LeadStatusRecord != LeadStatus.Initial && m.LeadStatusRecord != LeadStatus.Reject
                && !m.CheckNCLExpired(daysExpired)))
                return "Cannot block this company... Company is requesting to NCL or exists in NCL by another";

            string fileName = null;
            if (AttachmentFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Lead, AttachmentFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    fileName = uploadPicture;
                }
            }
            lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, Enumeration.FromValue<LeadStatus>(requestType), currentUser.ID, fileName, "");
            if (!leadRepo.UpdateLead(lead)) return "Submit failed";

            var membershipService = DependencyHelper.GetService<IMembershipService>();
            var notiUsers = membershipService.GetUsersInRole(new string[] { "Manager", "QC" });
            LeadNotificator.NotifyUser(notiUsers, lead.ID); // notify for manager
            LeadNotificator.NotifyUpdateNCL(lead.ID);
            return "";
        }

        public string CancelRequest()
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var bookingRepo = DependencyHelper.GetService<IBookingService>();
            var lead = leadRepo.GetLead(id);
            if (lead == null) return "Cancel failed";
            if (lead.LeadStatusRecord == LeadStatus.Booked)
                return "Cannot process ... This item has been booked";
            if (lead.LeadStatusRecord != LeadStatus.RequestLOI &&
                lead.LeadStatusRecord != LeadStatus.RequestNCL &&
                lead.LeadStatusRecord != LeadStatus.RequestBook)
                return "Cancel failed";
            if (lead.LeadStatusRecord == LeadStatus.RequestBook)
            {
                var booking = bookingRepo.GetBooking(id);
                if (booking != null)
                {
                    if (booking.BookingStatusRecord == BookingStatus.Approved)
                    {
                        return "Cannot process ... Booking has been approved";
                    }
                    bookingRepo.DeleteBooking(booking.ID);
                }
            }
            var currentUser = CurrentUser.Identity;
            var titleNotify = lead.LeadStatusRecord.Status.DisplayName + " cancelled";
            lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.Initial, currentUser.ID);
            if (!leadRepo.UpdateLead(lead))
                return "Cancel failed";

            var membershipService = DependencyHelper.GetService<IMembershipService>();
            var notiUsers = membershipService.GetUsersInRole(new string[] { "Manager", "QC" });
            LeadNotificator.NotifyUser(notiUsers, lead.ID, titleNotify); // notify for manager
            LeadNotificator.NotifyUpdateNCL(lead.ID);
            return "";
        }
        public object ApprovalRequest()
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var lead = leadRepo.GetLead(id);
            if (lead == null)
                return new
                {
                    Message = "Approval failed",
                    IsSuccess = false
                };
            if (lead.LeadStatusRecord == LeadStatus.Booked)
                return new
                {
                    Message = "Cannot process ... This item has been booked",
                    IsSuccess = false
                };
            if (lead.LeadStatusRecord != LeadStatus.RequestLOI &&
                lead.LeadStatusRecord != LeadStatus.RequestNCL &&
                lead.LeadStatusRecord != LeadStatus.RequestBook)
                return new
                {
                    Message = "Approval failed",
                    IsSuccess = false
                };

            var leads = leadRepo.GetAllLeads(m => m.EventID == lead.EventID);
            var daysExpired = Settings.Lead.NumberDaysExpired();
            if (leads.Any(m => m.UserID != lead.UserID && m.User.TransferUserID != lead.UserID &&
                               m.CompanyID == lead.CompanyID &&
                               m.LeadStatusRecord != LeadStatus.Initial && m.LeadStatusRecord != LeadStatus.Reject
                               && !m.CheckNCLExpired(daysExpired)))
                return new
                {
                    Message = "Cannot approve this company... Company is requesting to NCL or exists in NCL",
                    IsSuccess = false
                };

            var currentUserId = CurrentUser.Identity.ID;
            var titleNotify = lead.LeadStatusRecord.Status.DisplayName + " approved";
            if (lead.LeadStatusRecord == LeadStatus.RequestNCL)
            {
                lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.Live, currentUserId);
            }
            else if (lead.LeadStatusRecord == LeadStatus.RequestLOI)
            {
                lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.LOI, currentUserId);
            }
            else if (lead.LeadStatusRecord == LeadStatus.RequestBook)
            {
                lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.Booked, currentUserId);
            }
            if (!leadRepo.UpdateLead(lead))
                return new
                {
                    Message = "Approval failed",
                    IsSuccess = false
                };
            var membershipService = DependencyHelper.GetService<IMembershipService>();
            LeadNotificator.NotifyUser(new List<User> { lead.User.TransferUserID > 0 ? membershipService.GetUser((int)lead.User.TransferUserID) : lead.User }, lead.ID, titleNotify); // notify for manager
            LeadNotificator.NotifyUpdateNCL(lead.ID, hubConnectionId);
            return new
            {
                Data = lead.SerializingFull(daysExpired),
                Message = "",
                IsSuccess = true
            };
        }
        public object RejectRequest()
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var lead = leadRepo.GetLead(id);
            if (lead == null)
                return new
                {
                    Message = "Reject failed",
                    IsSuccess = false
                };
            if (lead.LeadStatusRecord == LeadStatus.Booked)
                return new
                {
                    Message = "Cannot process ... This item has been booked",
                    IsSuccess = false
                };
            if (lead.LeadStatusRecord != LeadStatus.RequestLOI &&
                lead.LeadStatusRecord != LeadStatus.RequestNCL &&
                lead.LeadStatusRecord != LeadStatus.RequestBook)
                return new
                {
                    Message = "Reject failed",
                    IsSuccess = false
                };

            var currentUserId = CurrentUser.Identity.ID;
            var titleNotify = lead.LeadStatusRecord.Status.DisplayName + " rejected";
            lead.LeadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.Reject, currentUserId, Reason);
            if (!leadRepo.UpdateLead(lead))
                return new
                {
                    Message = "Reject failed",
                    IsSuccess = false
                };
            var membershipService = DependencyHelper.GetService<IMembershipService>();
            LeadNotificator.NotifyUser(new List<User> { lead.User.TransferUserID > 0 ? membershipService.GetUser((int)lead.User.TransferUserID) : lead.User }, lead.ID, titleNotify); // notify for manager
            LeadNotificator.NotifyUpdateNCL(lead.ID, hubConnectionId);

            var daysExpired = Settings.Lead.NumberDaysExpired();
            return new
            {
                Data = lead.SerializingFull(daysExpired),
                Message = "",
                IsSuccess = true
            };
        }


        public string RequestBrochure()
        {
            var leadRepo = DependencyHelper.GetService<ILeadNewService>();
            var lead = leadRepo.GetLeadNew(id);
            if (lead == null) return "Submit failed";

            if (AttachmentFile == null) return "Please attach proof of Topic Probing Questions";
            string uploadPicture = FileUpload.Upload(FileUploadType.LeadNew, AttachmentFile);
            if (string.IsNullOrEmpty(uploadPicture)) return "Please attach proof of Topic Probing Questions";
            lead.RequestBrochure = uploadPicture;
            if(!leadRepo.UpdateLeadNew(lead))
                return "Update failed";

            var membershipService = DependencyHelper.GetService<IMembershipService>();
            var notiUsers = membershipService.GetUsersInRole(new string[] { "Manager"});
            NewEventNotificator.NotifyUser(notiUsers, lead.ID, "Request Brochure"); // notify for manager
            return "";
        }

        public string DeleteLeadNew()
        {
            var leadRepo = DependencyHelper.GetService<ILeadNewService>();
            var lead = leadRepo.GetLeadNew(id);
            if (lead.AssignUserID > 0)
                return "Cannot process ... Event has been assigned to sales";
            return leadRepo.DeleteLeadNew(id) ? "" : "Delete failed";
        }

    }
    public class CallingModel
    {
        public int LeadID { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        public string JobTitle { get; set; }//Job Title
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string DirectLine { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        public string CompanyName { get; set; }
        public string BusinessUnit { get; set; }

        public string LineExtension { get; set; }
        public int EventID { get; set; }
        public Event Event { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        public int? CompanyID { get; set; }
        public IEnumerable<Company> Companies { get; set; }

        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string MobilePhone1 { get; set; }
        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string MobilePhone2 { get; set; }
        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string MobilePhone3 { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        [RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessage = "Email is invalid")]
        public string WorkEmail { get; set; }
        [RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessage = "Email is invalid")]
        public string WorkEmail1 { get; set; }
        [RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessage = "Email is invalid")]
        public string PersonalEmail { get; set; }

        public int? EstimatedDelegateNumber { get; set; }
        public decimal? TrainingBudgetPerHead { get; set; }//USD
        public int GoodTrainingMonth { get; set; }
        public FollowUpStatus FirstFollowUpStatus { get; set; }
        public FinalStatus FinalStatus { get; set; }
        public string NewTopics { get; set; }
        public string NewLocations { get; set; }
        public DateTime? NewDateFrom { get; set; }
        public DateTime? NewDateTo { get; set; }
        public string NewTrainingType { get; set; }
        public string GoodTrainingMonthStr
        {
            get
            {
                var monthEnum = Enumeration.FromValue<MonthStatus>(GoodTrainingMonth.ToString());
                return monthEnum != null ? monthEnum.ToString() : "";
            }
        }
        public string DialingCode { get; set; }
        public string TypeSubmit { get; set; }
        public PhoneCall PhoneCall { get; set; }
        public Lead Lead { get; set; }
        public LeadNew LeadNew { get; set; }
        public EventCompany EventCompany { get; set; }


        public int AssignSales { get; set; }
        public IEnumerable<User> Sales { get; set; }
        public CallingModel()
        {
            PhoneCall = new PhoneCall();
            EventCompany = new EventCompany();
            FirstFollowUpStatus = FollowUpStatus.Neutral;
            FinalStatus = FinalStatus.Neutral;
        }
        public void PrepareCall(int eventId, int resourceId)
        {
            EventID = eventId;
            FirstFollowUpStatus = FollowUpStatus.Neutral;
            FinalStatus = FinalStatus.Neutral;
            PhoneCall = new PhoneCall();
            var evetnRepo = DependencyHelper.GetService<IEventService>();
            Event = evetnRepo.GetEvent(eventId);
            if (resourceId > 0)
            {
                var comRepo = DependencyHelper.GetService<ICompanyRepository>();
                var unitRepo = DependencyHelper.GetService<IUnitRepository>();
                var resource = comRepo.GetCompanyResource(resourceId);
                if (resource != null)
                {
                    JobTitle = resource.Role;
                    CompanyName = resource.Organisation;
                    //BusinessUnit = resource.BusinessUnit;
                    Salutation = resource.Salutation;
                    FirstName = resource.FirstName;
                    LastName = resource.LastName;
                    MobilePhone1 = resource.MobilePhone1;
                    MobilePhone2 = resource.MobilePhone2;
                    MobilePhone3 = resource.MobilePhone3;
                    WorkEmail = resource.WorkEmail;
                    PersonalEmail = resource.PersonalEmail;
                    CompanyID = resource.CompanyID;
                    if (resource.CountryID > 0)
                    {
                        var country = unitRepo.GetCountry((int)resource.CountryID);
                        if (country != null)
                        {
                            DialingCode = country.DialingCode;
                        }
                        var com = comRepo.GetCompany((int)resource.CompanyID);
                        if (com != null)
                        {
                            BusinessUnit = com.DialingCode;
                        }
                    }
                    var comp = comRepo.GetCompany(Convert.ToInt32(resource.CompanyID));
                    if (comp != null)
                    {
                        EventCompany = new EventCompany
                        {
                            EventID = eventId,
                            BusinessUnit = comp.BusinessUnit,
                            BudgetMonth = comp.BudgetMonth,
                            Remarks = comp.Remarks,
                            CompanyID = Convert.ToInt32(resource.CompanyID)
                        };
                    }
                }
            }
        }
        public void PrepareCalling(int leadId)
        {

            FirstFollowUpStatus = FollowUpStatus.Neutral;
            FinalStatus = FinalStatus.Neutral;
            LeadID = leadId;
            if (leadId > 0)
            {
                var leadRepo = DependencyHelper.GetService<ILeadService>();
                var lead = leadRepo.GetLead(leadId);
                PhoneCall = new PhoneCall { LeadID = leadId };
                if (lead != null)
                {
                    LeadID = leadId;
                    EventID = lead.EventID;
                    Event = lead.Event;
                    //GeneralLine = lead.GeneralLine;
                    JobTitle = lead.JobTitle;
                    LineExtension = lead.LineExtension;
                    DirectLine = lead.DirectLine;
                    Salutation = lead.Salutation;
                    FirstName = lead.FirstName;
                    LastName = lead.LastName;
                    MobilePhone1 = lead.MobilePhone1;
                    MobilePhone2 = lead.MobilePhone2;
                    MobilePhone3 = lead.MobilePhone3;
                    WorkEmail = lead.WorkEmail;
                    WorkEmail1 = lead.WorkEmail1;
                    PersonalEmail = lead.PersonalEmail;
                    EstimatedDelegateNumber = lead.EstimatedDelegateNumber;
                    TrainingBudgetPerHead = lead.TrainingBudgetPerHead;
                    GoodTrainingMonth = lead.GoodTrainingMonth;
                    //NewTopics = lead.NewTopics;
                    //NewLocations = lead.NewLocations;
                    //NewDateFrom = lead.NewDateFrom;
                    //NewDateTo = lead.NewDateTo;
                    //NewTrainingType = lead.NewTrainingType;
                    FinalStatus = lead.FinalStatus;
                    FirstFollowUpStatus = lead.FirstFollowUpStatus;
                    CompanyName = lead.CompanyName;
                    DialingCode = lead.Company.DialingCode;
                    BusinessUnit = lead.Company.BusinessUnit;
                    Lead = lead;
                    //var eventCom = eventRepo.GetEventCompany(Lead.EventID, Lead.CompanyID);
                    //EventCompany = eventCom;
                }
            }
        }
        //public void LoadCompanies(int eventId)
        //{
        //    var eventRepo = DependencyHelper.GetService<IEventService>();
        //    var eventLead = eventRepo.GetEvent(eventId);
        //    if (eventLead != null)
        //    {
        //        var leadRepo = DependencyHelper.GetService<ILeadService>();
        //        var companiesInNcl = leadRepo.GetAllLeads(m => m.EventID == eventId).Where(m =>
        //            m.UserID != CurrentUser.Identity.ID &&
        //            m.LeadStatusRecord != LeadStatus.Initial && m.LeadStatusRecord != LeadStatus.Reject &&
        //            (m.LeadStatusRecord == LeadStatus.Blocked || m.LeadStatusRecord == LeadStatus.Booked ||
        //             m.LeadStatusRecord.UpdatedTime.Date >=
        //             DateTime.Today.AddDays(-Settings.Lead.NumberDaysExpired()))).Select(m => m.CompanyID).Distinct();// get list company blocked
        //        Companies = eventLead.EventCompanies.Where(m => !companiesInNcl.Contains(m.CompanyID)).Select(m => m.Company);
        //    }
        //    else
        //    {
        //        Companies = new List<Company>();
        //    }
        //}
        public bool SaveEdit()
        {
            return TransactionWrapper.Do(() =>
            {
                var leadRepo = DependencyHelper.GetService<ILeadService>();
                Lead = leadRepo.GetLead(LeadID);
                if (Lead != null)
                {

                    //GeneralLine = lead.GeneralLine;
                    Lead.JobTitle = JobTitle;
                    Lead.LineExtension = LineExtension;
                    Lead.DirectLine = DirectLine;
                    Lead.Salutation = Salutation;
                    Lead.FirstName = FirstName;
                    Lead.LastName = LastName;
                    Lead.MobilePhone1 = MobilePhone1;
                    Lead.MobilePhone2 = MobilePhone2;
                    Lead.MobilePhone3 = MobilePhone3;
                    Lead.WorkEmail = WorkEmail;
                    Lead.WorkEmail1 = WorkEmail1;
                    Lead.PersonalEmail = PersonalEmail;
                    Lead.EstimatedDelegateNumber = EstimatedDelegateNumber;
                    Lead.TrainingBudgetPerHead = TrainingBudgetPerHead;
                    Lead.GoodTrainingMonth = GoodTrainingMonth;
                    //Lead.NewLocations = NewLocations;
                    //Lead.NewTopics = NewTopics;
                    //Lead.NewDateFrom = NewDateFrom;
                    //Lead.NewDateTo = NewDateTo;
                    //Lead.NewTrainingType = NewTrainingType;
                    Lead.FinalStatus = FinalStatus;
                    Lead.FirstFollowUpStatus = FirstFollowUpStatus;
                    leadRepo.UpdateLead(Lead);
                    return true;
                }
                return false;
            });
        }
        public bool Create()
        {
            return TransactionWrapper.Do(() =>
            {
                var leadRepo = DependencyHelper.GetService<ILeadService>();
                var eventRepo = DependencyHelper.GetService<IEventService>();
                var comRepo = DependencyHelper.GetService<ICompanyRepository>();
                Lead = new Lead
                {
                    EventID = EventID,
                    CompanyID = (int)CompanyID,
                    //GeneralLine = GeneralLine;
                    JobTitle = JobTitle,
                    LineExtension = LineExtension,
                    DirectLine = DirectLine,
                    Salutation = Salutation,
                    FirstName = FirstName,
                    LastName = LastName,
                    MobilePhone1 = MobilePhone1,
                    MobilePhone2 = MobilePhone2,
                    MobilePhone3 = MobilePhone3,
                    WorkEmail = WorkEmail,
                    WorkEmail1 = WorkEmail1,
                    PersonalEmail = PersonalEmail,
                    EstimatedDelegateNumber = EstimatedDelegateNumber,
                    TrainingBudgetPerHead = TrainingBudgetPerHead,
                    GoodTrainingMonth = GoodTrainingMonth,
                    //NewLocations = NewLocations,
                    //NewTopics = NewTopics,
                    //NewDateFrom = NewDateFrom,
                    //NewDateTo = NewDateTo,
                    //NewTrainingType = NewTrainingType,
                    FinalStatus = FinalStatus,
                    FirstFollowUpStatus = FirstFollowUpStatus,
                    UserID = CurrentUser.Identity.ID
                };
                Lead = leadRepo.CreateLead(Lead);
                var company = Lead.Company;
                if (Lead != null)
                {
                    Lead.LeadStatusRecord = new LeadStatusRecord(Lead.ID, LeadStatus.Initial, CurrentUser.Identity.ID);
                    leadRepo.UpdateLead(Lead);
                    //LeadNotificator.NotifyUser(result.Event.Users, result);
                    //LeadNotificator.NotifyUser(result.Event.SalesGroups.SelectMany(m => m.Users), result);
                    PhoneCall.EndTime = DateTime.Now;
                    PhoneCall.LeadID = Lead.ID;
                    var result = leadRepo.CreatePhoneCall(PhoneCall);


                    if (company != null)
                    {
                        company.BusinessUnit = EventCompany.BusinessUnit;
                        company.BudgetMonth = EventCompany.BudgetMonth;
                        company.Remarks = EventCompany.Remarks;
                        comRepo.UpdateCompany(company);
                    }
                    if (result != null)
                    {
                        Lead.Event = eventRepo.GetEvent(Lead.EventID);
                        return true;
                    }
                    Lead.Event = eventRepo.GetEvent(Lead.EventID);
                }
                return false;
            });
        }

        public bool Save()
        {
            return TransactionWrapper.Do(() =>
            {
                var leadRepo = DependencyHelper.GetService<ILeadService>();
                var comRepo = DependencyHelper.GetService<ICompanyRepository>();
                PhoneCall.EndTime = DateTime.Now;
                var result = leadRepo.CreatePhoneCall(PhoneCall);
                if (result != null)
                {
                    Lead = leadRepo.GetLead(PhoneCall.LeadID);
                    //GeneralLine = lead.GeneralLine;
                    Lead.JobTitle = JobTitle;
                    Lead.LineExtension = LineExtension;
                    Lead.DirectLine = DirectLine;
                    Lead.Salutation = Salutation;
                    Lead.FirstName = FirstName;
                    Lead.LastName = LastName;
                    Lead.MobilePhone1 = MobilePhone1;
                    Lead.MobilePhone2 = MobilePhone2;
                    Lead.MobilePhone3 = MobilePhone3;
                    Lead.WorkEmail = WorkEmail;
                    Lead.WorkEmail1 = WorkEmail1;
                    Lead.PersonalEmail = PersonalEmail;
                    Lead.EstimatedDelegateNumber = EstimatedDelegateNumber;
                    Lead.TrainingBudgetPerHead = TrainingBudgetPerHead;
                    Lead.GoodTrainingMonth = GoodTrainingMonth;
                    //Lead.NewTopics = NewTopics;
                    //Lead.NewLocations = NewLocations;
                    //Lead.NewDateFrom = NewDateFrom;
                    //Lead.NewDateTo = NewDateTo;
                    //Lead.NewTrainingType = NewTrainingType;
                    Lead.FirstFollowUpStatus = FirstFollowUpStatus;
                    Lead.FinalStatus = FinalStatus;
                    leadRepo.UpdateLead(Lead);
                    if (Lead.Company != null)
                    {
                        Lead.Company.BusinessUnit = EventCompany.BusinessUnit;
                        Lead.Company.BudgetMonth = EventCompany.BudgetMonth;
                        Lead.Company.Remarks = EventCompany.Remarks;
                        comRepo.UpdateCompany(Lead.Company);
                    }
                    return true;
                }
                return false;
            });
        }

        public void PrepareNewEvent(int leadId)
        {
            LeadID = leadId;
            if (leadId > 0)
            {
                var leadRepo = DependencyHelper.GetService<ILeadNewService>();
                var lead = leadRepo.GetLeadNew(leadId);
                if (lead != null)
                {
                    LeadID = leadId;
                    EventID = lead.EventID;
                    Event = lead.Event;
                    JobTitle = lead.JobTitle;
                    LineExtension = lead.LineExtension;
                    DirectLine = lead.DirectLine;
                    Salutation = lead.Salutation;
                    FirstName = lead.FirstName;
                    LastName = lead.LastName;
                    MobilePhone1 = lead.MobilePhone1;
                    MobilePhone2 = lead.MobilePhone2;
                    MobilePhone3 = lead.MobilePhone3;
                    WorkEmail = lead.WorkEmail;
                    WorkEmail1 = lead.WorkEmail1;
                    PersonalEmail = lead.PersonalEmail;
                    EstimatedDelegateNumber = lead.EstimatedDelegateNumber;
                    TrainingBudgetPerHead = lead.TrainingBudgetPerHead;
                    GoodTrainingMonth = lead.GoodTrainingMonth;
                    NewTopics = lead.NewTopics;
                    NewLocations = lead.NewLocations;
                    NewDateFrom = lead.NewDateFrom;
                    NewDateTo = lead.NewDateTo;
                    NewTrainingType = lead.NewTrainingType;
                    FinalStatus = lead.FinalStatus;
                    FirstFollowUpStatus = lead.FirstFollowUpStatus;
                    CompanyName = lead.CompanyName;
                    DialingCode = lead.Company.DialingCode;
                    BusinessUnit = lead.Company.BusinessUnit;
                    LeadNew = lead;
                }
            }
        }

        public bool CreateNewEvent()
        {
            return TransactionWrapper.Do(() =>
            {
                var leadRepo = DependencyHelper.GetService<ILeadNewService>();
                var leadNew = new LeadNew
                {
                    EventID = EventID,
                    CompanyID = (int)CompanyID,
                    JobTitle = JobTitle,
                    LineExtension = LineExtension,
                    DirectLine = DirectLine,
                    Salutation = Salutation,
                    FirstName = FirstName,
                    LastName = LastName,
                    MobilePhone1 = MobilePhone1,
                    MobilePhone2 = MobilePhone2,
                    MobilePhone3 = MobilePhone3,
                    WorkEmail = WorkEmail,
                    WorkEmail1 = WorkEmail1,
                    PersonalEmail = PersonalEmail,
                    EstimatedDelegateNumber = EstimatedDelegateNumber,
                    TrainingBudgetPerHead = TrainingBudgetPerHead,
                    GoodTrainingMonth = GoodTrainingMonth,
                    NewLocations = NewLocations,
                    NewTopics = NewTopics,
                    NewDateFrom = NewDateFrom,
                    NewDateTo = NewDateTo,
                    NewTrainingType = NewTrainingType,
                    FinalStatus = FinalStatus,
                    FirstFollowUpStatus = FirstFollowUpStatus,
                    UserID = CurrentUser.Identity.ID
                };
                leadNew = leadRepo.CreateLeadNew(leadNew);
                if (leadNew != null)
                {
                    var membershipService = DependencyHelper.GetService<IMembershipService>();
                    var notiUsers = membershipService.GetUsersInRole(new string[] { "Manager" });
                    NewEventNotificator.NotifyUser(notiUsers, leadNew.ID, "Request New Event"); // notify for manager
                    return true;
                }
                return false;
            });
        }
        public bool SaveEditNewEvent()
        {
            return TransactionWrapper.Do(() =>
            {
                var leadRepo = DependencyHelper.GetService<ILeadNewService>();
                LeadNew = leadRepo.GetLeadNew(LeadID);
                if (LeadNew != null)
                {
                    LeadNew.JobTitle = JobTitle;
                    LeadNew.LineExtension = LineExtension;
                    LeadNew.DirectLine = DirectLine;
                    LeadNew.Salutation = Salutation;
                    LeadNew.FirstName = FirstName;
                    LeadNew.LastName = LastName;
                    LeadNew.MobilePhone1 = MobilePhone1;
                    LeadNew.MobilePhone2 = MobilePhone2;
                    LeadNew.MobilePhone3 = MobilePhone3;
                    LeadNew.WorkEmail = WorkEmail;
                    LeadNew.WorkEmail1 = WorkEmail1;
                    LeadNew.PersonalEmail = PersonalEmail;
                    LeadNew.EstimatedDelegateNumber = EstimatedDelegateNumber;
                    LeadNew.TrainingBudgetPerHead = TrainingBudgetPerHead;
                    LeadNew.GoodTrainingMonth = GoodTrainingMonth;
                    LeadNew.NewLocations = NewLocations;
                    LeadNew.NewTopics = NewTopics;
                    LeadNew.NewDateFrom = NewDateFrom;
                    LeadNew.NewDateTo = NewDateTo;
                    LeadNew.NewTrainingType = NewTrainingType;
                    LeadNew.FinalStatus = FinalStatus;
                    LeadNew.FirstFollowUpStatus = FirstFollowUpStatus;
                    leadRepo.UpdateLeadNew(LeadNew);
                    return true;
                }
                return false;
            });
        }

        public bool AssignSalesNewEvent()
        {
            return TransactionWrapper.Do(() =>
            {
                var leadRepo = DependencyHelper.GetService<ILeadNewService>();
                LeadNew = leadRepo.GetLeadNew(LeadID);
                if (LeadNew != null)
                {
                    LeadNew.AssignUserID = AssignSales;
                    if (leadRepo.UpdateLeadNew(LeadNew))
                    {
                        var membershipService = DependencyHelper.GetService<IMembershipService>();
                        var notiUser = membershipService.GetUser(AssignSales);
                        NewEventNotificator.NotifyUser(new List<User>{notiUser}, LeadID, "Assign New Event"); // notify for manager
                        return true;
                    }
                }
                return false;
            });
        }
    }
}