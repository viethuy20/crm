using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface ILeaveService
    {
        #region Leave
        int GetCountLeaves(Func<Leave, bool> predicate);
        IEnumerable<Leave> GetAllLeaves(Func<Leave, bool> predicate, string sortColumnDir, Func<Leave, object> orderBy, int page, int pageSize);
        IEnumerable<Leave> GetAllLeaves(Func<Leave, bool> predicate);
        IEnumerable<Leave> GetAllLeavesNotInclude(Func<Leave, bool> predicate);
        Leave GetLeave(int id);
        Leave CreateLeave(Leave info);
        bool UpdateLeave(Leave info);
        bool DeleteLeave(int id);

        #endregion Leave
        #region NonSalesDay
        int GetCountNonSalesDays(Func<NonSalesDay, bool> predicate);
        IEnumerable<NonSalesDay> GetAllNonSalesDays(Func<NonSalesDay, bool> predicate, string sortColumnDir,Func<NonSalesDay, object> orderBy, int page, int pageSize);
        IEnumerable<NonSalesDay> GetAllNonSalesDays(Func<NonSalesDay, bool> predicate);
        IEnumerable<NonSalesDay> GetAllNonSalesDaysNotInclude(Func<NonSalesDay, bool> predicate);

        NonSalesDay GetNonSalesDay(int id);
        NonSalesDay GetNonSalesDayByMonth(DateTime month);
        NonSalesDay CreateNonSalesDay(NonSalesDay info);
        bool UpdateNonSalesDay(NonSalesDay info);
        bool DeleteNonSalesDay(int id);


        #endregion NonSalesDay
        #region TechnicalIssueDay
        int GetCountTechnicalIssueDays(Func<TechnicalIssueDay, bool> predicate);
        IEnumerable<TechnicalIssueDay> GetAllTechnicalIssueDays(Func<TechnicalIssueDay, bool> predicate, string sortColumnDir, Func<TechnicalIssueDay, object> orderBy, int page, int pageSize);
        IEnumerable<TechnicalIssueDay> GetAllTechnicalIssueDays(Func<TechnicalIssueDay, bool> predicate);
        IEnumerable<TechnicalIssueDay> GetAllTechnicalIssueDaysNotInclude(Func<TechnicalIssueDay, bool> predicate);

        TechnicalIssueDay GetTechnicalIssueDay(int id);
        TechnicalIssueDay GetTechnicalIssueDayByMonth(DateTime month);
        TechnicalIssueDay CreateTechnicalIssueDay(TechnicalIssueDay info);
        bool UpdateTechnicalIssueDay(TechnicalIssueDay info);
        bool DeleteTechnicalIssueDay(int id);
        #endregion TechnicalIssueDay

    }
}
