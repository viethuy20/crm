using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Infrastructure;

namespace PQT.Web.Models
{
    public class CompanyModel
    {
        public Company Company { get; set; }
        public List<int> UsersSelected { get; set; }
        public IEnumerable<User> Users { get; set; }

        public CompanyModel()
        {
            UsersSelected = new List<int>();
            Users = new List<User>();
            Company = new Company();
        }
        public void Prepare(int id)
        {
            var saleRepo = DependencyHelper.GetService<ISalesGroupService>();
            var repo = DependencyHelper.GetService<ICompanyRepository>();
            Users = saleRepo.GetAllSalesmans();
            if (id == 0)
                return;
            Company = repo.GetCompany(id);
            if (Company != null)
            {
                UsersSelected = Company.ManagerUsers.Select(m => m.ID).ToList();
            }
        }
    }
}