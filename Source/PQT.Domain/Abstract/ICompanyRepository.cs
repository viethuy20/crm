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

        int GetCountCompanies(string companyName, string countryName, string productService, string sector,
            int tier, string industry, string businessUnit,
            string ownership, int financialYear);

        IEnumerable<Company> GetAllCompanies(string companyName, string countryName, string productService,
            string sector,int tier, string industry, string businessUnit, string ownership, int financialYear,
            string sortColumnDir, string sortColumn, int page, int pageSize);

        IEnumerable<Company> GetCompaniesForAjaxDropdown(int id, string q);

        int GetCountCompaniesForAssignEvent(int tier, int[] saleIds, string companyName,
            string[] countries, string[] productServices, string[] sectors, string[] industries);
        IEnumerable<Company> GetAllCompaniesForAssignEvent(int tier,int[] saleIds,string companyName,
            string[] countries,string[] productServices, string[] sectors, string[] industries,
            string sortColumnDir, string sortColumn, int page, int pageSize);
        IEnumerable<Company> GetAllCompanies(Func<Company, bool> predicate);
        Company GetCompany(int companyID);
        Company GetCompanyInDb(int companyID);
        Company GetCompany(string name);
        Company CreateCompany(Company company, IEnumerable<int> users);
        //List<Company> CreateCompanies(List<Company> companies);
        bool UpdateCompany(Company company);
        Company UpdateCompany(Company company, IEnumerable<int> users);
        Company MergeCompany(int companyID, int mergeCompanyID);
        bool DeleteCompany(int companyID);
        #endregion Company

        #region Company Resource
        IEnumerable<CompanyResource> GetAllCompanyResources();
        int GetAllCompanyResourcesCheckPhone(int id, string directLine,string mobilePhone1,string mobilePhone2,string mobilePhone3);
        CompanyResource GetAllCompanyResourcesCheckPhoneForMerge(string role,string directLine,string mobilePhone1,string mobilePhone2,string mobilePhone3);
        IEnumerable<CompanyResource> GetAllCompanyResources(int comId,string name,string role,string email,string phone,string searchValue);
        IEnumerable<CompanyResource> GetAllCompanyResources(string[] countries, string[] organisations,string[] roles, string[] name, string[] email,string[] phone);
        int GetCountCompanyResources(string[] countries, string[] organisations,string[] roles, string[] name, string[] email,string[] phone);
        IEnumerable<CompanyResource> GetAllCompanyResources(string[] countries, string[] organisations,string[] roles, string[] name, string[] email,string[] phone, string sortColumnDir, string sortColumn, int page, int pageSize);
        IEnumerable<CompanyResource> GetAllCompanyResources(int[] comIds);
        int CountCompanyResources(int comId);
        IEnumerable<CompanyResource> GetAllCompanyResources(Func<CompanyResource, bool> predicate);
        CompanyResource GetCompanyResource(int resourceId);
        CompanyResource CreateCompanyResource(CompanyResource resource);
        //void CreateCompanyResources(List<CompanyResource> companies);
        bool UpdateCompanyResource(CompanyResource resource);
        bool DeleteCompanyResource(int resourceID);
        #endregion Company Resource

    }
}
