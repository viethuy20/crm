using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using PQT.Domain.Abstract;
using PQT.Domain.Concrete;
using PQT.Domain.Entities;

namespace PQT.Web.Infrastructure
{
    public class MemoryCompanyRepository : EFCompanyRepository
    {
        private List<Company> _companies = new List<Company>();
        private List<CompanyResource> _companyResoures = new List<CompanyResource>();

        #region Factory

        public MemoryCompanyRepository(DbContext db)
            : base(db)
        {
            RetrieveCacheCompanies();
            RetrieveCacheCompanyResources();
        }

        #endregion

        #region Decorator Properties

        public EFCompanyRepository CompanyRepository
        {
            get { return DependencyHelper.GetService<EFCompanyRepository>(); }
        }
        public IEventService EventService
        {
            get { return DependencyHelper.GetService<IEventService>(); }
        }

        #endregion

        private void RetrieveCacheCompanies()
        {
            _companies.Clear();
            _companies.AddRange(CompanyRepository.GetAllCompanies());
        }

        private void RetrieveCacheCompanyResources()
        {
            _companyResoures.Clear();
            _companyResoures.AddRange(CompanyRepository.GetAllCompanyResources());
        }

        #region Company

        public override IEnumerable<Company> GetAllCompanies()
        {
            return _companies.AsEnumerable();
        }
        public override IEnumerable<Company> GetAllCompanies(Func<Company, bool> predicate)
        {
            return _companies.Where(predicate).AsEnumerable();
        }

        public override Company GetCompany(int companyID)
        {
            return _companies.FirstOrDefault(m => m.ID == companyID);
        }
        public override Company GetCompanyInDb(int companyID)
        {
            return CompanyRepository.GetCompanyInDb(companyID);
        }

        public override Company GetCompany(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            return _companies.FirstOrDefault(m => m.CompanyName.Trim().ToLower() == name.Trim().ToLower());
        }

        public override Company CreateCompany(Company company, IEnumerable<int> users)
        {
            company = CompanyRepository.CreateCompany(company, users);
            var com = new Company(company);
            _companies.Add(com);
            return com;
        }

        //public override List<Company> CreateCompanies(List<Company> companies)
        //{
        //    var menus = CompanyRepository.CreateCompanies(companies);
        //    _companies.AddRange(menus.Select(m => new Company(CompanyRepository.GetCompany(m.ID))));
        //    return menus;
        //}
        public override bool UpdateCompany(Company company)
        {
            if (!CompanyRepository.UpdateCompany(company)) return false;
            _companies.Remove(GetCompany(company.ID));
            var reTryCom = new Company(company);
            _companies.Add(reTryCom);
            EventService.UpdateCompanyCache(reTryCom);
            return true;
        }
        public override Company UpdateCompany(Company company, IEnumerable<int> users)
        {
            var com = CompanyRepository.UpdateCompany(company, users);
            if (com != null)
            {
                var exist = GetCompany(company.ID);
                if (exist != null)
                {
                    _companies.Remove(exist);
                    var reTryCom = new Company(com);
                    _companies.Add(reTryCom);
                    EventService.UpdateCompanyCache(reTryCom);
                }
                else
                {
                    _companies.Add(new Company(com));
                }
            }
            return com;
        }

        public override bool DeleteCompany(int companyID)
        {
            if (CompanyRepository.DeleteCompany(companyID))
            {
                var com = GetCompany(companyID);
                _companies.Remove(com);
                EventService.DeleteCompanyCache(com);
                return true;
            }
            return false;
        }


        #endregion Company



        #region Company Resource
        public override IEnumerable<CompanyResource> GetAllCompanyResources()
        {
            return _companyResoures.AsEnumerable();
        }

        public override IEnumerable<CompanyResource> GetAllCompanyResources(Func<CompanyResource, bool> predicate)
        {
            return _companyResoures.Where(predicate).AsEnumerable();
        }

        public override CompanyResource GetCompanyResource(int resourceId)
        {
            return _companyResoures.FirstOrDefault(m => m.ID == resourceId);
        }

        public override CompanyResource CreateCompanyResource(CompanyResource resource)
        {
            var menu = CompanyRepository.CreateCompanyResource(resource);
            _companyResoures.Add(menu);
            return menu;
        }

        public override bool UpdateCompanyResource(CompanyResource resource)
        {
            if (!CompanyRepository.UpdateCompanyResource(resource)) return false;
            _companyResoures.Remove(GetCompanyResource(resource.ID));
            _companyResoures.Add(resource);
            return true;
        }

        public override bool DeleteCompanyResource(int resourceID)
        {
            if (CompanyRepository.DeleteCompanyResource(resourceID))
            {
                _companyResoures.Remove(GetCompanyResource(resourceID));
                return true;
            }
            return false;
        }

        #endregion Company Resource
    }
}
