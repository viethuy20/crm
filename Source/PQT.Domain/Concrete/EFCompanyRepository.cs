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

        public int GetCountCompanies(Func<Company, bool> predicate)
        {
            if (predicate != null)
            {
                return _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(predicate).Count();
            }
            return _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Count();
        }

        public IEnumerable<Company> GetAllCompanies(string sortColumnDir, string sortColumn, int page, int pageSize)
        {
            IEnumerable<Company> companies = new HashSet<Company>();
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CountryName":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m=>m.EntityStatus.Value == EntityStatus.Normal.Value)
                            .OrderBy(s => s.Country.Name).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "ProductOrService":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                            .OrderBy(s => s.ProductOrService).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "Sector":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                            .OrderBy(s => s.Sector).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Industry":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                            .OrderBy(s => s.Industry).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "Tier":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                            .OrderBy(s => s.Tier).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    default:
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                            .OrderBy(s => s.CompanyName).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CountryName":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                            .OrderByDescending(s => s.Country.Name).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "ProductOrService":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                            .OrderByDescending(s => s.ProductOrService).ThenBy(s => s.CompanyName).Skip(page)
                            .Take(pageSize).AsEnumerable();
                        break;
                    case "Sector":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                            .OrderByDescending(s => s.Sector).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "Industry":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                            .OrderByDescending(s => s.Industry).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "Tier":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                            .OrderByDescending(s => s.Tier).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    default:
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers).Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
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
                            .AsEnumerable().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(predicate).OrderBy(s => s.CountryName).ThenBy(s => s.CompanyName).Skip(page)
                            .Take(pageSize).AsEnumerable();
                        break;
                    case "ProductOrService":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(predicate).OrderBy(s => s.ProductOrService).ThenBy(s => s.CompanyName).Skip(page)
                            .Take(pageSize).AsEnumerable();
                        break;
                    case "Sector":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(predicate).OrderBy(s => s.Sector).ThenBy(s => s.CompanyName).Skip(page)
                            .Take(pageSize).AsEnumerable();
                        break;
                    case "Industry":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(predicate).OrderBy(s => s.Industry).ThenBy(s => s.CompanyName).Skip(page)
                            .Take(pageSize).AsEnumerable();
                        break;
                    case "Tier":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(predicate).OrderBy(s => s.Tier).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    default:
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(predicate).OrderBy(s => s.CompanyName).Skip(page).Take(pageSize).AsEnumerable();
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
                            .Include(m => m.ManagerUsers).AsEnumerable().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(predicate)
                            .OrderByDescending(s => s.CountryName).ThenBy(s => s.CompanyName).Skip(page).Take(pageSize)
                            .AsEnumerable();
                        break;
                    case "ProductOrService":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(predicate).OrderByDescending(s => s.ProductOrService)
                            .ThenBy(s => s.CompanyName).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Sector":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(predicate).OrderByDescending(s => s.Sector).ThenBy(s => s.CompanyName)
                            .Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Industry":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(predicate).OrderByDescending(s => s.Industry)
                            .ThenBy(s => s.CompanyName).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Tier":
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(predicate).OrderByDescending(s => s.Tier).ThenBy(s => s.CompanyName)
                            .Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    default:
                        companies = _db.Set<Company>().Include(m => m.Country).Include(m => m.ManagerUsers)
                            .AsEnumerable().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(predicate).OrderByDescending(s => s.CompanyName).Skip(page)
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
        public bool UpdateCompany(Company company)
        {
            return Update(company);
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

        public CompanyResource GetCompanyResource(int resourceId)
        {
            return Get<CompanyResource>(resourceId);
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

        public EventCompany GetEventCompany(int eventId, int companyId)
        {
            return GetAll<EventCompany>(m => m.EventID == eventId && m.CompanyID == companyId).LastOrDefault();
        }

        public EventCompany GetEventCompany(int companyId)
        {
            return GetAll<EventCompany>(m => m.CompanyID == companyId).OrderBy(m => m.UpdatedTime).LastOrDefault(m => m.UpdatedTime != null);
        }

        public bool UpdateEventCompany(EventCompany company)
        {
            var exist = Get<EventCompany>(company.ID);
            if (exist != null)
            {
                //exist.EstimatedDelegateNumber = company.EstimatedDelegateNumber;
                //exist.FirstFollowUpStatus = company.FirstFollowUpStatus;
                //exist.FinalStatus = company.FinalStatus;
                //exist.DateNextFollowUp = company.DateNextFollowUp;
                exist.BudgetMonth = company.BudgetMonth;
                //exist.GoodTrainingMonth = company.GoodTrainingMonth;
                //exist.TopicsInterested = company.TopicsInterested;
                //exist.LocationInterested = company.LocationInterested;
                //exist.TrainingBudget = company.TrainingBudget;
                exist.Remarks = company.Remarks;
                return Update(exist);
            }
            return false;
        }
    }
}
