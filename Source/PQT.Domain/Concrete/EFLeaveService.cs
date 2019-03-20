using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Domain.Concrete
{
    public class EFLeaveService : Repository, ILeaveService
    {
        public EFLeaveService(DbContext db)
            : base(db)
        {
        }
        #region Leave
        public int GetCountLeaves(Func<Leave, bool> predicate)
        {
            if (predicate != null)
            {
                return _db.Set<Leave>()
                    .Include(m => m.User)
                    .Include(m => m.AprroveUser)
                    .Include(m => m.CreatedUser)
                    .Count(predicate);
            }
            return _db.Set<Leave>().Count();
        }

        public IEnumerable<Leave> GetAllLeaves(Func<Leave, bool> predicate, string sortColumnDir, Func<Leave, object> orderBy, int page, int pageSize)
        {
            if (predicate != null)
            {
                if (sortColumnDir == "asc")
                {
                    return _db.Set<Leave>()
                        .Include(m => m.User)
                        .Include(m => m.AprroveUser)
                        .Include(m => m.CreatedUser)
                        .Where(predicate).OrderBy(orderBy).ThenByDescending(s => s.ID)
                        .Skip(page).Take(pageSize).AsEnumerable();
                }
                return _db.Set<Leave>()
                    .Include(m => m.User)
                    .Include(m => m.AprroveUser)
                    .Include(m => m.CreatedUser)
                    .Where(predicate).OrderByDescending(orderBy).ThenByDescending(s => s.ID)
                    .Skip(page).Take(pageSize).AsEnumerable();
            }
            if (sortColumnDir == "asc")
            {
                return _db.Set<Leave>()
                    .Include(m => m.User)
                    .Include(m => m.AprroveUser)
                    .Include(m => m.CreatedUser)
                    .OrderBy(orderBy).ThenByDescending(s => s.ID)
                    .Skip(page).Take(pageSize).AsEnumerable();
            }
            return _db.Set<Leave>()
                .Include(m => m.User)
                .Include(m => m.AprroveUser)
                .Include(m => m.CreatedUser)
                .OrderByDescending(orderBy).ThenByDescending(s => s.ID)
                .Skip(page).Take(pageSize).AsEnumerable();
        }

        public IEnumerable<Leave> GetAllLeaves(Func<Leave, bool> predicate)
        {
            if (predicate != null)
                return _db.Set<Leave>()
                    .Include(m => m.User)
                    .Include(m => m.AprroveUser)
                    .Include(m => m.CreatedUser)
                    .Where(predicate).AsEnumerable();
            return _db.Set<Leave>()
                .Include(m => m.User)
                .Include(m => m.AprroveUser)
                .Include(m => m.CreatedUser)
                .AsEnumerable();
        }
        public IEnumerable<Leave> GetAllLeavesNotInclude(Func<Leave, bool> predicate)
        {
            if (predicate != null)
                return _db.Set<Leave>()
                    .Where(predicate).AsEnumerable();
            return _db.Set<Leave>()
                .AsEnumerable();
        }

        public Leave GetLeave(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<Leave>()
                .Include(m => m.User)
                .Include(m => m.AprroveUser)
                .Include(m => m.CreatedUser)
                .FirstOrDefault(m => m.ID == id);
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


        public int GetCountNonSalesDays(Func<NonSalesDay, bool> predicate)
        {
            if (predicate != null)
            {
                return _db.Set<NonSalesDay>().Include(m => m.User).Count(predicate);
            }
            return _db.Set<NonSalesDay>().Count();
        }

        public IEnumerable<NonSalesDay> GetAllNonSalesDays(Func<NonSalesDay, bool> predicate, string sortColumnDir, Func<NonSalesDay, object> orderBy, int page, int pageSize)
        {
            if (predicate != null)
            {
                if (sortColumnDir == "asc")
                {
                    return _db.Set<NonSalesDay>()
                        .Include(m => m.User)
                        .Where(predicate).OrderBy(orderBy).ThenByDescending(s => s.ID).Skip(page)
                        .Take(pageSize).AsEnumerable();
                }
                return _db.Set<NonSalesDay>()
                    .Include(m => m.User)
                    .Where(predicate).OrderByDescending(orderBy).ThenByDescending(s => s.ID).Skip(page)
                    .Take(pageSize).AsEnumerable();
            }
            if (sortColumnDir == "asc")
            {
                return _db.Set<NonSalesDay>()
                    .Include(m => m.User)
                    .OrderBy(orderBy).ThenByDescending(s => s.ID).Skip(page)
                    .Take(pageSize).AsEnumerable();
            }
            return _db.Set<NonSalesDay>()
                .Include(m => m.User)
                .OrderByDescending(orderBy).ThenByDescending(s => s.ID).Skip(page)
                .Take(pageSize).AsEnumerable();
        }

        public IEnumerable<NonSalesDay> GetAllNonSalesDays(Func<NonSalesDay, bool> predicate)
        {
            if (predicate != null)
            {
                return _db.Set<NonSalesDay>()
                    .Include(m => m.User)
                    .Where(predicate).AsEnumerable();
            }
            return _db.Set<NonSalesDay>()
                .Include(m => m.User).AsEnumerable();
        }
        public IEnumerable<NonSalesDay> GetAllNonSalesDaysNotInclude(Func<NonSalesDay, bool> predicate)
        {
            if (predicate != null)
            {
                return _db.Set<NonSalesDay>()
                    .Where(predicate).AsEnumerable();
            }
            return _db.Set<NonSalesDay>().AsEnumerable();
        }
        public NonSalesDay GetNonSalesDay(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<NonSalesDay>()
                .Include(m => m.User)
                .FirstOrDefault(m => m.ID == id);
        }

        public NonSalesDay GetNonSalesDayByMonth(DateTime month)
        {
            return _db.Set<NonSalesDay>()
                .Include(m => m.User)
                .FirstOrDefault(m => m.IssueMonth == month);
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
        public int GetCountTechnicalIssueDays(Func<TechnicalIssueDay, bool> predicate)
        {
            if (predicate != null)
            {
                return _db.Set<TechnicalIssueDay>().Include(m => m.User).Count(predicate);
            }
            return _db.Set<TechnicalIssueDay>().Count();
        }

        public IEnumerable<TechnicalIssueDay> GetAllTechnicalIssueDays(Func<TechnicalIssueDay, bool> predicate, string sortColumnDir, Func<TechnicalIssueDay, object> orderBy, int page, int pageSize)
        {
            if (predicate != null)
            {
                if (sortColumnDir == "asc")
                {
                    return _db.Set<TechnicalIssueDay>()
                        .Include(m => m.User)
                        .Where(predicate).OrderBy(orderBy).ThenByDescending(s => s.ID).Skip(page)
                        .Take(pageSize).AsEnumerable();
                }
                return _db.Set<TechnicalIssueDay>()
                    .Include(m => m.User)
                    .Where(predicate).OrderByDescending(orderBy).ThenByDescending(s => s.ID).Skip(page)
                    .Take(pageSize).AsEnumerable();
            }
            if (sortColumnDir == "asc")
            {
                return _db.Set<TechnicalIssueDay>()
                    .Include(m => m.User)
                    .OrderBy(orderBy).ThenByDescending(s => s.ID).Skip(page)
                    .Take(pageSize).AsEnumerable();
            }
            return _db.Set<TechnicalIssueDay>()
                .Include(m => m.User)
                .OrderByDescending(orderBy).ThenByDescending(s => s.ID).Skip(page)
                .Take(pageSize).AsEnumerable();
        }

        public IEnumerable<TechnicalIssueDay> GetAllTechnicalIssueDays(Func<TechnicalIssueDay, bool> predicate)
        {
            if (predicate != null)
            {
                return _db.Set<TechnicalIssueDay>()
                    .Include(m => m.User)
                    .Where(predicate).AsEnumerable();
            }
            return _db.Set<TechnicalIssueDay>()
                .Include(m => m.User).AsEnumerable();
        }
        public IEnumerable<TechnicalIssueDay> GetAllTechnicalIssueDaysNotInclude(Func<TechnicalIssueDay, bool> predicate)
        {
            if (predicate != null)
            {
                return _db.Set<TechnicalIssueDay>()
                    .Where(predicate).AsEnumerable();
            }
            return _db.Set<TechnicalIssueDay>().AsEnumerable();
        }
        public TechnicalIssueDay GetTechnicalIssueDay(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<TechnicalIssueDay>()
                .Include(m => m.User)
                .FirstOrDefault(m => m.ID == id);
        }
        public TechnicalIssueDay GetTechnicalIssueDayByMonth(DateTime month)
        {
            return _db.Set<TechnicalIssueDay>()
                .Include(m => m.User)
                .FirstOrDefault(m => m.IssueMonth == month);
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
