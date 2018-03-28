using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Domain.Concrete
{
    public class EFRegistrationService : Repository, IRegistrationService
    {
        public EFRegistrationService(DbContext db)
            : base(db)
        {
        }

        public IEnumerable<Registration> GetAllRegistrations()
        {
            return GetAll<Registration>().AsEnumerable();
        }
        public IEnumerable<Registration> GetAllRegistrations(Func<Registration, bool> predicate)
        {
            return GetAll(predicate).AsEnumerable();
        }

        public Registration GetRegistration(int id)
        {
            return Get<Registration>(m => m.ID == id);
        }

        public Registration CreateRegistration(Registration info)
        {
            return Create(info);
        }

        public bool UpdateRegistration(Registration info)
        {
            return Update(info);
        }

        public bool DeleteRegistration(int id)
        {
            return Delete<Registration>(id);
        }
    }
}
