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

        #region Companys

        public IEnumerable<Company> GetAllCompanies()
        {
            return GetAll<Company>();
        }

        public Company GetCompany(int companyID)
        {
            return Get<Company>(companyID);
        }

        public Company CreateCompany(Company company)
        {
            return Create(company);
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

        #endregion

    }
}
