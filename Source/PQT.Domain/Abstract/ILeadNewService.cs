using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface ILeadNewService
    {
        IEnumerable<LeadNew> GetAllLeadNews(Func<LeadNew, bool> predicate);
        IEnumerable<LeadNew> GetAllLeadNewsForKPI(int eventId, int userId, DateTime dateFrom, DateTime dateTo, string searchValue);
        LeadNew GetLeadNew(int id);
        LeadNew CreateLeadNew(LeadNew info);
        bool UpdateLeadNew(LeadNew info);
        bool DeleteLeadNew(int id);

    }
}
