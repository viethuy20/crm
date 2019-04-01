using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using NS;
using NS.Entity;
using NS.Helpers;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;

namespace PQT.Domain.Concrete
{
    public class EFEventService : Repository, IEventService
    {
        public EFEventService(DbContext db)
            : base(db)
        {
        }

        public int GetCountEvents(int userId, string searchValue)
        {
            IQueryable<Event> queries = _db.Set<Event>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (userId > 0)
            {
                queries = queries.Where(m => m.EventStatus.Value == EventStatus.Confirmed.Value ||
                    m.EventStatus.Value == EventStatus.Live.Value);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.StartDate == dtSearch || m.EndDate == dtSearch ||
                                                 m.DateOfConfirmation == dtSearch || m.ClosingDate == dtSearch);
                else
                {
                    var searchEventStatuss = Enumeration.GetAll<EventStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        m.EventCode.ToLower().Contains(searchValue) ||
                        m.EventName.ToLower().Contains(searchValue) ||
                        (m.EventCategory != null && m.EventCategory.Name.ToLower().Contains(searchValue)) ||
                        searchEventStatuss.Contains(m.EventStatus.Value) ||
                        (m.Location != null && m.Location.ToLower().Contains(searchValue)) ||
                        (m.VenueInfo != null && m.VenueInfo.HotelVenue.ToLower().Contains(searchValue)));
                }
            }
            return queries.Include(m => m.EventCategory).Include(m => m.VenueInfo).Count();
        }

        public IEnumerable<Event> GetAllEvents(int userId, string searchValue, string sortColumnDir,
            string sortColumn, int page, int pageSize)
        {
            IQueryable<Event> queries = _db.Set<Event>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (userId > 0)
            {
                queries = queries.Where(m => m.EventStatus.Value == EventStatus.Confirmed.Value ||
                                             m.EventStatus.Value == EventStatus.Live.Value);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.StartDate == dtSearch || m.EndDate == dtSearch ||
                                                 m.DateOfConfirmation == dtSearch || m.ClosingDate == dtSearch);
                else
                {
                    var searchEventStatuss = Enumeration.GetAll<EventStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        m.EventCode.ToLower().Contains(searchValue) ||
                        m.EventName.ToLower().Contains(searchValue) ||
                        (m.EventCategory != null && m.EventCategory.Name.ToLower().Contains(searchValue)) ||
                        searchEventStatuss.Contains(m.EventStatus.Value) ||
                        (m.Location != null && m.Location.ToLower().Contains(searchValue)) ||
                        (m.VenueInfo != null && m.VenueInfo.HotelVenue.ToLower().Contains(searchValue)));
                }
            }

            switch (sortColumn)
            {
                case "EventCode":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.EventCode)
                        : queries.OrderByDescending(s => s.EventCode);
                    break;
                case "EventName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.EventName)
                        : queries.OrderByDescending(s => s.EventName);
                    break;
                case "EventCategoryDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.EventCategory.Name)
                        : queries.OrderByDescending(s => s.EventCategory.Name);
                    break;
                case "EventStatusDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.EventStatus.Value)
                        : queries.OrderByDescending(s => s.EventStatus.Value);
                    break;
                case "StartDate":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.StartDate)
                        : queries.OrderByDescending(s => s.StartDate);
                    break;
                case "EndDate":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.EndDate)
                        : queries.OrderByDescending(s => s.EndDate);
                    break;
                case "DateOfConfirmation":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.DateOfConfirmation)
                        : queries.OrderByDescending(s => s.DateOfConfirmation);
                    break;
                case "ClosingDate":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ClosingDate)
                        : queries.OrderByDescending(s => s.ClosingDate);
                    break;
                case "Location":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Location)
                        : queries.OrderByDescending(s => s.Location);
                    break;
                case "HotelVenue":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.VenueInfo.HotelVenue)
                        : queries.OrderByDescending(s => s.VenueInfo.HotelVenue);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize)
                .Include(m => m.EventCategory)
                .Include(m => m.VenueInfo)
                .ToList();
        }
        public int GetCountEventsForOpe(string searchValue)
        {
            IQueryable<Event> queries = _db.Set<Event>().Where(m =>
                m.EventStatus.Value != EventStatus.Completed.Value &&
                m.EventStatus.Value != EventStatus.Cancelled.Value &&
                m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.StartDate == dtSearch || m.EndDate == dtSearch ||
                                                 m.DateOfConfirmation == dtSearch || m.ClosingDate == dtSearch);
                else
                {
                    var searchEventStatuss = Enumeration.GetAll<EventStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        m.EventCode.ToLower().Contains(searchValue) ||
                        m.EventName.ToLower().Contains(searchValue) ||
                        (m.EventCategory != null && m.EventCategory.Name.ToLower().Contains(searchValue)) ||
                        searchEventStatuss.Contains(m.EventStatus.Value) ||
                        (m.Location != null && m.Location.ToLower().Contains(searchValue)) ||
                        (m.VenueInfo != null && m.VenueInfo.HotelVenue.ToLower().Contains(searchValue)));
                }
            }
            return queries.Include(m => m.EventCategory).Include(m => m.VenueInfo).Count();
        }

        public IEnumerable<Event> GetAllEventsForOpe(string searchValue, string sortColumnDir,
            string sortColumn, int page, int pageSize)
        {
            IQueryable<Event> queries = _db.Set<Event>().Where(m =>
                m.EventStatus.Value != EventStatus.Completed.Value &&
                m.EventStatus.Value != EventStatus.Cancelled.Value &&
                m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.StartDate == dtSearch || m.EndDate == dtSearch ||
                                                 m.DateOfConfirmation == dtSearch || m.ClosingDate == dtSearch);
                else
                {
                    var searchEventStatuss = Enumeration.GetAll<EventStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        m.EventCode.ToLower().Contains(searchValue) ||
                        m.EventName.ToLower().Contains(searchValue) ||
                        (m.EventCategory != null && m.EventCategory.Name.ToLower().Contains(searchValue)) ||
                        searchEventStatuss.Contains(m.EventStatus.Value) ||
                        (m.Location != null && m.Location.ToLower().Contains(searchValue)) ||
                        (m.VenueInfo != null && m.VenueInfo.HotelVenue.ToLower().Contains(searchValue)));
                }
            }

            switch (sortColumn)
            {
                case "EventCode":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.EventCode)
                        : queries.OrderByDescending(s => s.EventCode);
                    break;
                case "EventName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.EventName)
                        : queries.OrderByDescending(s => s.EventName);
                    break;
                case "EventCategoryDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.EventCategory.Name)
                        : queries.OrderByDescending(s => s.EventCategory.Name);
                    break;
                case "EventStatusDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.EventStatus.Value)
                        : queries.OrderByDescending(s => s.EventStatus.Value);
                    break;
                case "StartDate":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.StartDate)
                        : queries.OrderByDescending(s => s.StartDate);
                    break;
                case "EndDate":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.EndDate)
                        : queries.OrderByDescending(s => s.EndDate);
                    break;
                case "DateOfConfirmation":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.DateOfConfirmation)
                        : queries.OrderByDescending(s => s.DateOfConfirmation);
                    break;
                case "ClosingDate":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ClosingDate)
                        : queries.OrderByDescending(s => s.ClosingDate);
                    break;
                case "Location":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Location)
                        : queries.OrderByDescending(s => s.Location);
                    break;
                case "HotelVenue":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.VenueInfo.HotelVenue)
                        : queries.OrderByDescending(s => s.VenueInfo.HotelVenue);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize).Include(m => m.EventCategory).Include(m => m.VenueInfo)
                .ToList();
        }

        public IEnumerable<Event> GetAllEventsForDashboard(int userId, string searchValue)
        {
            var today = DateTime.Today;
            IQueryable<Event> queries = _db.Set<Event>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m => m.EventName.ToLower().Contains(searchValue) ||
                                             m.EventCode.ToLower().Contains(searchValue));
            }
            if (userId > 0)
            {
                queries = queries.Where(m => (m.EventStatus.Value == EventStatus.Live.Value ||
                                              m.EventStatus.Value == EventStatus.Confirmed.Value) &&
                                             today <= m.ClosingDate &&
                                             (m.UserID == userId ||
                                              m.SalesGroups.SelectMany(g => g.Users.Select(u => u.ID))
                                                  .Contains(userId) ||
                                              m.SalesGroups
                                                  .SelectMany(g =>
                                                      g.Users.Where(u => u.TransferUserID > 0)
                                                          .Select(u => u.TransferUserID))
                                                  .Contains(userId) ||
                                              m.ManagerUsers.Select(u => u.ID).Contains(userId) ||
                                              m.ManagerUsers.Where(u => u.TransferUserID > 0)
                                                  .Select(u => u.TransferUserID).Contains(userId) ||
                                              (m.DateOfOpen <= today && today <= m.ClosingDate)));
            }
            else
            {

                queries = queries.Where(m => (m.EventStatus.Value == EventStatus.Live.Value || m.EventStatus.Value == EventStatus.Confirmed.Value) &&
                                             (m.DateOfOpen <= today && today <= m.ClosingDate));
            }
            return queries.Include(m => m.SalesGroups).Include(m => m.ManagerUsers).ToList();
        }
        public IEnumerable<Event> GetAllPossibleEvents(string searchValue)
        {
            IQueryable<Event> queries = _db.Set<Event>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m => m.EventName.ToLower().Contains(searchValue) ||
                                             m.EventCode.ToLower().Contains(searchValue));
            }
            return queries.ToList();
        }
        public virtual IEnumerable<Event> GetAllEvents()
        {
            return GetAll<Event>(m => m.EventSessions, m => m.SalesGroups, m => m.ManagerUsers, m => m.EventCompanies, m => m.User).Select(m => new Event(m)).AsEnumerable();
        }
        public virtual IEnumerable<Event> GetAllEvents(Func<Event, bool> predicate)
        {
            return GetAll(predicate, m => m.EventSessions, m => m.SalesGroups, m => m.ManagerUsers, m => m.User).Select(m => new Event(m)).AsEnumerable();
        }


        public int GetCountEventCompanies(int eventId, string companyName, string productService, string countryName, int tier, string sector, string industry)
        {
            IQueryable<EventCompany> queries = _db.Set<EventCompany>().Where(m => m.EventID == eventId &&
            m.EntityStatus.Value == EntityStatus.Normal.Value && m.Company.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(companyName))
                queries = queries.Where(m => m.Company.CompanyName.ToLower().Contains(companyName));
            if (!string.IsNullOrEmpty(countryName))
                queries = queries.Where(m => m.Company.Country != null && (m.Company.Country.Code.ToLower().Contains(countryName) ||
                                                                   m.Company.Country.Name.ToLower().Contains(countryName)));
            if (!string.IsNullOrEmpty(productService))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.ProductOrService) && m.Company.ProductOrService.ToLower().Contains(productService));
            if (!string.IsNullOrEmpty(sector))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.Sector) && m.Company.Sector.ToLower().Contains(sector));
            if (tier > 0)
                queries = queries.Where(m => m.Company.Tier == tier);
            if (!string.IsNullOrEmpty(industry))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.Industry) && m.Company.Industry.ToLower().Contains(industry));

            return queries.Count();
        }

        public IEnumerable<Company> GetAllEventCompanies(int eventId, string companyName, string productService, string countryName, int tier, string sector, string industry, string sortColumnDir,
            string sortColumn, int page, int pageSize)
        {
            IQueryable<EventCompany> queries = _db.Set<EventCompany>().Where(m => m.EventID == eventId &&
                                                                                  m.EntityStatus.Value == EntityStatus.Normal.Value && m.Company.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(companyName))
                queries = queries.Where(m => m.Company.CompanyName.ToLower().Contains(companyName));
            if (!string.IsNullOrEmpty(countryName))
                queries = queries.Where(m => m.Company.Country != null && (m.Company.Country.Code.ToLower().Contains(countryName) ||
                                                                           m.Company.Country.Name.ToLower().Contains(countryName)));
            if (!string.IsNullOrEmpty(productService))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.ProductOrService) && m.Company.ProductOrService.ToLower().Contains(productService));
            if (!string.IsNullOrEmpty(sector))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.Sector) && m.Company.Sector.ToLower().Contains(sector));
            if (tier > 0)
                queries = queries.Where(m => m.Company.Tier == tier);
            if (!string.IsNullOrEmpty(industry))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.Industry) && m.Company.Industry.ToLower().Contains(industry));

            switch (sortColumn)
            {
                case "CountryName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.Country.Name)
                        : queries.OrderByDescending(s => s.Company.Country.Name);
                    break;
                case "ProductOrService":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.ProductOrService)
                        : queries.OrderByDescending(s => s.Company.ProductOrService);
                    break;
                case "Sector":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.Sector)
                        : queries.OrderByDescending(s => s.Company.Sector);
                    break;
                case "Industry":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.Industry)
                        : queries.OrderByDescending(s => s.Company.Industry);
                    break;
                case "Tier":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.Tier)
                        : queries.OrderByDescending(s => s.Company.Tier);
                    break;
                case "BusinessUnit":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.BusinessUnit)
                        : queries.OrderByDescending(s => s.BusinessUnit);
                    break;
                case "FinancialYear":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.FinancialYear)
                        : queries.OrderByDescending(s => s.Company.FinancialYear);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.CompanyName)
                        : queries.OrderByDescending(s => s.Company.CompanyName);
                    break;
            }
            return queries.Skip(page).Take(pageSize)
                .Select(m => m.Company)
                .ToList();
        }

        public int GetCountEventCompaniesForCall(int eventId, int userId, string companyName, string productService, string countryName, int tier, string sector, string industry, string businessUnit, string ownership)
        {
            int tier1 = Convert.ToInt16(TierType.Tier1);
            int tier2 = Convert.ToInt16(TierType.Tier2);
            int tier3 = Convert.ToInt16(TierType.Tier3);
            IQueryable<EventCompany> queries = _db.Set<EventCompany>()
                .Where(m => m.EventID == eventId &&
                            m.Company.EntityStatus.Value == EntityStatus.Normal.Value &&
                            (m.Company.Tier == tier3 ||
                             m.Company.Tier == tier2 ||
                             !m.Company.ManagerUsers.Any() ||
                             (m.Company.Tier == tier1 &&
                              m.Company.ManagerUsers.Select(u => u.ID).Contains(userId)
                             )));
            if (!string.IsNullOrEmpty(companyName))
                queries = queries.Where(m => m.Company.CompanyName.ToLower().Contains(companyName));
            if (!string.IsNullOrEmpty(countryName))
                queries = queries.Where(m => m.Company.Country != null && (m.Company.Country.Code.ToLower().Contains(countryName) ||
                                                                   m.Company.Country.Name.ToLower().Contains(countryName)));
            if (!string.IsNullOrEmpty(productService))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.ProductOrService) && m.Company.ProductOrService.ToLower().Contains(productService));
            if (!string.IsNullOrEmpty(sector))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.Sector) && m.Company.Sector.ToLower().Contains(sector));
            if (tier > 0)
                queries = queries.Where(m => m.Company.Tier == tier);
            if (!string.IsNullOrEmpty(industry))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.Industry) && m.Company.Industry.ToLower().Contains(industry));
            if (!string.IsNullOrEmpty(businessUnit))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.BusinessUnit) && m.Company.BusinessUnit.ToLower().Contains(businessUnit));
            if (!string.IsNullOrEmpty(ownership))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.Ownership) && m.Company.Ownership.ToLower().Contains(ownership));

            return queries.Count();
        }

        public IEnumerable<Company> GetAllEventCompaniesForCall(int eventId, int userId, string companyName, string productService, string countryName, int tier, string sector, string industry, string businessUnit, string ownership, string sortColumnDir,
            string sortColumn, int page, int pageSize)
        {
            int tier1 = Convert.ToInt16(TierType.Tier1);
            int tier2 = Convert.ToInt16(TierType.Tier2);
            int tier3 = Convert.ToInt16(TierType.Tier3);
            IQueryable<EventCompany> queries = _db.Set<EventCompany>()
                .Where(m => m.EventID == eventId &&
                            m.Company.EntityStatus.Value == EntityStatus.Normal.Value &&
                            (m.Company.Tier == tier3 ||
                             m.Company.Tier == tier2 ||
                             !m.Company.ManagerUsers.Any() ||
                             (m.Company.Tier == tier1 &&
                              m.Company.ManagerUsers.Select(u => u.ID).Contains(userId)
                             )));
            if (!string.IsNullOrEmpty(companyName))
                queries = queries.Where(m => m.Company.CompanyName.ToLower().Contains(companyName));
            if (!string.IsNullOrEmpty(countryName))
                queries = queries.Where(m => m.Company.Country != null && (m.Company.Country.Code.ToLower().Contains(countryName) ||
                                                                           m.Company.Country.Name.ToLower().Contains(countryName)));
            if (!string.IsNullOrEmpty(productService))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.ProductOrService) && m.Company.ProductOrService.ToLower().Contains(productService));
            if (!string.IsNullOrEmpty(sector))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.Sector) && m.Company.Sector.ToLower().Contains(sector));
            if (tier > 0)
                queries = queries.Where(m => m.Company.Tier == tier);
            if (!string.IsNullOrEmpty(industry))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.Industry) && m.Company.Industry.ToLower().Contains(industry));
            if (!string.IsNullOrEmpty(businessUnit))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.BusinessUnit) && m.Company.BusinessUnit.ToLower().Contains(businessUnit));
            if (!string.IsNullOrEmpty(ownership))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.Company.Ownership) && m.Company.Ownership.ToLower().Contains(ownership));

            switch (sortColumn)
            {
                case "CountryName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.Country.Name)
                        : queries.OrderByDescending(s => s.Company.Country.Name);
                    break;
                case "ProductOrService":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.ProductOrService)
                        : queries.OrderByDescending(s => s.Company.ProductOrService);
                    break;
                case "Sector":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.Sector)
                        : queries.OrderByDescending(s => s.Company.Sector);
                    break;
                case "Industry":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.Industry)
                        : queries.OrderByDescending(s => s.Company.Industry);
                    break;
                case "Tier":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.Tier)
                        : queries.OrderByDescending(s => s.Company.Tier);
                    break;
                case "BusinessUnit":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.BusinessUnit)
                        : queries.OrderByDescending(s => s.BusinessUnit);
                    break;
                case "FinancialYear":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.FinancialYear)
                        : queries.OrderByDescending(s => s.Company.FinancialYear);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.CompanyName)
                        : queries.OrderByDescending(s => s.Company.CompanyName);
                    break;
            }
            return queries.Skip(page).Take(pageSize)
                .Select(m => m.Company)
                .ToList();
        }

        public int CountEventCompanies(int eventId, int tier)
        {
            return _db.Set<EventCompany>().Count(m => m.EventID == eventId && m.Company.Tier == tier);
        }
        public virtual Event GetEvent(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return _db.Set<Event>().FirstOrDefault(u => u.ID == id);
            //return Get<Event>(u => u.ID == id, m => new
            //{
            //    EventSessions = m.EventSessions.Select(s => s.Trainer),
            //    SalesGroups = m.SalesGroups.Select(s => s.Users),
            //    m.ManagerUsers,
            //    EventCompanies = m.EventCompanies.Select(s => s.Company),
            //    m.User
            //});
        }
        public virtual Event GetEvent(string code)
        {
            code = code?.Trim().ToUpper() ?? "";
            return _db.Set<Event>().FirstOrDefault(m => m.EventCode == code);
        }

        public virtual Event CreateEvent(Event info, IEnumerable<int> groups, IEnumerable<int> users)
        {
            return TransactionWrapper.Do(() =>
            {
                info.EventCode = info.EventCode.Trim().ToUpper();
                info.SalesGroups = _db.Set<SalesGroup>().Where(r => groups.Contains(r.ID)).ToList();
                info.ManagerUsers = _db.Set<User>().Where(r => users.Contains(r.ID)).ToList();
                return Create(info);
            });
        }

        public virtual bool UpdateEvent(Event info)
        {
            var exist = _db.Set<Event>().FirstOrDefault(u => u.ID == info.ID);
            if (exist == null)
            {
                return false;
            }
            info.EventCode = info.EventCode.Trim().ToUpper();
            Update(info);
            return Update(exist);
        }

        public virtual Event UpdateEventOperation(int id, VenueInfo venueInfo, AccomodationInfo accomodationInfo, DriverInfo driverInfo, PhotographerInfo photographerInfo, LocalVisaAgentInfo localVisaAgentInfo, PostEventInfo postEventInfo, IEnumerable<EventSession> eventSessions)
        {
            var exist = _db.Set<Event>().FirstOrDefault(u => u.ID == id);
            if (exist == null)
            {
                return null;
            }
            if (venueInfo.ID > 0) Update(venueInfo);
            exist.VenueInfo = venueInfo;

            if (accomodationInfo.ID > 0) Update(accomodationInfo);
            exist.AccomodationInfo = accomodationInfo;

            if (driverInfo.ID > 0) Update(driverInfo);
            exist.DriverInfo = driverInfo;

            if (photographerInfo.ID > 0) Update(photographerInfo);
            exist.PhotographerInfo = photographerInfo;

            if (localVisaAgentInfo.ID > 0) Update(localVisaAgentInfo);
            exist.LocalVisaAgentInfo = localVisaAgentInfo;

            if (postEventInfo.ID > 0) Update(postEventInfo);
            exist.PostEventInfo = postEventInfo;
            var sessionUpdated = false;
            try
            {

                if (eventSessions != null)
                {
                    foreach (var existEventSession in exist.EventSessions)
                    {
                        var trainerUpdate = eventSessions.FirstOrDefault(m => m.ID == existEventSession.ID);
                        if (trainerUpdate == null)
                            continue;
                        if (!string.IsNullOrEmpty(trainerUpdate.TrainerInvoice) &&
                            existEventSession.TrainerInvoice != trainerUpdate.TrainerInvoice)
                        {
                            sessionUpdated = true;
                            existEventSession.TrainerInvoice = trainerUpdate.TrainerInvoice;
                        }
                        if (!string.IsNullOrEmpty(trainerUpdate.TrainerTicket) &&
                            existEventSession.TrainerTicket != trainerUpdate.TrainerTicket)
                        {
                            sessionUpdated = true;
                            existEventSession.TrainerTicket = trainerUpdate.TrainerTicket;
                        }
                        if (!string.IsNullOrEmpty(trainerUpdate.TrainerVisa) &&
                            existEventSession.TrainerVisa != trainerUpdate.TrainerVisa)
                        {
                            sessionUpdated = true;
                            existEventSession.TrainerVisa = trainerUpdate.TrainerVisa;
                        }
                        if (!string.IsNullOrEmpty(trainerUpdate.TrainerInsurance) &&
                            existEventSession.TrainerInsurance != trainerUpdate.TrainerInsurance)
                        {
                            sessionUpdated = true;
                            existEventSession.TrainerInsurance = trainerUpdate.TrainerInsurance;
                        }
                        if (!string.IsNullOrEmpty(trainerUpdate.OperationTicket) &&
                            existEventSession.OperationTicket != trainerUpdate.OperationTicket)
                        {
                            sessionUpdated = true;
                            existEventSession.OperationTicket = trainerUpdate.OperationTicket;
                        }
                        if (!string.IsNullOrEmpty(trainerUpdate.OperationVisa) &&
                            existEventSession.OperationVisa != trainerUpdate.OperationVisa)
                        {
                            sessionUpdated = true;
                            existEventSession.OperationVisa = trainerUpdate.OperationVisa;
                        }
                        if (!string.IsNullOrEmpty(trainerUpdate.OperationInsurance) &&
                            existEventSession.OperationInsurance != trainerUpdate.OperationInsurance)
                        {
                            sessionUpdated = true;
                            existEventSession.OperationInsurance = trainerUpdate.OperationInsurance;
                        }
                    }
                    if (sessionUpdated)
                    {
                        UpdateCollection(exist, m => m.ID == exist.ID, m => m.EventSessions, m => m.ID);
                    }
                }

            }
            catch (Exception e)
            {
            }
            if (venueInfo.ID == 0 || accomodationInfo.ID == 0 || driverInfo.ID == 0 ||
                photographerInfo.ID == 0 || localVisaAgentInfo.ID == 0 || postEventInfo.ID == 0)
                Update(exist);
            return exist;
        }

        public virtual Event UpdateEventIncludeUpdateCollection(Event info, IEnumerable<int> groups, IEnumerable<int> users)
        {
            return TransactionWrapper.Do(() =>
            {
                var exist = _db.Set<Event>().FirstOrDefault(u => u.ID == info.ID);
                if (exist == null)
                {
                    return null;
                }
                info.EventCode = info.EventCode.Trim().ToUpper();
                Update(info);

                exist.SalesGroups.Clear();
                exist.SalesGroups = _db.Set<SalesGroup>().Where(r => groups.Contains(r.ID)).ToList();
                exist.ManagerUsers.Clear();
                exist.ManagerUsers = _db.Set<User>().Where(r => users.Contains(r.ID)).ToList();
                if (info.EventSessions != null && info.EventSessions.Any())
                {
                    foreach (var item in exist.EventSessions.Where(m => !info.EventSessions.Select(n => n.ID).Contains(m.ID)).ToList())
                    {
                        exist.EventSessions.Remove(item);
                        PermanentlyDelete(item);
                    }
                    UpdateCollection(info, m => m.ID == info.ID, m => m.EventSessions, m => m.ID);
                }
                else if (exist.EventSessions != null)
                    foreach (var item in exist.EventSessions.ToList())
                    {
                        exist.EventSessions.Remove(item);
                        PermanentlyDelete(item);
                    }
                Update(exist);
                return exist;
            });
        }
        public virtual bool AssignCompany(int id, IEnumerable<int> companyIds)
        {
            return TransactionWrapper.Do(() =>
            {
                var exist = _db.Set<Event>().FirstOrDefault(u => u.ID == id);
                if (exist == null)
                {
                    return false;
                }
                var deleteEvenCompanies = exist.EventCompanies
                    .Where(m => !companyIds.Contains(m.CompanyID)).ToList();
                foreach (var item in deleteEvenCompanies)
                {
                    exist.EventCompanies.Remove(item);
                    PermanentlyDelete(item);
                }
                var addEvenCompanies = companyIds.Where(m => !exist.EventCompanies.Select(c => c.CompanyID).Contains(m))
                    .Select(m => new EventCompany(id, m));
                exist.EventCompanies.AddRange(addEvenCompanies);
                return Update(exist);
            });
        }

        public virtual bool DeleteEvent(int id)
        {
            return Delete<Event>(id);
        }


        public virtual EventCompany GetEventCompany(int eventId, int companyId)
        {
            return GetAll<EventCompany>(m => m.EventID == eventId && m.CompanyID == companyId).LastOrDefault();
        }

        public virtual EventCompany GetEventCompany(int companyId)
        {
            return GetAll<EventCompany>(m => m.CompanyID == companyId).OrderBy(m => m.UpdatedTime).LastOrDefault(m => m.UpdatedTime != null);
        }

        public virtual EventCompany CreateEventCompany(EventCompany company)
        {
            return Create(company);
        }

        public virtual bool UpdateEventCompany(EventCompany company)
        {
            //var exist = Get<EventCompany>(company.ID);
            //if (exist != null)
            //{
            //exist.BudgetMonth = company.BudgetMonth;
            //exist.BusinessUnit = company.BusinessUnit;
            //exist.Remarks = company.Remarks;
            return Update(company);
            //}
            //return false;
        }
        public virtual bool DeleteEventCompany(int id)
        {
            return PermanentlyDelete(Get<EventCompany>(id));
        }

        public virtual void UpdateCompanyCache(Company info)
        {

        }
        public virtual void UpdateVenueInfo(VenueInfo info)
        {

        }
        public virtual void UpdateAccomodationInfo(AccomodationInfo info)
        {

        }
        public virtual void DeleteCompanyCache(Company info)
        {

        }
        public virtual void UpdateSalesGroupCache(SalesGroup info)
        {

        }
    }
}
