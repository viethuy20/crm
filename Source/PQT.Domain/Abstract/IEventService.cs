using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IEventService
    {
        int GetCountEvents(int salesUserId, string searchValue);
        IEnumerable<Event> GetAllEvents(int salesUserId, string searchValue, string sortColumnDir,
            string sortColumn, int page, int pageSize);
        int GetCountEventsForOpe(string searchValue);
        IEnumerable<Event> GetAllEventsForOpe(string searchValue, string sortColumnDir,
            string sortColumn, int page, int pageSize);
        IEnumerable<Event> GetAllEventsForDashboard(int userId,string searchValue);
        IEnumerable<Event> GetAllPossibleEvents(string searchValue);
        IEnumerable<Event> GetAllEvents();
        IEnumerable<Event> GetAllEvents(Func<Event, bool> predicate);

        int GetCountEventCompanies(int eventId, string companyName, string productService, string countryName, int tier,
            string sector, string industry);
        IEnumerable<Company> GetAllEventCompanies(int eventId, string companyName, string productService,
            string countryName, int tier, string sector, string industry, string sortColumnDir,
            string sortColumn, int page, int pageSize);

        int GetCountEventCompaniesForCall(int eventId,int userId, string companyName, string productService, string countryName, int tier,
            string sector, string industry);
        IEnumerable<Company> GetAllEventCompaniesForCall(int eventId, int userId, string companyName, string productService,
            string countryName, int tier, string sector, string industry, string sortColumnDir,
            string sortColumn, int page, int pageSize);

        int CountEventCompanies(int eventId, int tier);
        Event GetEvent(int id);
        Event GetEvent(string code);
        Event CreateEvent(Event info, IEnumerable<int> groups, IEnumerable<int> users);
        bool UpdateEvent(Event info);
        Event UpdateEventOperation(int id,VenueInfo venueInfo, AccomodationInfo accomodationInfo, DriverInfo driverInfo, PhotographerInfo photographerInfo, LocalVisaAgentInfo localVisaAgentInfo, PostEventInfo postEventInfo,IEnumerable<EventSession> eventSessions);
        Event UpdateEventIncludeUpdateCollection(Event info, IEnumerable<int> groups, IEnumerable<int> users);
        bool AssignCompany(int id, IEnumerable<int> companies);
        bool DeleteEvent(int id);

        EventCompany GetEventCompany(int eventId, int companyId);
        EventCompany GetEventCompany(int companyId);
        EventCompany CreateEventCompany(EventCompany company);
        bool UpdateEventCompany(EventCompany company);
        bool DeleteEventCompany(int id);
        void DeleteCompanyCache(Company info);
        void UpdateCompanyCache(Company info);
        void UpdateVenueInfo(VenueInfo info);
        void UpdateAccomodationInfo(AccomodationInfo info);
        void UpdateSalesGroupCache(SalesGroup info);
    }
}
