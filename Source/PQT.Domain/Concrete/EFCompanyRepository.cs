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

        public int GetCountCompanies(Func<Company, bool> predicate)
        {
            if (predicate != null)
            {
                return _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(predicate).Count();
            }
            return _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Count();
        }

        public IEnumerable<Company> GetAllCompanies(string sortColumnDir, string sortColumn, int page, int pageSize)
        {
            IEnumerable<Company> companies = new HashSet<Company>();
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CountryName":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .OrderBy(s => s.Country.Name).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "ProductOrService":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .OrderBy(s => s.ProductOrService).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "Sector":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .OrderBy(s => s.Sector).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Industry":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .OrderBy(s => s.Industry).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "Tier":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .OrderBy(s => s.Tier).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    default:
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .OrderBy(s => s.CompanyName).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CountryName":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .OrderByDescending(s => s.Country.Name).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "ProductOrService":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .OrderByDescending(s => s.ProductOrService).ThenBy(s => s.CompanyName).Skip(page)
                            .Take(pageSize).AsEnumerable();
                        break;
                    case "Sector":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .OrderByDescending(s => s.Sector).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "Industry":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .OrderByDescending(s => s.Industry).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "Tier":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .OrderByDescending(s => s.Tier).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    default:
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .OrderByDescending(s => s.CompanyName).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                }
            }
            return companies;
        }

        public IEnumerable<Company> GetAllCompanies(Func<Company, bool> predicate, string sortColumnDir,
            string sortColumn, int page, int pageSize)
        {
            IEnumerable<Company> companies = new HashSet<Company>();
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CountryName":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(predicate).OrderBy(s => s.CountryName).ThenBy(s => s.CompanyName).Skip(page)
                            .Take(pageSize).AsEnumerable();
                        break;
                    case "ProductOrService":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(predicate).OrderBy(s => s.ProductOrService).ThenBy(s => s.CompanyName).Skip(page)
                            .Take(pageSize).AsEnumerable();
                        break;
                    case "Sector":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(predicate).OrderBy(s => s.Sector).ThenBy(s => s.CompanyName).Skip(page)
                            .Take(pageSize).AsEnumerable();
                        break;
                    case "Industry":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(predicate).OrderBy(s => s.Industry).ThenBy(s => s.CompanyName).Skip(page)
                            .Take(pageSize).AsEnumerable();
                        break;
                    case "Tier":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(predicate).OrderBy(s => s.Tier).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    default:
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(predicate).OrderBy(s => s.CompanyName).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CountryName":
                        companies = _db.Set<Company>()
                            .Include(m => m.Country)
                            .Include(m => m.ManagerUsers).AsEnumerable().Where(predicate)
                            .OrderByDescending(s => s.CountryName).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "ProductOrService":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(predicate).OrderByDescending(s => s.ProductOrService)
                            .ThenBy(s => s.CompanyName).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Sector":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(predicate).OrderByDescending(s => s.Sector).ThenBy(s => s.CompanyName)
                            .Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Industry":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(predicate).OrderByDescending(s => s.Industry)
                            .ThenBy(s => s.CompanyName).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Tier":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(predicate).OrderByDescending(s => s.Tier).ThenBy(s => s.CompanyName)
                            .Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    default:
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(predicate).OrderByDescending(s => s.CompanyName).Skip(page)
                            .Take(pageSize).AsEnumerable();
                        break;
                }
            }
            return companies;
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

        public Company CreateCompany(Company company, IEnumerable<int> users)
        {
            if (users.Any())
                company.ManagerUsers = _db.Set<User>().Where(r => users.Contains(r.ID)).ToList();
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
        public bool UpdateCompany(Company company, IEnumerable<int> users)
        {
            if (company.ID == 0)
            {
                if (users.Any())
                    company.ManagerUsers = _db.Set<User>().Where(r => users.Contains(r.ID)).ToList();
                Create(company);
                return true;
            }
            Update(company);
            var exist = Get<Company>(company.ID);
            exist.ManagerUsers.Clear();
            if (users.Any())
                exist.ManagerUsers = _db.Set<User>().Where(r => users.Contains(r.ID)).ToList();
            return Update(exist);
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
