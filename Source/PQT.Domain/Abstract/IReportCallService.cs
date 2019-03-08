using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IReportCallService
    {
        IEnumerable<ReportCall> GetAllReportCalls();
        IEnumerable<ReportCall> GetAllReportCalls(Func<ReportCall, bool> predicate);
        ReportCall GetReportCall(int id);
        ReportCall CreateReportCall(ReportCall info);
        bool UpdateReportCall(ReportCall info);
        bool DeleteReportCall(int id);

    }
}
