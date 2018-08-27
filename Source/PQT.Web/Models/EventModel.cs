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

        public EventModel()
        {
            GroupsSelected = new List<int>();
            UsersSelected = new List<int>();
            CompaniesSelected = new List<int>();
        }
        public void Prepare()
        {
            var saleRepo = DependencyHelper.GetService<ISalesGroupService>();
            Users = saleRepo.GetAllSalesmans();
            SalesGroups = saleRepo.GetAllSalesGroups();
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
                Users = saleRepo.GetAllSalesmans();
                SalesGroups = saleRepo.GetAllSalesGroups();
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
            if (VenueInfo.Status == BookingStatus.Rejected)
            {
                VenueInfo.Status = InfoStatus.Request;
            }
            if (AccomodationInfo.Status == BookingStatus.Rejected)
            {
                AccomodationInfo.Status = InfoStatus.Request;
            }
            return repo.UpdateEventOperation(ID, VenueInfo, AccomodationInfo, DriverInfo, PhotographerInfo,
                LocalVisaAgentInfo, PostEventInfo) != null;
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