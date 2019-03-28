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
        int GetCountLeaves(int userId, bool isSupervisor,string searchValue);
        IEnumerable<Leave> GetAllLeaves(int userId, bool isSupervisor, string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);

        int GetCountLeavesByMonthlyReport(DateTime month, int userId, bool isSupervisor, string searchValue);
        IEnumerable<Leave> GetAllLeavesByMonthlyReport(DateTime month, int userId, bool isSupervisor,
            string searchValue, string sortColumnDir,
            string sortColumn, int page, int pageSize);
        IEnumerable<Leave> GetAllLeavesMonthlyReport(DateTime month, int userId, bool isSupervisor, string searchValue);
        IEnumerable<Leave> GetAllLeavesForKpi(DateTime dateFrom, DateTime dateTo);
        Leave GetLeave(int id);
        Leave CreateLeave(Leave info);
        bool UpdateLeave(Leave info);
        bool DeleteLeave(int id);

        #endregion Leave
        #region NonSalesDay
        int GetCountNonSalesDays(string searchValue);
        IEnumerable<NonSalesDay> GetAllNonSalesDays(string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        IEnumerable<NonSalesDay> GetAllNonSalesDaysForKpi(DateTime dateFrom, DateTime dateTo);

        NonSalesDay GetNonSalesDay(int id);
        NonSalesDay GetNonSalesDayByMonth(DateTime month, int? userId);
        NonSalesDay CreateNonSalesDay(NonSalesDay info);
        bool UpdateNonSalesDay(NonSalesDay info);
        bool DeleteNonSalesDay(int id);


        #endregion NonSalesDay
        #region TechnicalIssueDay
        int GetCountTechnicalIssueDays(string searchValue);
        IEnumerable<TechnicalIssueDay> GetAllTechnicalIssueDays(string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        IEnumerable<TechnicalIssueDay> GetAllTechnicalIssueDaysForKpi(DateTime dateFrom, DateTime dateTo);
        TechnicalIssueDay GetTechnicalIssueDay(int id);
        TechnicalIssueDay GetTechnicalIssueDayByMonth(DateTime month, int? userId);
        TechnicalIssueDay CreateTechnicalIssueDay(TechnicalIssueDay info);
        bool UpdateTechnicalIssueDay(TechnicalIssueDay info);
        bool DeleteTechnicalIssueDay(int id);
        #endregion TechnicalIssueDay

    }
}
