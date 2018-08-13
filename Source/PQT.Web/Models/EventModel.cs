using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NS.Entity;
using PQT.Domain;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Utility;

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
        public HttpPostedFileBase BrochureFile { get; set; }
        public HttpPostedFileBase RegContractFile { get; set; }
        public EventModel()
        {
            GroupsSelected = new List<int>();
            UsersSelected = new List<int>();
            CompaniesSelected = new List<int>();
            Event = new Event();
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
            if (BrochureFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Event, BrochureFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Event.Brochure = uploadPicture;
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
            if (BrochureFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Event, BrochureFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Event.Brochure = uploadPicture;
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
            if (repo.UpdateEventIncludeUpdateCollection(Event, GroupsSelected, UsersSelected))
            {
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