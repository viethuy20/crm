using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Concrete
{
    public class EFLeadService : Repository, ILeadService
    {
        public EFLeadService(DbContext db)
            : base(db)
        {
        }

        public IEnumerable<Lead> GetAllLeads(Func<Lead, bool> predicate)
        {
            Func<Lead, bool> predicate2 =
                m => predicate(m) && m.LeadStatusRecord != LeadStatus.Deleted;
            return GetAll(predicate2, m => m.Event, m => m.LeadStatusRecord).AsEnumerable();
        }

        public Lead GetLead(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return Get<Lead>(m => m.ID == id, u => new
            {
                u.Event,
                u.Company
            });
        }

        public Lead CreateLead(Lead info)
        {
            return Create(info);
        }

        public bool UpdateLead(Lead info)
        {
            return Update(info);
        }
        public bool UpdateAttachment(LeadStatusRecord record)
        {
            return Update(record);
        }

        public bool DeleteLead(int id)
        {
            return Delete<Lead>(id);
        }
        public PhoneCall CreatePhoneCall(PhoneCall info)
        {
            return Create(info);
        }

        public bool UpdatePhoneCall(PhoneCall info)
        {
            var exist = Get<PhoneCall>(info.ID);
            if (exist != null)
            {
                exist.Remark = info.Remark;
                exist.CallBackDate = info.CallBackDate;
                exist.CallBackTiming = info.CallBackTiming;
                return Update(exist);
            }
            return false;
        }

        public PhoneCall GetPhoneCall(int id)
        {
            return Get<PhoneCall>(id);
        }
    }
}
