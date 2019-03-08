using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Concrete
{
    public class EFReportCallService : Repository, IReportCallService
    {
        public EFReportCallService(DbContext db)
            : base(db)
        {
        }

        public IEnumerable<ReportCall> GetAllReportCalls()
        {
            return GetAll<ReportCall>(m => m.User).AsEnumerable();
        }

        public IEnumerable<ReportCall> GetAllReportCalls(Func<ReportCall, bool> predicate)
        {
            Func<ReportCall, bool> predicate2 =
                m => predicate(m);
            return GetAll(predicate2, m => m.User).AsEnumerable();
        }

        public ReportCall GetReportCall(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return Get<ReportCall>(m => m.ID == id, u => new
            {
                u.User
            });
        }

        public ReportCall CreateReportCall(ReportCall info)
        {
            info = Create(info);
            return info;
        }

        public bool UpdateReportCall(ReportCall info)
        {
            return Update(info);
        }
        public bool DeleteReportCall(int id)
        {
            return Delete<ReportCall>(id);
        }
    }
}

