using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using NS.Entity;
using PQT.Domain;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Infrastructure.Utility;
using Quartz;

namespace PQT.Web.Models
{
    public class EventModel
    {
        public int ID { get; set; }
        public Event Event { get; set; }
        public List<int> GroupsSelected { get; set; }
        public List<int> UsersSelected { get; set; }
        public List<int> CompaniesSelected { get; set; }
        public IEnumerable<SalesGroup> SalesGroups { get; set; }
        public IEnumerable<EventCategory> EventCategories { get; set; }
        public IEnumerable<User> Users { get; set; }
        public HttpPostedFileBase SalesBrochureFile { get; set; }
        public HttpPostedFileBase OperationBrochureFile { get; set; }
        public HttpPostedFileBase RegContractFile { get; set; }
        public HttpPostedFileBase VenueFile { get; set; }
        public HttpPostedFileBase AccomodationFile { get; set; }
        public HttpPostedFileBase AttendanceFile { get; set; }
        public HttpPostedFileBase FeedbackFile { get; set; }


        public VenueInfo VenueInfo { get; set; }
        public AccomodationInfo AccomodationInfo { get; set; }
        public DriverInfo DriverInfo { get; set; }
        public PhotographerInfo PhotographerInfo { get; set; }
        public LocalVisaAgentInfo LocalVisaAgentInfo { get; set; }
        public PostEventInfo PostEventInfo { get; set; }

        public ICollection<EventSession> EventSessions { get; set; }
        public EventModel()
        {
            GroupsSelected = new List<int>();
            UsersSelected = new List<int>();
            CompaniesSelected = new List<int>();
            EventSessions = new HashSet<EventSession>();
        }
        public void Prepare()
        {
            var saleRepo = DependencyHelper.GetService<ISalesGroupService>();
            var unitRepo = DependencyHelper.GetService<IUnitRepository>();
            Users = saleRepo.GetAllSalesmans();
            SalesGroups = saleRepo.GetAllSalesGroups();
            EventCategories = unitRepo.GetEventCategories();
            Event = new Event
            {
                UserID = CurrentUser.Identity.ID
            };
        }
        public void PrepareEdit(int id)
        {
            var repo = DependencyHelper.GetService<IEventService>();
            Event = repo.GetEvent(id);
            if (Event != null)
            {
                GroupsSelected = Event.SalesGroups.Select(m => m.ID).ToList();
                UsersSelected = Event.ManagerUsers.Select(m => m.ID).ToList();

                var saleRepo = DependencyHelper.GetService<ISalesGroupService>();
                var unitRepo = DependencyHelper.GetService<IUnitRepository>();
                Users = saleRepo.GetAllSalesmans();
                SalesGroups = saleRepo.GetAllSalesGroups();
                EventCategories = unitRepo.GetEventCategories();
                Event.EventSessions = Event.EventSessions.Where(m => m.EntityStatus == EntityStatus.Normal).ToList();
                //Event.EventCompanies = Event.EventCompanies.Where(m => m.EntityStatus == EntityStatus.Normal).ToList();
            }
        }
        public void PrepareOperationEdit(int id, bool initial = true)
        {
            var repo = DependencyHelper.GetService<IEventService>();
            Event = repo.GetEvent(id);
            ID = id;
            var currentUser = CurrentUser.Identity;
            if (initial && Event != null)
            {
                VenueInfo = Event.VenueInfo;
                AccomodationInfo = Event.AccomodationInfo;
                DriverInfo = Event.DriverInfo;
                PhotographerInfo = Event.PhotographerInfo;
                LocalVisaAgentInfo = Event.LocalVisaAgentInfo;
                PostEventInfo = Event.PostEventInfo;
                EventSessions = Event.EventSessions;
            }
            if (VenueInfo == null) VenueInfo = new VenueInfo { EntryId = ID, UserId = currentUser.ID };
            if (AccomodationInfo == null) AccomodationInfo = new AccomodationInfo { EntryId = ID, UserId = currentUser.ID };
            if (DriverInfo == null) DriverInfo = new DriverInfo();
            if (PhotographerInfo == null) PhotographerInfo = new PhotographerInfo();
            if (LocalVisaAgentInfo == null) LocalVisaAgentInfo = new LocalVisaAgentInfo();
            if (PostEventInfo == null) PostEventInfo = new PostEventInfo();
        }
        public void PrepareAssign(int id)
        {
            var repo = DependencyHelper.GetService<IEventService>();
            Event = repo.GetEvent(id);
            if (Event != null)
            {
                CompaniesSelected = Event.EventCompanies.Select(m => m.CompanyID).ToList();
            }
        }
        public bool Create()
        {
            var repo = DependencyHelper.GetService<IEventService>();
            if (SalesBrochureFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Event, SalesBrochureFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Event.SalesBrochure = uploadPicture;
                }
            }
            if (OperationBrochureFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Event, OperationBrochureFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Event.OperationBrochure = uploadPicture;
                }
            }
            if (RegContractFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Event, RegContractFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Event.RegContract = uploadPicture;
                }
            }
            Event = repo.CreateEvent(Event, GroupsSelected, UsersSelected);
            if (Event != null)
            {
                return true;
            }
            return false;
        }
        public bool Update()
        {
            var repo = DependencyHelper.GetService<IEventService>();
            if (SalesBrochureFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Event, SalesBrochureFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Event.SalesBrochure = uploadPicture;
                }
            }
            if (OperationBrochureFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Event, OperationBrochureFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Event.OperationBrochure = uploadPicture;
                }
            }
            if (RegContractFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Event, RegContractFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Event.RegContract = uploadPicture;
                }
            }
            return repo.UpdateEventIncludeUpdateCollection(Event, GroupsSelected, UsersSelected) != null;
        }
        public bool OperationUpdate()
        {
            var repo = DependencyHelper.GetService<IEventService>();
            if (VenueFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Venue, VenueFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    VenueInfo.HotelContract = uploadPicture;
                }
            }
            if (AccomodationFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Accomodation, AccomodationFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    AccomodationInfo.HotelContract = uploadPicture;
                }
            }
            if (AttendanceFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.PostEvent, AttendanceFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    PostEventInfo.AttendanceUpload = uploadPicture;
                }
            }
            if (FeedbackFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.PostEvent, FeedbackFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    PostEventInfo.FeedbackUpload = uploadPicture;
                }
            }
            if (EventSessions != null)
                foreach (var eventSession in EventSessions)
                {
                    if (eventSession.TrainerInvoiceFile != null)
                    {
                        string uploadPicture = FileUpload.Upload(FileUploadType.Trainer, eventSession.TrainerInvoiceFile);
                        if (!string.IsNullOrEmpty(uploadPicture))
                        {
                            eventSession.TrainerInvoice = uploadPicture;
                        }
                    }
                    if (eventSession.TrainerTicketFile != null)
                    {
                        string uploadPicture = FileUpload.Upload(FileUploadType.Trainer, eventSession.TrainerTicketFile);
                        if (!string.IsNullOrEmpty(uploadPicture))
                        {
                            eventSession.TrainerTicket = uploadPicture;
                        }
                    }
                    if (eventSession.TrainerVisaFile != null)
                    {
                        string uploadPicture = FileUpload.Upload(FileUploadType.Trainer, eventSession.TrainerVisaFile);
                        if (!string.IsNullOrEmpty(uploadPicture))
                        {
                            eventSession.TrainerVisa = uploadPicture;
                        }
                    }
                    if (eventSession.TrainerInsuranceFile != null)
                    {
                        string uploadPicture = FileUpload.Upload(FileUploadType.Trainer, eventSession.TrainerInsuranceFile);
                        if (!string.IsNullOrEmpty(uploadPicture))
                        {
                            eventSession.TrainerInsurance = uploadPicture;
                        }
                    }
                    if (eventSession.OperationTicketFile != null)
                    {
                        string uploadPicture = FileUpload.Upload(FileUploadType.Trainer, eventSession.OperationTicketFile);
                        if (!string.IsNullOrEmpty(uploadPicture))
                        {
                            eventSession.OperationTicket = uploadPicture;
                        }
                    }
                    if (eventSession.OperationVisaFile != null)
                    {
                        string uploadPicture = FileUpload.Upload(FileUploadType.Trainer, eventSession.OperationVisaFile);
                        if (!string.IsNullOrEmpty(uploadPicture))
                        {
                            eventSession.OperationVisa = uploadPicture;
                        }
                    }
                    if (eventSession.OperationInsuranceFile != null)
                    {
                        string uploadPicture = FileUpload.Upload(FileUploadType.Trainer, eventSession.OperationInsuranceFile);
                        if (!string.IsNullOrEmpty(uploadPicture))
                        {
                            eventSession.OperationInsurance = uploadPicture;
                        }
                    }
                }
            if (!string.IsNullOrEmpty(VenueInfo.HotelVenue) &&
                (VenueInfo.Status == InfoStatus.Rejected ||
                 VenueInfo.Status == InfoStatus.Initial))
            {
                VenueInfo.Status = InfoStatus.Request;
            }
            if (!string.IsNullOrEmpty(AccomodationInfo.HotelAccomodation) &&
                (AccomodationInfo.Status == InfoStatus.Rejected ||
                 AccomodationInfo.Status == InfoStatus.Initial))
            {
                AccomodationInfo.Status = InfoStatus.Request;
            }
            var result = repo.UpdateEventOperation(ID, VenueInfo, AccomodationInfo, DriverInfo, PhotographerInfo,
                LocalVisaAgentInfo, PostEventInfo, EventSessions) != null;
            if (result)
            {
                if (VenueInfo.Status == InfoStatus.Request && AccomodationInfo.Status == InfoStatus.Request)
                {
                    OpeEventNotificator.NotifyUser(NotifyAction.Request, ID, "Operation Request Hotel");
                }
                else if (VenueInfo.Status == InfoStatus.Request)
                {
                    OpeEventNotificator.NotifyUser(NotifyAction.Request, ID, "Operation Request Hotel");
                }
                else if (AccomodationInfo.Status == InfoStatus.Request)
                {
                    OpeEventNotificator.NotifyUser(NotifyAction.Request, ID, "Operation Request Hotel");
                }
                return true;
            }
            return false;
        }
        public bool AssignCompany()
        {
            var repo = DependencyHelper.GetService<IEventService>();
            if (repo.AssignCompany(ID, CompaniesSelected))
            {
                return true;
            }
            return false;
        }
    }
}