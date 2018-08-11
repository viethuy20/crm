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
        public virtual void RetrieveCacheCompanies()
        {
        }
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
            return Create(company);
        }

        public virtual List<Company> CreateCompanies(List<Company> companies)
        {
            foreach (var company in companies)
            {
                _db.Set<Company>().Add(company);
            }
            _db.SaveChanges();
            return companies;
        }
        public virtual bool UpdateCompany(Company company)
        {
            return Update(company);
        }
        public virtual Company UpdateCompany(Company company, IEnumerable<int> users)
        {
            if (company.ID == 0)
            {
                if (users.Any())
                    company.ManagerUsers = _db.Set<User>().Where(r => users.Contains(r.ID)).ToList();
                Create(company);
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
            Update(exist);
            return exist;
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
