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
        Event GetEvent(int id);
        Event GetEvent(string code);
        Event CreateEvent(Event info, IEnumerable<int> companies, IEnumerable<int> groups, IEnumerable<int> users);
        bool UpdateEvent(Event info);
        bool UpdateEventIncludeUpdateCollection(Event info, IEnumerable<int> companies, IEnumerable<int> groups, IEnumerable<int> users);
        bool DeleteEvent(int id);
    }
}
