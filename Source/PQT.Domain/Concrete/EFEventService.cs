using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

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

        public Event GetEvent(int id)
        {
            return Get<Event>(id);
        }
        public Event GetEvent(string code)
        {
            return Get<Event>(m => m.EventCode == code.Trim().ToUpper());
        }

        public Event CreateEvent(Event info, IEnumerable<int> companies, IEnumerable<int> groups, IEnumerable<int> users)
        {
            return TransactionWrapper.Do(() =>
            {
                info.EventCode = info.EventCode.Trim().ToUpper();
                info.Companies = GetAll<Company>(r => companies.Contains(r.ID)).ToList();
                info.SalesGroups = GetAll<SalesGroup>(r => groups.Contains(r.ID)).ToList();
                info.Users = GetAll<User>(r => users.Contains(r.ID)).ToList();
                return Create(info);
            });
        }

        public bool UpdateEvent(Event info)
        {
            info.EventCode = info.EventCode.Trim().ToUpper();
            return Update(info);
        }

        public bool UpdateEventIncludeUpdateCollection(Event info, IEnumerable<int> companies,
            IEnumerable<int> groups, IEnumerable<int> users)
        {
            return TransactionWrapper.Do(() =>
            {
                var groupExist = Get<Event>(info.ID);
                if (groupExist == null)
                {
                    return false;
                }
                info.EventCode = info.EventCode.Trim().ToUpper();
                Update(info);

                groupExist.Companies.Clear();
                groupExist.Companies = GetAll<Company>(r => companies.Contains(r.ID)).ToList();
                groupExist.SalesGroups.Clear();
                groupExist.SalesGroups = GetAll<SalesGroup>(r => groups.Contains(r.ID)).ToList();
                groupExist.Users.Clear();
                groupExist.Users = GetAll<User>(r => users.Contains(r.ID)).ToList();


                if (info.EventSessions != null && info.EventSessions.Any())
                {
                    foreach (var photo in groupExist.EventSessions.Where(m => !info.EventSessions.Select(n => n.ID).Contains(m.ID)).ToList())
                    {
                        groupExist.EventSessions.Remove(photo);
                        Delete(photo);
                    }
                    UpdateCollection(info, m => m.ID == info.ID, m => m.EventSessions, m => m.ID);
                }
                else if (groupExist.EventSessions != null)
                    foreach (var photo in groupExist.EventSessions.ToList())
                    {
                        groupExist.EventSessions.Remove(photo);
                        Delete(photo);
                    }

                return Update(groupExist);
            });
        }

        public bool DeleteEvent(int id)
        {
            return Delete<Event>(id);
        }
    }
}
