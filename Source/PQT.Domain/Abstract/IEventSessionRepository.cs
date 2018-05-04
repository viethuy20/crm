using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IEventSessionRepository
    {
        IEnumerable<EventSession> GetAllEventSessions();
        EventSession GetEventSession(int id);
        EventSession CreateEventSession(EventSession info);
        bool UpdateEventSessuib(EventSession info);
        bool DeleteEventSession(int id);
    }
}
