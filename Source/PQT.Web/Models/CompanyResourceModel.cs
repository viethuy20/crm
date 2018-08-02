using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NS.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Web.Models
{
    public class CompanyResourceModel
    {
        public CompanyResource CompanyResource { get; set; }

        public CompanyResourceModel()
        {
            CompanyResource = new CompanyResource();
        }
        public void Prepare(int id)
        {
            var repo = DependencyHelper.GetService<ICompanyRepository>();
            if (id == 0)
                return;
            CompanyResource = repo.GetCompanyResource(id);
        }
    }
}