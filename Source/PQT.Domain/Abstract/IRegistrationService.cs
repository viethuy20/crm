using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IRegistrationService
    {
        IEnumerable<Registration> GetAllRegistrations();
        IEnumerable<Registration> GetAllRegistrations(Func<Registration, bool> predicate);
        Registration GetRegistration(int id);
        Registration CreateRegistration(Registration info);
        bool UpdateRegistration(Registration info);
        bool DeleteRegistration(int id);
    }
}
