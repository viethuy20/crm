using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NS.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Web.Models
{
    public class SalesGroupModel
    {
        public SalesGroupModel()
        {
            UsersSelected = new List<int>();
            Users = new HashSet<User>();
        }
        public int ID { get; set; }
        public string GroupName { get; set; }
        public List<int> UsersSelected { get; set; }
        public IEnumerable<User> Users { get; set; }
        public SalesGroup SalesGroup { get; set; }

        public void AddNewGroup()
        {
            var repo = DependencyHelper.GetService<ISalesGroupService>();
            var newGroup = new SalesGroup { GroupName = GroupName };
            SalesGroup = repo.CreateSalesGroup(newGroup, UsersSelected);
        }
        public string UpdateGroup()
        {
            var repo = DependencyHelper.GetService<ISalesGroupService>();
            SalesGroup = repo.UpdateSalesGroup(ID, GroupName, UsersSelected);
            if (SalesGroup != null)
            {
                return "";
            }
            return "Group not found.";
        }
    }
}