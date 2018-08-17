using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Helpers;

namespace PQT.Domain.Concrete
{
    public class EFEventService : Repository, IEventService
    {
        public EFEventService(DbContext db)
            : base(db)
        {
        }

        public virtual void RetrieveCacheEvents()
        {

        }
        public virtual IEnumerable<Event> GetAllEvents()
        {
            return GetAll<Event>(m => m.EventSessions, m => m.SalesGroups, m => m.ManagerUsers, m => m.EventCompanies, m => m.User).Select(m => new Event(m)).AsEnumerable();
        }
        public virtual IEnumerable<Event> GetAllEvents(Func<Event, bool> predicate)
        {
            return GetAll(predicate, m => m.EventSessions, m => m.SalesGroups, m => m.ManagerUsers, m => m.User).Select(m => new Event(m)).AsEnumerable();
        }
        public virtual Event GetEvent(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return Get<Event>(u => u.ID == id, m => new
            {
                EventSessions = m.EventSessions.Select(s => s.Trainer),
                SalesGroups = m.SalesGroups.Select(s => s.Users),
                m.ManagerUsers,
                EventCompanies = m.EventCompanies.Select(s => s.Company),
                m.User
            });
        }
        public virtual Event GetEvent(string code)
        {
            return Get<Event>(m => m.EventCode == code.Trim().ToUpper());
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
            info.EventCode = info.EventCode.Trim().ToUpper();
            return Update(info);
        }

        public virtual Event UpdateEventIncludeUpdateCollection(Event info, IEnumerable<int> groups, IEnumerable<int> users)
        {
            return TransactionWrapper.Do(() =>
            {
                var exist = Get<Event>(info.ID);
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
                var exist = Get<Event>(id);
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
        public virtual void DeleteCompanyCache(Company info)
        {

        }
        public virtual void UpdateSalesGroupCache(SalesGroup info)
        {

        }
    }
}
