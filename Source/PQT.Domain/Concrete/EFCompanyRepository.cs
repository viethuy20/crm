using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
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

        public IEnumerable<Company> GetAllCompanies()
        {
            return GetAll<Company>(m => m.Country).AsEnumerable();
        }

        public IEnumerable<Company> GetAllCompanies(Func<Company, bool> predicate)
        {
            return GetAll(predicate).AsEnumerable();
        }

        public Company GetCompany(int companyID)
        {
            return Get<Company>(companyID);
        }

        public Company GetCompany(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            return Get<Company>(m => m.CompanyName.Trim().ToLower() == name.Trim().ToLower());
        }

        public Company CreateCompany(Company company)
        {
            return Create(company);
        }

        public void CreateCompanies(List<Company> companies)
        {
            foreach (var company in companies)
            {
                _db.Set<Company>().Add(company);
            }
            _db.SaveChanges();
        }
        public bool UpdateCompany(Company company)
        {
            if (company.ID == 0)
            {
                Create(company);
                return true;
            }
            return Update(company);
        }

        public bool DeleteCompany(int companyID)
        {
            return Delete<Company>(companyID);
        }

        #endregion Company


        #region Company Resource
        public IEnumerable<CompanyResource> GetAllCompanyResources()
        {
            return GetAll<CompanyResource>().AsEnumerable();
        }

        public IEnumerable<CompanyResource> GetAllCompanyResources(Func<CompanyResource, bool> predicate)
        {
            return GetAll(predicate).AsEnumerable();
        }

        public CompanyResource GetCompanyResource(int companyID)
        {
            return Get<CompanyResource>(companyID);
        }

        public CompanyResource CreateCompanyResource(CompanyResource resource)
        {
            return Create(resource);
        }

        //public void CreateCompanyResources(List<CompanyResource> companies)
        //{
        //    foreach (var company in companies)
        //    {
        //        _db.Set<CompanyResource>().Add(company);
        //    }
        //    _db.SaveChanges();
        //}
        public bool UpdateCompanyResource(CompanyResource resource)
        {
            return Update(resource);
        }

        public bool DeleteCompanyResource(int resourceID)
        {
            return Delete<CompanyResource>(resourceID);
        }
        #endregion Company Resource

    }
}
