using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Domain.Concrete
{
    public class EFEventSessionRepository : Repository, IEventSessionRepository
    {
        public EFEventSessionRepository(DbContext db) : base(db)
        {
        }

        public IEnumerable<EventSession> GetAllEventSessions()
        {
            return GetAll<EventSession>().AsEnumerable();
        }

        public EventSession GetEventSession(int id)
        {
            return Get<EventSession>(u => u.ID == id);
        }

        public EventSession CreateEventSession(EventSession info)
        {
            return TransactionWrapper.Do(() =>
            {
                return Create(info);
            });
        }

        public bool UpdateEventSessuib(EventSession info)
        {
            return Update(info);
        }

        public bool DeleteEventSession(int id)
        {
            return Delete<Event>(id);
        }
    }
}
