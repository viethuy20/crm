using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using NS;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Concrete
{
    public class EFLeaveService : Repository, ILeaveService
    {
        public EFLeaveService(DbContext db)
            : base(db)
        {
        }
        #region Leave
        public int GetCountLeaves(int userId, bool isSupervisor, string searchValue)
        {
            IQueryable<Leave> queries = _db.Set<Leave>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (userId > 0)
            {
                if (isSupervisor)
                    queries = queries.Where(m => m.UserID == userId ||
                                                 m.User.DirectSupervisorID == userId);
                else
                    queries = queries.Where(m => m.UserID == userId);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.LeaveDateFrom == dtSearch || m.LeaveDateTo == dtSearch);
                else
                {
                    var searchLeaveTypes = Enumeration.GetAll<LeaveType>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchTypeOfLeaves = Enumeration.GetAll<TypeOfLeave>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchTypeOfLateness = Enumeration.GetAll<TypeOfLatenes>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        m.CreatedUser.DisplayName.ToLower().Contains(searchValue) ||
                        m.User.DisplayName.ToLower().Contains(searchValue) ||
                        (m.Summary != null && m.Summary.ToLower().Contains(searchValue)) ||
                        searchLeaveTypes.Contains(m.LeaveType.Value) ||
                        searchTypeOfLeaves.Contains(m.TypeOfLeave.Value) ||
                        searchTypeOfLateness.Contains(m.TypeOfLatenes.Value));
                }
            }
            return queries.Count();
        }

        public IEnumerable<Leave> GetAllLeaves(int userId, bool isSupervisor, string searchValue, string sortColumnDir,
            string sortColumn, int page, int pageSize)
        {
            IQueryable<Leave> queries = _db.Set<Leave>().Where(m=>m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (userId > 0)
            {
                if (isSupervisor)
                    queries = queries.Where(m => m.UserID == userId ||
                                                          m.User.DirectSupervisorID == userId);
                else
                    queries = queries.Where(m => m.UserID == userId);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.LeaveDateFrom == dtSearch || m.LeaveDateTo == dtSearch);
                else
                {
                    var searchLeaveTypes = Enumeration.GetAll<LeaveType>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchTypeOfLeaves = Enumeration.GetAll<TypeOfLeave>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchTypeOfLateness = Enumeration.GetAll<TypeOfLatenes>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        m.CreatedUser.DisplayName.ToLower().Contains(searchValue) ||
                        m.User.DisplayName.ToLower().Contains(searchValue) ||
                        (m.Summary != null && m.Summary.ToLower().Contains(searchValue)) ||
                        searchLeaveTypes.Contains(m.LeaveType.Value) ||
                        searchTypeOfLeaves.Contains(m.TypeOfLeave.Value) ||
                        searchTypeOfLateness.Contains(m.TypeOfLatenes.Value));
                }
            }

            switch (sortColumn)
            {
                case "UserDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.User.DisplayName)
                        : queries.OrderByDescending(s => s.User.DisplayName);
                    break;
                case "LeaveDateDesc":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LeaveDateFrom)
                        : queries.OrderByDescending(s => s.LeaveDateFrom);
                    break;
                case "AprroveUserDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.CreatedUser.DisplayName)
                        : queries.OrderByDescending(s => s.CreatedUser.DisplayName);
                    break;
                case "LeaveType":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LeaveType.Value)
                        : queries.OrderByDescending(s => s.LeaveType.Value);
                    break;
                case "ReasonLeave":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ReasonLeave)
                        : queries.OrderByDescending(s => s.ReasonLeave);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize)
                .Include(m => m.User)
                .Include(m => m.CreatedUser)
                .ToList();
        }
        public int GetCountLeavesByMonthlyReport(DateTime month, int userId, bool isSupervisor, string searchValue)
        {
            var monthInt = month.Month;
            var yearInt = month.Year;
            IQueryable<Leave> queries = _db.Set<Leave>()
                .Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value &&
                            m.LeaveDateFrom.Month == monthInt &&
                            m.LeaveDateFrom.Year == yearInt ||
                            m.LeaveDateTo.Month == monthInt &&
                            m.LeaveDateTo.Year == yearInt);
            if (userId > 0)
            {
                if (isSupervisor)
                    queries = queries.Where(m => m.UserID == userId ||
                                                          m.User.DirectSupervisorID == userId);
                else
                    queries = queries.Where(m => m.UserID == userId);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.LeaveDateFrom == dtSearch || m.LeaveDateTo == dtSearch);
                else
                {
                    var searchLeaveTypes = Enumeration.GetAll<LeaveType>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchTypeOfLeaves = Enumeration.GetAll<TypeOfLeave>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchTypeOfLateness = Enumeration.GetAll<TypeOfLatenes>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m => m.User.DisplayName.ToLower().Contains(searchValue) ||
                                                 (m.Summary != null && m.Summary.ToLower().Contains(searchValue)) ||
                                                 searchLeaveTypes.Contains(m.LeaveType.Value) ||
                                                 searchTypeOfLeaves.Contains(m.TypeOfLeave.Value) ||
                                                 searchTypeOfLateness.Contains(m.TypeOfLatenes.Value));
                }
            }
            return queries.Count();
        }
        public IEnumerable<Leave> GetAllLeavesByMonthlyReport(DateTime month, int userId, bool isSupervisor, string searchValue, string sortColumnDir,
            string sortColumn, int page, int pageSize)
        {
            var monthInt = month.Month;
            var yearInt = month.Year;
            IQueryable<Leave> queries = _db.Set<Leave>()
                .Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value &&
                            m.LeaveDateFrom.Month == monthInt &&
                            m.LeaveDateFrom.Year == yearInt ||
                            m.LeaveDateTo.Month == monthInt &&
                            m.LeaveDateTo.Year == yearInt);
            if (userId > 0)
            {
                if (isSupervisor)
                    queries = queries.Where(m => m.UserID == userId ||
                                                          m.User.DirectSupervisorID == userId);
                else
                    queries = queries.Where(m => m.UserID == userId);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.LeaveDateFrom == dtSearch || m.LeaveDateTo == dtSearch);
                else
                {
                    var searchLeaveTypes = Enumeration.GetAll<LeaveType>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchTypeOfLeaves = Enumeration.GetAll<TypeOfLeave>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchTypeOfLateness = Enumeration.GetAll<TypeOfLatenes>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m => m.User.DisplayName.ToLower().Contains(searchValue) ||
                                                 (m.Summary != null && m.Summary.ToLower().Contains(searchValue)) ||
                                                 searchLeaveTypes.Contains(m.LeaveType.Value) ||
                                                 searchTypeOfLeaves.Contains(m.TypeOfLeave.Value) ||
                                                 searchTypeOfLateness.Contains(m.TypeOfLatenes.Value));
                }
            }

            switch (sortColumn)
            {
                case "UserDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.User.DisplayName)
                        : queries.OrderByDescending(s => s.User.DisplayName);
                    break;
                case "LeaveDateDesc":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LeaveDateFrom)
                        : queries.OrderByDescending(s => s.LeaveDateFrom);
                    break;
                case "AprroveUserDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.CreatedUser.DisplayName)
                        : queries.OrderByDescending(s => s.CreatedUser.DisplayName);
                    break;
                case "LeaveType":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LeaveType.Value)
                        : queries.OrderByDescending(s => s.LeaveType.Value);
                    break;
                case "ReasonLeave":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ReasonLeave)
                        : queries.OrderByDescending(s => s.ReasonLeave);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize)
                .Include(m => m.User)
                .Include(m => m.CreatedUser)
                .ToList();
        }
        public IEnumerable<Leave> GetAllLeavesMonthlyReport(DateTime month, int userId, bool isSupervisor, string searchValue)
        {
            var monthInt = month.Month;
            var yearInt = month.Year;
            var queries = _db.Set<Leave>()
                .Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value &&
                            m.LeaveDateFrom.Month == monthInt &&
                            m.LeaveDateFrom.Year == yearInt ||
                            m.LeaveDateTo.Month == monthInt &&
                            m.LeaveDateTo.Year == yearInt);
            if (userId > 0)
            {
                if (isSupervisor)
                    queries = queries.Where(m => m.UserID == userId || m.User.DirectSupervisorID == userId);
                else
                    queries = queries.Where(m => m.UserID == userId);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m => m.User.DisplayName.ToLower().Contains(searchValue));
            }

            return queries
                .Include(m => m.User)
                .ToList();
        }
        public IEnumerable<Leave> GetAllLeavesForKpi(DateTime dateFrom, DateTime dateTo)
        {
            var leaveTypeValue = LeaveType.Leave.Value;
            return _db.Set<Leave>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value &&
                                               ((m.LeaveDateFrom >= dateFrom &&
                                               m.LeaveDateFrom <= dateTo) ||
                                                (m.LeaveDateTo >= dateFrom &&
                                                 m.LeaveDateTo <= dateTo)) &&
                                               m.LeaveType.Value == leaveTypeValue).ToList();
        }

        public Leave GetLeave(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<Leave>().Where(m => m.ID == id)
                .Include(m => m.User)
                .Include(m => m.CreatedUser)
                .FirstOrDefault();
        }

        public Leave CreateLeave(Leave info)
        {
            info = Create(info);
            return info;
        }

        public bool UpdateLeave(Leave info)
        {
            return Update(info);
        }
        public bool DeleteLeave(int id)
        {
            return Delete<Leave>(id);
        }


        #endregion Leave
        #region NonSalesDay


        public int GetCountNonSalesDays(string searchValue)
        {
            IQueryable<NonSalesDay> queries = _db.Set<NonSalesDay>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (string.IsNullOrEmpty(searchValue)) return queries.Count();
            bool isValid = DateTime.TryParseExact(
                searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dtSearch);
            if (isValid)
                queries = queries.Where(m => m.IssueMonth == dtSearch);
            else
                queries = queries.Where(m => m.User != null && m.User.DisplayName.ToLower().Contains(searchValue));
            return queries.Count();
        }

        public IEnumerable<NonSalesDay> GetAllNonSalesDays(string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize)
        {
            IQueryable<NonSalesDay> queries = _db.Set<NonSalesDay>().Where(m=> m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "MMM yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                {
                    queries = queries.Where(m => m.IssueMonth == dtSearch);
                }
                else
                {
                    queries = queries.Where(m => m.User != null && m.User.DisplayName.ToLower().Contains(searchValue) ||
                                                 m.Remarks != null && m.Remarks.ToLower().Contains(searchValue));
                }
            }
            switch (sortColumn)
            {
                case "UserDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.User.DisplayName)
                        : queries.OrderByDescending(s => s.User.DisplayName);
                    break;
                case "IssueMonthDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.IssueMonth)
                        : queries.OrderByDescending(s => s.IssueMonth);
                    break;
                case "NonSalesDays":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.NonSalesDays)
                        : queries.OrderByDescending(s => s.NonSalesDays);
                    break;
                case "Remarks":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Remarks)
                        : queries.OrderByDescending(s => s.Remarks);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize)
                .Include(m => m.User)
                .ToList();
        }

        public IEnumerable<NonSalesDay> GetAllNonSalesDaysForKpi(DateTime dateFrom, DateTime dateTo)
        {
            var dateFromMonth = new DateTime(dateFrom.Year, dateFrom.Month, 1);
            var dateToMonth = new DateTime(dateTo.Year, dateTo.Month, 1);
            return _db.Set<NonSalesDay>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                .Where(m => m.IssueMonth >= dateFromMonth &&
                            m.IssueMonth <= dateToMonth).ToList();
        }
        public NonSalesDay GetNonSalesDay(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<NonSalesDay>()
                .Where(m => m.ID == id)
                .Include(m => m.User)
                .FirstOrDefault();
        }

        public NonSalesDay GetNonSalesDayByMonth(DateTime month, int? userId)
        {
            return _db.Set<NonSalesDay>()
                .FirstOrDefault(m => m.IssueMonth == month && m.UserID == userId);
        }

        public NonSalesDay CreateNonSalesDay(NonSalesDay info)
        {
            info = Create(info);
            return info;
        }

        public bool UpdateNonSalesDay(NonSalesDay info)
        {
            return Update(info);
        }
        public bool DeleteNonSalesDay(int id)
        {
            return Delete<NonSalesDay>(id);
        }


        #endregion NonSalesDay

        #region TechnicalIssueDay

        public int GetCountTechnicalIssueDays(string searchValue)
        {
            IQueryable<TechnicalIssueDay> queries = _db.Set<TechnicalIssueDay>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (string.IsNullOrEmpty(searchValue)) return queries.Count();
            bool isValid = DateTime.TryParseExact(
                searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dtSearch);
            if (isValid)
                queries = queries.Where(m => m.IssueMonth == dtSearch);
            else
                queries = queries.Where(m => m.User != null && m.User.DisplayName.ToLower().Contains(searchValue));
            return queries.Count();
        }

        public IEnumerable<TechnicalIssueDay> GetAllTechnicalIssueDays(string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize)
        {
            IQueryable<TechnicalIssueDay> queries = _db.Set<TechnicalIssueDay>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "MMM yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                {
                    queries = queries.Where(m => m.IssueMonth == dtSearch);
                }
                else
                {
                    queries = queries.Where(m => m.User != null && m.User.DisplayName.ToLower().Contains(searchValue) ||
                                                 m.Remarks != null && m.Remarks.ToLower().Contains(searchValue));
                }
            }
            switch (sortColumn)
            {
                case "UserDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.User.DisplayName)
                        : queries.OrderByDescending(s => s.User.DisplayName);
                    break;
                case "IssueMonthDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.IssueMonth)
                        : queries.OrderByDescending(s => s.IssueMonth);
                    break;
                case "TechnicalIssueDays":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.TechnicalIssueDays)
                        : queries.OrderByDescending(s => s.TechnicalIssueDays);
                    break;
                case "Remarks":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Remarks)
                        : queries.OrderByDescending(s => s.Remarks);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize)
                .Include(m => m.User)
                .ToList();
        }
        public IEnumerable<TechnicalIssueDay> GetAllTechnicalIssueDaysForKpi(DateTime dateFrom, DateTime dateTo)
        {
            var dateFromMonth = new DateTime(dateFrom.Year, dateFrom.Month, 1);
            var dateToMonth = new DateTime(dateTo.Year, dateTo.Month, 1);
            return _db.Set<TechnicalIssueDay>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value)
                .Where(m => m.IssueMonth >= dateFromMonth &&
                            m.IssueMonth <= dateToMonth).ToList();
        }
        public TechnicalIssueDay GetTechnicalIssueDay(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<TechnicalIssueDay>()
                .Where(m => m.ID == id)
                .Include(m => m.User).FirstOrDefault();
        }
        public TechnicalIssueDay GetTechnicalIssueDayByMonth(DateTime month, int? userId)
        {
            return _db.Set<TechnicalIssueDay>()
                .FirstOrDefault(m => m.IssueMonth == month && m.UserID == userId);
        }

        public TechnicalIssueDay CreateTechnicalIssueDay(TechnicalIssueDay info)
        {
            info = Create(info);
            return info;
        }

        public bool UpdateTechnicalIssueDay(TechnicalIssueDay info)
        {
            return Update(info);
        }
        public bool DeleteTechnicalIssueDay(int id)
        {
            return Delete<TechnicalIssueDay>(id);
        }
        #endregion TechnicalIssueDay
    }
}
