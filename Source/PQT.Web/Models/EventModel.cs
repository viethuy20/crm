using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PQT.Domain;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Models
{
    public class EventModel
    {
        public Event Event { get; set; }
        public List<int> UsersSelected { get; set; }
        public List<int> GroupsSelected { get; set; }
        public List<int> CompaniesSelected { get; set; }
        public IEnumerable<Company> Companies { get; set; }
        public IEnumerable<SalesGroup> SalesGroups { get; set; }
        public IEnumerable<User> Users { get; set; }

        public EventModel()
        {
            UsersSelected = new List<int>();
            GroupsSelected = new List<int>();
            CompaniesSelected = new List<int>();
            Event = new Event
            {
                UserID = CurrentUser.Identity.ID
            };
        }
        public void Prepare()
        {
            var comRepo = DependencyHelper.GetService<ICompanyRepository>();
            Companies = comRepo.GetAllCompanies();
            var saleRepo = DependencyHelper.GetService<ISalesGroupService>();
            Users = saleRepo.GetAllSalesmans();
            SalesGroups = saleRepo.GetAllSalesGroups();
        }
        public void PrepareEdit(int id)
        {
            var repo = DependencyHelper.GetService<IEventService>();
            Event = repo.GetEvent(id);
            if (Event != null)
            {
                UsersSelected = Event.Users.Select(m => m.ID).ToList();
                GroupsSelected = Event.SalesGroups.Select(m => m.ID).ToList();
                CompaniesSelected = Event.Companies.Select(m => m.ID).ToList();

                var comRepo = DependencyHelper.GetService<ICompanyRepository>();
                Companies = comRepo.GetAllCompanies();
                var saleRepo = DependencyHelper.GetService<ISalesGroupService>();
                Users = saleRepo.GetAllSalesmans();
                SalesGroups = saleRepo.GetAllSalesGroups();
            }
        }
        public bool Create()
        {
            var repo = DependencyHelper.GetService<IEventService>();
            if (repo.CreateEvent(Event, CompaniesSelected, GroupsSelected, UsersSelected) != null)
            {
                return true;
            }
            return false;
        }
        public bool Update()
        {
            var repo = DependencyHelper.GetService<IEventService>();
            if (repo.UpdateEventIncludeUpdateCollection(Event, CompaniesSelected, GroupsSelected, UsersSelected))
            {
                return true;
            }
            return false;
        }
    }
}