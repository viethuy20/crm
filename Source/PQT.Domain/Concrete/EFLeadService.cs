using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Domain.Concrete
{
    public class EFLeadService : Repository, ILeadService
    {
        public EFLeadService(DbContext db)
            : base(db)
        {
        }

        public IEnumerable<Lead> GetAllLeads()
        {
            return GetAll<Lead>().AsEnumerable();
        }

        public Lead GetLead(int id)
        {
            return Get<Lead>(id);
        }

        public Lead CreateLead(Lead info)
        {
            return Create(info);
        }

        public bool UpdateLead(Lead info)
        {
            return Update(info);
        }

        public bool DeleteLead(int id)
        {
            return Delete<Lead>(id);
        }
    }
}
