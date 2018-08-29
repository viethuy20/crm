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
        //int GetCountCompanies(Func<Company, bool> predicate);
        //IEnumerable<Company> GetAllCompanies(string sortColumnDir, string sortColumn, int page, int pageSize);
        IEnumerable<Company> GetAllCompanies();
        IEnumerable<Company> GetAllCompanies(Func<Company, bool> predicate);
        Company GetCompany(int companyID);
        Company GetCompanyInDb(int companyID);
        Company GetCompany(string name);
        Company CreateCompany(Company company, IEnumerable<int> users);
        //List<Company> CreateCompanies(List<Company> companies);
        bool UpdateCompany(Company company);
        Company UpdateCompany(Company company, IEnumerable<int> users);
        bool DeleteCompany(int companyID);
        #endregion Company

        #region Company Resource
        IEnumerable<CompanyResource> GetAllCompanyResources();
        IEnumerable<CompanyResource> GetAllCompanyResources(Func<CompanyResource, bool> predicate);
        CompanyResource GetCompanyResource(int resourceId);
        CompanyResource CreateCompanyResource(CompanyResource resource);
        //void CreateCompanyResources(List<CompanyResource> companies);
        bool UpdateCompanyResource(CompanyResource resource);
        bool DeleteCompanyResource(int resourceID);
        #endregion Company Resource

    }
}
