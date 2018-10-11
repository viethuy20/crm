using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Domain.Concrete
{
    public class EFCompanyRepository : Repository, ICompanyRepository
    {
        public EFCompanyRepository(DbContext db)
            : base(db)
        {
        }

        #region Company
        public virtual IEnumerable<Company> GetAllCompanies()
        {
            return GetAll<Company>(m => new
            {
                m.Country,
                m.ManagerUsers,
            }).Select(m => new Company(m)).AsEnumerable();
        }
        public virtual IEnumerable<Company> GetAllCompanies(Func<Company, bool> predicate)
        {
            return GetAll(predicate, m => new
            {
                m.Country,
                m.ManagerUsers,
            }).Select(m => new Company(m)).AsEnumerable();
        }

        public virtual Company GetCompany(int companyID)
        {
            return Get<Company>(m => m.ID == companyID, m => new
            {
                m.Country,
                m.ManagerUsers,
            });
        }
        public virtual Company GetCompanyInDb(int companyID)
        {
            return Get<Company>(m => m.ID == companyID, m => new
            {
                m.Country,
                m.ManagerUsers,
            });
        }

        public virtual Company GetCompany(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            return Get<Company>(m => m.CompanyName.Trim().ToLower() == name.Trim().ToLower());
        }

        public virtual Company CreateCompany(Company company, IEnumerable<int> users)
        {
            if (users.Any())
                company.ManagerUsers = _db.Set<User>().Where(r => users.Contains(r.ID)).ToList();
            company = Create(company);
            if (company != null)
            {
                company.Country = Get<Country>(Convert.ToInt32(company.CountryID));
            }
            return company;
        }

        //public virtual List<Company> CreateCompanies(List<Company> companies)
        //{
        //    foreach (var company in companies)
        //    {
        //        _db.Set<Company>().Add(company);
        //    }
        //    _db.SaveChanges();
        //    return companies;
        //}
        public virtual bool UpdateCompany(Company company)
        {
            if (Update(company))
            {
                company.Country = Get<Country>(Convert.ToInt32(company.CountryID));
                return true;
            }
            return false;
        }
        public virtual Company UpdateCompany(Company company, IEnumerable<int> users)
        {
            if (company.ID == 0)
            {
                if (users.Any())
                    company.ManagerUsers = _db.Set<User>().Where(r => users.Contains(r.ID)).ToList();
                company = Create(company);
                if (company != null)
                {
                    company.Country = Get<Country>(Convert.ToInt32(company.CountryID));
                }
                return company;
            }
            var exist = Get<Company>(company.ID);
            if (exist == null) return null;
            exist.CountryID = company.CountryID;
            exist.Country = null;
            exist.CompanyName = company.CompanyName;
            exist.ProductOrService = company.ProductOrService;
            exist.Sector = company.Sector;
            exist.Industry = company.Industry;
            exist.Ownership = company.Ownership;
            exist.BusinessUnit = company.BusinessUnit;
            exist.BudgetMonth = company.BudgetMonth;
            exist.BudgetPerHead = company.BudgetPerHead;
            exist.FinancialYear = company.FinancialYear;
            exist.Tier = company.Tier;
            exist.Address = company.Address;
            exist.Tel = company.Tel;
            exist.Fax = company.Fax;
            exist.Remarks = company.Remarks;
            exist.ManagerUsers.Clear();
            if (users.Any())
                exist.ManagerUsers = _db.Set<User>().Where(r => users.Contains(r.ID)).ToList();
            if (Update(exist))
            {
                exist.Country = Get<Country>(Convert.ToInt32(company.CountryID));
                var allResources = GetAll<CompanyResource>(m => m.CompanyID == company.ID);
                foreach (var companyResource in allResources)
                {
                    companyResource.Organisation = exist.CompanyName;
                    companyResource.CountryID = exist.CountryID;
                    Update(companyResource);
                }
                return exist;
            }
            return null;
        }

        public virtual Company MergeCompany(int companyID, int mergeCompanyID)
        {

            var com = Get<Company>(companyID);
            if (com == null) return null;

            var mergeCom = Get<Company>(mergeCompanyID);
            if (mergeCom == null) return null;
            mergeCom.CountryID = com.CountryID;
            mergeCom.Country = null;
            mergeCom.CompanyName = com.CompanyName;
            mergeCom.ProductOrService = com.ProductOrService;
            mergeCom.Sector = com.Sector;
            mergeCom.Industry = com.Industry;
            mergeCom.Ownership = com.Ownership;
            mergeCom.BusinessUnit = com.BusinessUnit;
            mergeCom.BudgetMonth = com.BudgetMonth;
            mergeCom.BudgetPerHead = com.BudgetPerHead;
            mergeCom.FinancialYear = com.FinancialYear;
            mergeCom.Tier = com.Tier;
            mergeCom.Address = com.Address;
            mergeCom.Tel = com.Tel;
            mergeCom.Fax = com.Fax;
            mergeCom.Remarks = com.Remarks;
            mergeCom.ManagerUsers = _db.Set<User>().Where(r => mergeCom.ManagerUsers.Select(m => m.ID).Contains(r.ID) ||
                                                               com.ManagerUsers.Select(m => m.ID).Contains(r.ID)).ToList();
            if (Update(mergeCom))
            {
                //Delete<Company>(companyID);
                var resources = GetAll<CompanyResource>(m => m.CompanyID == companyID || m.CompanyID == mergeCompanyID);
                foreach (var companyResource in resources)
                {
                    companyResource.Organisation = mergeCom.CompanyName;
                    companyResource.CountryID = mergeCom.CountryID;
                    companyResource.CompanyID = mergeCompanyID;
                    Update(companyResource);
                }
                mergeCom.Country = Get<Country>(Convert.ToInt32(mergeCom.CountryID));
                return mergeCom;
            }
            return null;
        }
        public virtual bool DeleteCompany(int companyID)
        {
            return Delete<Company>(companyID);
        }

        #endregion Company


        #region Company Resource
        public virtual IEnumerable<CompanyResource> GetAllCompanyResources()
        {
            return GetAll<CompanyResource>().AsEnumerable();
        }

        public virtual IEnumerable<CompanyResource> GetAllCompanyResources(Func<CompanyResource, bool> predicate)
        {
            return GetAll(predicate).AsEnumerable();
        }

        public virtual CompanyResource GetCompanyResource(int resourceId)
        {
            return Get<CompanyResource>(resourceId);
        }

        public virtual CompanyResource CreateCompanyResource(CompanyResource resource)
        {
            return Create(resource);
        }

        public virtual bool UpdateCompanyResource(CompanyResource resource)
        {
            return Update(resource);
        }

        public virtual bool DeleteCompanyResource(int resourceID)
        {
            return Delete<CompanyResource>(resourceID);
        }

        #endregion Company Resource

    }
}
