using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IEventService
    {
        void RetrieveCacheEvents();
        IEnumerable<Event> GetAllEvents();
        IEnumerable<Event> GetAllEvents(Func<Event, bool> predicate);
        Event GetEvent(int id);
        Event GetEvent(string code);
        Event CreateEvent(Event info, IEnumerable<int> groups, IEnumerable<int> users);
        bool UpdateEvent(Event info);
        bool UpdateEventIncludeUpdateCollection(Event info, IEnumerable<int> groups, IEnumerable<int> users);
        bool AssignCompany(int id, IEnumerable<int> companies);
        bool DeleteEvent(int id);

        EventCompany GetEventCompany(int eventId, int companyId);
        EventCompany GetEventCompany(int companyId);
        EventCompany CreateEventCompany(EventCompany company);
        bool UpdateEventCompany(EventCompany company);
        void DeleteCompanyCache(Company info);
        void UpdateCompanyCache(Company info);
        void UpdateSalesGroupCache(SalesGroup info);
    }
}
