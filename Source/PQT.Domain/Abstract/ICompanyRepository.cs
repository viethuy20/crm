using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface ICompanyRepository
    {
        #region Company
        IEnumerable<Company> GetAllCompanies();
        IEnumerable<Company> GetAllCompanies(Func<Company, bool> predicate);
        Company GetCompany(int companyID);
        Company GetCompany(string name);
        Company CreateCompany(Company company);
        bool UpdateCompany(Company company);
        bool DeleteCompany(int companyID);
        #endregion Company

        #region Company Resource
        IEnumerable<CompanyResource> GetAllCompanyResources();
        IEnumerable<CompanyResource> GetAllCompanyResources(Func<CompanyResource, bool> predicate);
        CompanyResource GetCompanyResource(int companyID);
        CompanyResource CreateCompanyResource(CompanyResource resource);
        bool UpdateCompanyResource(CompanyResource resource);
        bool DeleteCompanyResource(int resourceID);
        #endregion Company Resource
    }
}
