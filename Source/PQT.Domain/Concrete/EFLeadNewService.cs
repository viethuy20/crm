using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Concrete
{
    public class EFLeadNewService : Repository, ILeadNewService
    {
        public EFLeadNewService(DbContext db)
            : base(db)
        {
        }

        public IEnumerable<LeadNew> GetAllLeadNews(Func<LeadNew, bool> predicate)
        {
            Func<LeadNew, bool> predicate2 =
                m => predicate(m);
            return GetAll(predicate2, m => m.Event,m=>m.AssignUser).AsEnumerable();
        }

        public LeadNew GetLeadNew(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return Get<LeadNew>(m => m.ID == id, u => new
            {
                u.Event,
                u.Company
            });
        }

        public LeadNew CreateLeadNew(LeadNew info)
        {
            info = Create(info);
            if (info != null)
            {
                info.Company = Get<Company>(info.CompanyID);
            }
            return info;
        }

        public bool UpdateLeadNew(LeadNew info)
        {
            return Update(info);
        }
        public bool DeleteLeadNew(int id)
        {
            return Delete<LeadNew>(id);
        }
    }
}

