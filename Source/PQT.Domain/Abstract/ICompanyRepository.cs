using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface ICompanyRepository
    {
        #region FinanceCompany
        IEnumerable<Company> GetAllCompanies();
        IEnumerable<Company> GetAllCompanies(Func<Company, bool> predicate);
        Company GetCompany(int companyID);
        Company CreateCompany(Company company);
        bool UpdateCompany(Company company);
        bool DeleteCompany(int companyID);
        #endregion Company
    }
}
