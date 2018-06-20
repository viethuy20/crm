using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IEventService
    {
        IEnumerable<Event> GetAllEvents();
        IEnumerable<Event> GetAllEvents(Func<Event, bool> predicate);
        Event GetEvent(int id);
        Event GetEvent(string code);
        Event CreateEvent(Event info, IEnumerable<int> companies, IEnumerable<int> groups);
        bool UpdateEvent(Event info);
        bool UpdateEventIncludeUpdateCollection(Event info, IEnumerable<int> companies, IEnumerable<int> groups);
        bool DeleteEvent(int id);
    }
}
