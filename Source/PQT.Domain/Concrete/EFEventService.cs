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

        public IEnumerable<Event> GetAllEvents()
        {
            return GetAll<Event>().AsEnumerable();
        }
        public IEnumerable<Event> GetAllEvents(Func<Event, bool> predicate)
        {
            return GetAll(predicate, m => m.EventSessions, m => m.SalesGroups, m => m.ManagerUsers, m => m.User).AsEnumerable();
        }
        public Event GetEvent(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return Get<Event>(u => u.ID == id, u => new
            {
                u.SalesGroups,
                u.ManagerUsers,
                u.User
            });
        }
        public Event GetEvent(string code)
        {
            return Get<Event>(m => m.EventCode == code.Trim().ToUpper());
        }

        public Event CreateEvent(Event info, IEnumerable<int> groups, IEnumerable<int> users)
        {
            return TransactionWrapper.Do(() =>
            {
                info.EventCode = info.EventCode.Trim().ToUpper();
                info.SalesGroups = _db.Set<SalesGroup>().Where(r => groups.Contains(r.ID)).ToList();
                info.ManagerUsers = _db.Set<User>().Where(r => users.Contains(r.ID)).ToList();
                return Create(info);
            });
        }

        public bool UpdateEvent(Event info)
        {
            info.EventCode = info.EventCode.Trim().ToUpper();
            return Update(info);
        }

        public bool UpdateEventIncludeUpdateCollection(Event info, IEnumerable<int> groups, IEnumerable<int> users)
        {
            return TransactionWrapper.Do(() =>
            {
                var exist = Get<Event>(info.ID);
                if (exist == null)
                {
                    return false;
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
                return Update(exist);
            });
        }
        public bool AssignCompany(int id, IEnumerable<int> companyIds)
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

        public bool DeleteEvent(int id)
        {
            return Delete<Event>(id);
        }
    }
}
