using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Concrete;
using PQT.Domain.Entities;

namespace PQT.Web.Infrastructure
{
    public class MemoryEventRepository : EFEventService
    {
        private List<Event> _events = new List<Event>();

        #region Factory

        public MemoryEventRepository(DbContext db)
            : base(db)
        {
            RetrieveCacheEvents();
        }

        #endregion

        #region Decorator Properties

        public EFEventService EventRepository
        {
            get { return DependencyHelper.GetService<EFEventService>(); }
        }

        public ICompanyRepository CompanyRepository
        {
            get { return DependencyHelper.GetService<ICompanyRepository>(); }
        }

        #endregion

        public sealed override void RetrieveCacheEvents()
        {
            _events.Clear();
            _events.AddRange(EventRepository.GetAllEvents());
        }


        public override IEnumerable<Event> GetAllEvents()
        {
            return _events.AsEnumerable();
        }
        public override IEnumerable<Event> GetAllEvents(Func<Event, bool> predicate)
        {
            return _events.Where(predicate).AsEnumerable();
        }
        public override Event GetEvent(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _events.FirstOrDefault(m => m.ID == id);
        }
        public override Event GetEvent(string code)
        {
            return _events.FirstOrDefault(m => m.EventCode.ToUpper() == code.Trim().ToUpper());
        }

        public override Event CreateEvent(Event info, IEnumerable<int> groups, IEnumerable<int> users)
        {
            var menu = EventRepository.CreateEvent(info, groups, users);
            _events.Add(new Event(EventRepository.GetEvent(info.ID)));
            return menu;
        }

        public override bool UpdateEvent(Event info)
        {
            if (!EventRepository.UpdateEvent(info)) return false;
            _events.Remove(GetEvent(info.ID));
            _events.Add(new Event(EventRepository.GetEvent(info.ID)));
            return true;
        }

        public override bool UpdateEventIncludeUpdateCollection(Event info, IEnumerable<int> groups, IEnumerable<int> users)
        {
            if (!EventRepository.UpdateEventIncludeUpdateCollection(info, groups, users)) return false;
            _events.Remove(GetEvent(info.ID));
            _events.Add(new Event(EventRepository.GetEvent(info.ID)));
            return true;
        }
        public override bool AssignCompany(int id, IEnumerable<int> companyIds)
        {
            if (!EventRepository.AssignCompany(id, companyIds)) return false;
            var eventExist = EventRepository.GetEvent(id);
            if (eventExist != null)
            {
                _events.Remove(GetEvent(id));
                foreach (var eventCompany in eventExist.EventCompanies.Where(m => m.Company == null).ToList())
                {
                    var com = CompanyRepository.GetCompany(eventCompany.CompanyID);
                    if (com != null)
                    {
                        eventCompany.Company = com;
                    }
                    else
                    {
                        eventExist.EventCompanies.Remove(eventCompany);
                    }
                }
                _events.Add(new Event(eventExist));
            }
            return true;
        }

        public override bool DeleteEvent(int id)
        {
            if (EventRepository.DeleteEvent(id))
            {
                _events.Remove(GetEvent(id));
                return true;
            }
            return false;
        }


        public override EventCompany GetEventCompany(int eventId, int companyId)
        {
            var eventExist = _events.FirstOrDefault(m => m.ID == eventId);
            return eventExist == null ? null : eventExist.EventCompanies.FirstOrDefault(m => m.CompanyID == companyId);
        }

        public override EventCompany GetEventCompany(int companyId)
        {
            return _events.SelectMany(m => m.EventCompanies).Where(m => m.CompanyID == companyId)
                .OrderBy(m => m.UpdatedTime).LastOrDefault(m => m.UpdatedTime != null);
        }
        public override EventCompany CreateEventCompany(EventCompany company)
        {
            company = EventRepository.CreateEventCompany(company);
            if (company != null)
            {
                var eventExist = GetEvent((int)company.EventID);
                if (eventExist != null)
                {
                    _events.Remove(eventExist);
                    eventExist.EventCompanies.Add(company);
                    _events.Add(eventExist);
                }
            }
            return company;
        }
        public override bool UpdateEventCompany(EventCompany company)
        {
            if (!EventRepository.UpdateEventCompany(company)) return false;
            var eventExist = GetEvent((int)company.EventID);
            if (eventExist != null)
            {
                var ecom = eventExist.EventCompanies.FirstOrDefault(m => m.ID == company.ID);
                if (ecom != null)
                {
                    if (ecom.BudgetMonth != company.BudgetMonth ||
                        ecom.BusinessUnit != company.BusinessUnit ||
                        ecom.Remarks != company.Remarks)
                    {
                        _events.Remove(eventExist);
                        eventExist.EventCompanies.Remove(ecom);
                        eventExist.EventCompanies.Add(company);
                        _events.Add(eventExist);
                        var com = CompanyRepository.GetCompany(company.CompanyID);
                        if (com != null)
                        {
                            com.BusinessUnit = company.BusinessUnit;
                            com.BudgetMonth = company.BudgetMonth;
                            com.Remarks = company.Remarks;
                            //CompanyRepository.UpdateCompany(com);
                        }
                    }

                }
                else
                {
                    _events.Remove(eventExist);
                    eventExist.EventCompanies.Add(company);
                    _events.Add(eventExist);
                }
            }
            return true;
        }

        public override void UpdateCompanyCache(Company info)
        {
            var coms = _events.SelectMany(m => m.EventCompanies).Where(m => m.CompanyID == info.ID).Select(m => m.Company).ToList();
            foreach (var com in coms)
            {
                com.CountryID = info.CountryID;
                com.Country = info.Country;
                com.CompanyName = info.CompanyName;
                com.ProductOrService = info.ProductOrService;
                com.Sector = info.Sector;
                com.Industry = info.Industry;
                com.Ownership = info.Ownership;
                com.BusinessUnit = info.BusinessUnit;
                com.BudgetMonth = info.BudgetMonth;
                com.BudgetPerHead = info.BudgetPerHead;
                com.FinancialYear = info.FinancialYear;
                com.Tier = info.Tier;
                com.Address = info.Address;
                com.Tel = info.Tel;
                com.Fax = info.Fax;
                com.Remarks = info.Remarks;
                com.ManagerUsers = info.ManagerUsers;
                com.CreatedTime = info.CreatedTime;
                com.UpdatedTime = info.UpdatedTime;
            }
        }
        public override void DeleteCompanyCache(Company info)
        {
            var coms = _events.SelectMany(m => m.EventCompanies).Where(m => m.CompanyID == info.ID).ToList();
            foreach (var com in coms)
            {
                com.EntityStatus = EntityStatus.Deleted;
                com.Company.EntityStatus = EntityStatus.Deleted;
                EventRepository.DeleteEventCompany(com.ID);
            }
        }
        public override void UpdateSalesGroupCache(SalesGroup info)
        {
            var coms = _events.SelectMany(m => m.SalesGroups).Where(m => m.ID == info.ID).ToList();
            foreach (var com in coms)
            {
                com.GroupName = info.GroupName;
                com.Users = info.Users.Select(m => new User(m)).ToList();
                com.CreatedTime = info.CreatedTime;
                com.UpdatedTime = info.UpdatedTime;
            }
        }
    }
}
