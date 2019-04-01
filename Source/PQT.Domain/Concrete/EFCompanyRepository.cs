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

        public int GetCountCompanies(string companyName, string countryName, string productService, string sector,
            int tier, string industry, string businessUnit,
            string ownership, int financialYear)
        {
            IQueryable<Company> queries = _db.Set<Company>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(companyName))
                queries = queries.Where(m => m.CompanyName.ToLower().Contains(companyName));
            if (!string.IsNullOrEmpty(countryName))
                queries = queries.Where(m => m.Country != null && (m.Country.Code.ToLower().Contains(countryName) ||
                                                                   m.Country.Name.ToLower().Contains(countryName)));
            if (!string.IsNullOrEmpty(productService))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.ProductOrService) && m.ProductOrService.ToLower().Contains(productService));
            if (!string.IsNullOrEmpty(sector))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Sector) && m.Sector.ToLower().Contains(sector));
            if (tier > 0)
                queries = queries.Where(m => m.Tier == tier);
            if (!string.IsNullOrEmpty(industry))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Industry) && m.Industry.ToLower().Contains(industry));
            if (!string.IsNullOrEmpty(businessUnit))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.BusinessUnit) && m.BusinessUnit.ToLower().Contains(businessUnit));
            if (!string.IsNullOrEmpty(ownership))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Ownership) && m.Ownership.ToLower().Contains(ownership));
            if (financialYear > 0)
                queries = queries.Where(m => m.FinancialYear > 0 && m.FinancialYear == financialYear);
            return queries.Include(m => m.Country).Include(m => m.ManagerUsers).Count();
        }

        public IEnumerable<Company> GetAllCompanies(string companyName, string countryName, string productService,
            string sector, int tier, string industry, string businessUnit, string ownership, int financialYear,
            string sortColumnDir, string sortColumn, int page, int pageSize)
        {

            IQueryable<Company> queries = _db.Set<Company>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(companyName))
                queries = queries.Where(m => m.CompanyName.ToLower().Contains(companyName));
            if (!string.IsNullOrEmpty(countryName))
                queries = queries.Where(m => m.Country != null && (m.Country.Code.ToLower().Contains(countryName) ||
                                                                   m.Country.Name.ToLower().Contains(countryName)));
            if (!string.IsNullOrEmpty(productService))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.ProductOrService) && m.ProductOrService.ToLower().Contains(productService));
            if (!string.IsNullOrEmpty(sector))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Sector) && m.Sector.ToLower().Contains(sector));
            if (tier > 0)
                queries = queries.Where(m => m.Tier == tier);
            if (!string.IsNullOrEmpty(industry))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Industry) && m.Industry.ToLower().Contains(industry));
            if (!string.IsNullOrEmpty(businessUnit))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.BusinessUnit) && m.BusinessUnit.ToLower().Contains(businessUnit));
            if (!string.IsNullOrEmpty(ownership))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Ownership) && m.Ownership.ToLower().Contains(ownership));
            if (financialYear > 0)
                queries = queries.Where(m => m.FinancialYear > 0 && m.FinancialYear == financialYear);
            switch (sortColumn)
            {
                case "CountryName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Country.Name)
                        : queries.OrderByDescending(s => s.Country.Name);
                    break;
                case "ProductOrService":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ProductOrService)
                        : queries.OrderByDescending(s => s.ProductOrService);
                    break;
                case "Sector":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Sector)
                        : queries.OrderByDescending(s => s.Sector);
                    break;
                case "Industry":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Industry)
                        : queries.OrderByDescending(s => s.Industry);
                    break;
                case "Tier":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Tier)
                        : queries.OrderByDescending(s => s.Tier);
                    break;
                case "BusinessUnit":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.BusinessUnit)
                        : queries.OrderByDescending(s => s.BusinessUnit);
                    break;
                case "FinancialYear":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FinancialYear)
                        : queries.OrderByDescending(s => s.FinancialYear);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.CompanyName)
                        : queries.OrderByDescending(s => s.CompanyName);
                    break;
            }
            return queries.Skip(page).Take(pageSize).Include(m=>m.Country).Include(m => m.ManagerUsers)
                .ToList();
        }


        public virtual IEnumerable<Company> GetCompaniesForAjaxDropdown(int id, string q)
        {
            q = q?.ToUpper().Trim() ?? "";
            return _db.Set<Company>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(m => m.CompanyName.ToUpper().Trim().Contains(q)
                                                 && m.ID != id).ToList();
        }

        public int GetCountCompaniesForAssignEvent(int tier, int[] saleIds, string companyName,
            string[] countries, string[] productServices, string[] sectors, string[] industries)
        {
            IQueryable<Company> queries = _db.Set<Company>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (tier > 0)
                queries = queries.Where(m => m.Tier == tier);
            if (saleIds.Any())
            {
                queries = queries.Where(m => !m.ManagerUsers.Any() || m.ManagerUsers.Any(s => saleIds.Contains(s.ID)));
            }
            if (!string.IsNullOrEmpty(companyName))
                queries = queries.Where(m => m.CompanyName.ToLower().Contains(companyName));
            if (countries.Any())
            {
                var countryIds = _db.Set<Country>().Where(m => countries.Any(c => m.Code.ToLower().Contains(c)) ||
                                                               countries.Any(c => m.Name.ToLower().Contains(c))).Select(m => m.ID).ToArray();
                queries = queries.Where(m => m.CountryID != null && countryIds.Contains((int)m.CountryID));
            }
            if (productServices.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.ProductOrService) &&
                                             productServices.Any(c => m.ProductOrService.ToLower().Contains(c)));
            if (sectors.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Sector) && sectors.Any(c => m.Sector.ToLower().Contains(c)));
            if (industries.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Industry) && industries.Any(c => m.Industry.ToLower().Contains(c)));
            return queries.Include(m => m.Country).Include(m => m.ManagerUsers).Count();
        }
        public IEnumerable<Company> GetAllCompaniesForAssignEvent(int tier, int[] saleIds, string companyName,
            string[] countries, string[] productServices, string[] sectors, string[] industries,
            string sortColumnDir, string sortColumn, int page, int pageSize)
        {

            IQueryable<Company> queries = _db.Set<Company>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (tier > 0)
                queries = queries.Where(m => m.Tier == tier);
            if (saleIds.Any())
            {
                queries = queries.Where(m => !m.ManagerUsers.Any() || m.ManagerUsers.Any(s => saleIds.Contains(s.ID)));
            }
            if (!string.IsNullOrEmpty(companyName))
                queries = queries.Where(m => m.CompanyName.ToLower().Contains(companyName));
            if (countries.Any())
            {
                var countryIds = _db.Set<Country>().Where(m => countries.Any(c => m.Code.ToLower().Contains(c)) ||
                                                               countries.Any(c => m.Name.ToLower().Contains(c))).Select(m => m.ID).ToArray();
                queries = queries.Where(m => m.CountryID != null && countryIds.Contains((int)m.CountryID));
            }
            if (productServices.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.ProductOrService) &&
                                             productServices.Any(c => m.ProductOrService.ToLower().Contains(c)));
            if (sectors.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Sector) && sectors.Any(c => m.Sector.ToLower().Contains(c)));
            if (industries.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Industry) && industries.Any(c => m.Industry.ToLower().Contains(c)));
            switch (sortColumn)
            {
                case "CountryName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Country.Name)
                        : queries.OrderByDescending(s => s.Country.Name);
                    break;
                case "ProductOrService":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ProductOrService)
                        : queries.OrderByDescending(s => s.ProductOrService);
                    break;
                case "Sector":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Sector)
                        : queries.OrderByDescending(s => s.Sector);
                    break;
                case "Industry":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Industry)
                        : queries.OrderByDescending(s => s.Industry);
                    break;
                case "Tier":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Tier)
                        : queries.OrderByDescending(s => s.Tier);
                    break;
                case "BusinessUnit":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.BusinessUnit)
                        : queries.OrderByDescending(s => s.BusinessUnit);
                    break;
                case "FinancialYear":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FinancialYear)
                        : queries.OrderByDescending(s => s.FinancialYear);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.CompanyName)
                        : queries.OrderByDescending(s => s.CompanyName);
                    break;
            }
            return queries.Skip(page).Take(pageSize).Include(m => m.Country).Include(m => m.ManagerUsers)
                .ToList();
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
            return _db.Set<Company>().FirstOrDefault(m => m.EntityStatus.Value == EntityStatus.Normal.Value && m.ID == companyID);
            //return Get<Company>(m => m.ID == companyID, m => new
            //{
            //    m.Country,
            //    m.ManagerUsers,
            //});
        }
        public virtual Company GetCompanyInDb(int companyID)
        {
            return _db.Set<Company>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                .Where(m => m.ID == companyID)
                .Include(m => m.Country)
                .Include(m => m.ManagerUsers)
                .FirstOrDefault();
            //return Get<Company>(m => m.ID == companyID, m => new
            //{
            //    m.Country,
            //    m.ManagerUsers,
            //});
        }

        public virtual Company GetCompany(string name)
        {
            name = name?.Trim().ToLower() ?? "";
            return _db.Set<Company>()
                .Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                .Where(m => m.CompanyName.Trim().ToLower() == name)
                .Include(m => m.Country)
                .Include(m => m.ManagerUsers)
                .FirstOrDefault();
            //if (string.IsNullOrEmpty(name))
            //{
            //    return null;
            //}
            //return Get<Company>(m => m.CompanyName.Trim().ToLower() == name.Trim().ToLower(), m => new
            //{
            //    m.Country,
            //    m.ManagerUsers,
            //});
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
            var exist = _db.Set<Company>().FirstOrDefault(m => m.ID == company.ID);
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
                var allResources = _db.Set<CompanyResource>().Where(m => m.CompanyID == company.ID).AsEnumerable();
                foreach (var companyResource in allResources)
                {
                    companyResource.Organisation = exist.CompanyName;
                    companyResource.CountryID = exist.CountryID;
                    Update(companyResource);
                }
                exist.Country = Get<Country>(Convert.ToInt32(company.CountryID));
                return exist;
            }
            return null;
        }

        public virtual Company MergeCompany(int companyID, int mergeCompanyID)
        {
            var com = _db.Set<Company>().FirstOrDefault(m => m.ID == companyID);
            if (com == null) return null;
            var mergeCom = _db.Set<Company>().FirstOrDefault(m => m.ID == mergeCompanyID);
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
                var resources = _db.Set<CompanyResource>().Where(m => m.CompanyID == companyID || m.CompanyID == mergeCompanyID).AsEnumerable();
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

        public int GetAllCompanyResourcesCheckPhone(int id, string directLine, string mobilePhone1,
            string mobilePhone2, string mobilePhone3)
        {
            IQueryable<CompanyResource> queries = _db.Set<CompanyResource>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(directLine))
                queries = queries.Where(m => m.ID != id &&
                                             !string.IsNullOrEmpty(m.DirectLine) &&
                                             m.DirectLine == directLine);
            if (!string.IsNullOrEmpty(mobilePhone1))
                queries = queries.Where(m => m.ID != id &&
                                             !string.IsNullOrEmpty(m.MobilePhone1) &&
                                             m.MobilePhone1 == mobilePhone1);
            if (!string.IsNullOrEmpty(mobilePhone2))
                queries = queries.Where(m => m.ID != id &&
                                             !string.IsNullOrEmpty(m.MobilePhone2) &&
                                             m.MobilePhone2 == mobilePhone2);
            if (!string.IsNullOrEmpty(mobilePhone3))
                queries = queries.Where(m => m.ID != id &&
                                             !string.IsNullOrEmpty(m.MobilePhone3) &&
                                             m.MobilePhone3 == mobilePhone3);
            return queries.Count();
        }
        public CompanyResource GetAllCompanyResourcesCheckPhoneForMerge(string role, string directLine, string mobilePhone1,
            string mobilePhone2, string mobilePhone3)
        {
            IQueryable<CompanyResource> queries = _db.Set<CompanyResource>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            queries = queries.Where(m =>
               m.Role == role && (
                   (!string.IsNullOrEmpty(m.DirectLine) && !string.IsNullOrEmpty(directLine) &&
                    m.DirectLine == directLine) ||
                   (!string.IsNullOrEmpty(m.MobilePhone1) && !string.IsNullOrEmpty(mobilePhone1) &&
                    m.MobilePhone1 == mobilePhone1) ||
                   (!string.IsNullOrEmpty(m.MobilePhone2) && !string.IsNullOrEmpty(mobilePhone2) &&
                    m.MobilePhone2 == mobilePhone2) ||
                   (!string.IsNullOrEmpty(m.MobilePhone3) && !string.IsNullOrEmpty(mobilePhone3) &&
                    m.MobilePhone3 == mobilePhone3)));
            return queries.FirstOrDefault();
        }

        public IEnumerable<CompanyResource> GetAllCompanyResources(int comId, string name, string role,
            string email, string phone, string searchValue)
        {
            IQueryable<CompanyResource> queries = _db.Set<CompanyResource>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(m => m.CompanyID == comId);
            if (!string.IsNullOrEmpty(name))
                queries = queries.Where(m => m.FirstName.ToLower().Contains(name) || m.LastName.ToLower().Contains(name));
            if (!string.IsNullOrEmpty(role))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Role) && m.Role.ToLower().Contains(role));
            if (!string.IsNullOrEmpty(email))
                queries = queries.Where(m => (!string.IsNullOrEmpty(m.WorkEmail) && m.WorkEmail.ToLower().Contains(email)) ||
                                             (!string.IsNullOrEmpty(m.PersonalEmail) && m.PersonalEmail.ToLower().Contains(email)));
            if (!string.IsNullOrEmpty(phone))
                queries = queries.Where(m => (!string.IsNullOrEmpty(m.MobilePhone1) && m.MobilePhone1.ToLower().Contains(phone)) ||
                                             (!string.IsNullOrEmpty(m.MobilePhone2) && m.MobilePhone2.ToLower().Contains(phone)) ||
                                             (!string.IsNullOrEmpty(m.MobilePhone3) && m.MobilePhone3.ToLower().Contains(phone)) ||
                                             (!string.IsNullOrEmpty(m.DirectLine) && m.DirectLine.ToLower().Contains(phone)));
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m =>
                     (!string.IsNullOrEmpty(m.Salutation) && m.Salutation.ToLower().Contains(searchValue)) ||
                     (!string.IsNullOrEmpty(m.FirstName) && m.FirstName.ToLower().Contains(searchValue)) ||
                     (!string.IsNullOrEmpty(m.LastName) && m.LastName.ToLower().Contains(searchValue)) ||
                     (!string.IsNullOrEmpty(m.DirectLine) && m.DirectLine.ToLower().Contains(searchValue)) ||
                     (!string.IsNullOrEmpty(m.MobilePhone1) && m.MobilePhone1.ToLower().Contains(searchValue)) ||
                     (!string.IsNullOrEmpty(m.MobilePhone2) && m.MobilePhone2.ToLower().Contains(searchValue)) ||
                     (!string.IsNullOrEmpty(m.MobilePhone3) && m.MobilePhone3.ToLower().Contains(searchValue)) ||
                     (!string.IsNullOrEmpty(m.WorkEmail) && m.WorkEmail.ToLower().Contains(searchValue)) ||
                     (!string.IsNullOrEmpty(m.PersonalEmail) && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                     (!string.IsNullOrEmpty(m.Role) && m.Role.ToLower().Contains(searchValue)));
            }
            return queries.ToList();
        }
        public IEnumerable<CompanyResource> GetAllCompanyResources(string[] countries, string[] organisations, string[] roles, string[] name, string[] email, string[] phone)
        {
            IQueryable<CompanyResource> queries = _db.Set<CompanyResource>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (countries.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Country) && countries.Any(c => m.Country.ToLower().Contains(c)));
            if (organisations.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Organisation) && organisations.Any(c => m.Organisation.ToLower().Contains(c)));
            if (name.Any())
                queries = queries.Where(m => name.Any(c => m.FullName.ToLower().Contains(c)));
            if (email.Any())
                queries = queries.Where(m =>
                    (!string.IsNullOrEmpty(m.WorkEmail) && email.Any(c => m.WorkEmail.ToLower().Contains(c))) ||
                    (!string.IsNullOrEmpty(m.PersonalEmail) && email.Any(c => m.PersonalEmail.ToLower().Contains(c))));
            if (phone.Any())
                queries = queries.Where(m =>
                    (!string.IsNullOrEmpty(m.DirectLine) && phone.Any(c => m.DirectLine.Contains(c))) ||
                    (!string.IsNullOrEmpty(m.MobilePhone1) && phone.Any(c => m.MobilePhone1.Contains(c))) ||
                    (!string.IsNullOrEmpty(m.MobilePhone2) && phone.Any(c => m.MobilePhone2.Contains(c))) ||
                    (!string.IsNullOrEmpty(m.MobilePhone3) && phone.Any(c => m.MobilePhone3.Contains(c))));
            return queries.ToList();
        }

        public int GetCountCompanyResources(string[] countries, string[] organisations, string[] roles, string[] name, string[] email, string[] phone)
        {
            IQueryable<CompanyResource> queries = _db.Set<CompanyResource>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (countries.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Country) && countries.Any(c => m.Country.ToLower().Contains(c)));
            if (organisations.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Organisation) && organisations.Any(c => m.Organisation.ToLower().Contains(c)));
            if (roles.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Role) && roles.Any(c => m.Role.ToLower().Contains(c)));
            if (name.Any())
                queries = queries.Where(m => name.Any(c => m.FullName.ToLower().Contains(c)));
            if (email.Any())
                queries = queries.Where(m =>
                    (!string.IsNullOrEmpty(m.WorkEmail) && email.Any(c => m.WorkEmail.ToLower().Contains(c))) ||
                    (!string.IsNullOrEmpty(m.PersonalEmail) && email.Any(c => m.PersonalEmail.ToLower().Contains(c))));
            if (phone.Any())
                queries = queries.Where(m =>
                    (!string.IsNullOrEmpty(m.DirectLine) && phone.Any(c => m.DirectLine.Contains(c))) ||
                    (!string.IsNullOrEmpty(m.MobilePhone1) && phone.Any(c => m.MobilePhone1.Contains(c))) ||
                    (!string.IsNullOrEmpty(m.MobilePhone2) && phone.Any(c => m.MobilePhone2.Contains(c))) ||
                    (!string.IsNullOrEmpty(m.MobilePhone3) && phone.Any(c => m.MobilePhone3.Contains(c))));
            return queries.Count();
        }
        public IEnumerable<CompanyResource> GetAllCompanyResources(string[] countries, string[] organisations, string[] roles, string[] name,
            string[] email, string[] phone, string sortColumnDir, string sortColumn, int page, int pageSize)
        {
            IQueryable<CompanyResource> queries = _db.Set<CompanyResource>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (countries.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Country) && countries.Any(c => m.Country.ToLower().Contains(c)));
            if (organisations.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Organisation) && organisations.Any(c => m.Organisation.ToLower().Contains(c)));
            if (roles.Any())
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Role) && roles.Any(c => m.Role.ToLower().Contains(c)));
            if (name.Any())
                queries = queries.Where(m => name.Any(c => m.FullName.ToLower().Contains(c)));
            if (email.Any())
                queries = queries.Where(m =>
                    (!string.IsNullOrEmpty(m.WorkEmail) && email.Any(c => m.WorkEmail.ToLower().Contains(c))) ||
                    (!string.IsNullOrEmpty(m.PersonalEmail) && email.Any(c => m.PersonalEmail.ToLower().Contains(c))));
            if (phone.Any())
                queries = queries.Where(m =>
                    (!string.IsNullOrEmpty(m.DirectLine) && phone.Any(c => m.DirectLine.Contains(c))) ||
                    (!string.IsNullOrEmpty(m.MobilePhone1) && phone.Any(c => m.MobilePhone1.Contains(c))) ||
                    (!string.IsNullOrEmpty(m.MobilePhone2) && phone.Any(c => m.MobilePhone2.Contains(c))) ||
                    (!string.IsNullOrEmpty(m.MobilePhone3) && phone.Any(c => m.MobilePhone3.Contains(c))));

            switch (sortColumn)
            {
                case "Country":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Country)
                        : queries.OrderByDescending(s => s.Country).ThenBy(s => s.Organisation);
                    break;
                case "LastName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LastName)
                        : queries.OrderByDescending(s => s.LastName).ThenBy(s => s.Organisation);
                    break;
                case "FirstName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FirstName)
                        : queries.OrderByDescending(s => s.FirstName).ThenBy(s => s.Organisation);
                    break;
                case "Role":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Role)
                        : queries.OrderByDescending(s => s.Role).ThenBy(s => s.Organisation);
                    break;
                case "DirectLine":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.DirectLine)
                        : queries.OrderByDescending(s => s.DirectLine).ThenBy(s => s.Organisation);
                    break;
                case "MobilePhone1":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone1)
                        : queries.OrderByDescending(s => s.MobilePhone1).ThenBy(s => s.Organisation);
                    break;
                case "MobilePhone2":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone2)
                        : queries.OrderByDescending(s => s.MobilePhone2).ThenBy(s => s.Organisation);
                    break;
                case "MobilePhone3":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone3)
                        : queries.OrderByDescending(s => s.MobilePhone3).ThenBy(s => s.Organisation);
                    break;
                case "WorkEmail":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.WorkEmail)
                        : queries.OrderByDescending(s => s.WorkEmail);
                    break;
                case "PersonalEmail":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.PersonalEmail)
                        : queries.OrderByDescending(s => s.PersonalEmail).ThenBy(s => s.Organisation);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Organisation)
                        : queries.OrderByDescending(s => s.Organisation);
                    break;
            }
            return queries.Skip(page).Take(pageSize)
                .ToList();
        }

        public IEnumerable<CompanyResource> GetAllCompanyResources(int[] comIds)
        {
            IQueryable<CompanyResource> queries = _db.Set<CompanyResource>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value).Where(m => m.CompanyID != null && comIds.Contains((int)m.CompanyID));
            return queries.ToList();
        }
        public int CountCompanyResources(int comId)
        {
            return _db.Set<CompanyResource>().Count(m => m.EntityStatus.Value == EntityStatus.Normal.Value && m.CompanyID == comId);
        }
        public virtual IEnumerable<CompanyResource> GetAllCompanyResources(Func<CompanyResource, bool> predicate)
        {
            return GetAll(predicate).AsEnumerable();
        }

        public virtual CompanyResource GetCompanyResource(int resourceId)
        {
            return _db.Set<CompanyResource>().FirstOrDefault(m => m.EntityStatus.Value == EntityStatus.Normal.Value && m.ID == resourceId);
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
