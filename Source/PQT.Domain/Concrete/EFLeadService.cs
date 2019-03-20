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

        public IEnumerable<Lead> GetAllLeadsForKPI(int eventId, int userId, Func<Lead, bool> predicate)
        {
            Func<Lead, bool> predicate2 = null;
            if (eventId > 0 && userId > 0)
            {
                predicate2 =
                    m => m.EventID == eventId &&
                         (m.UserID == userId || m.User.TransferUserID == userId) &&
                         m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                          m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                          m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value &&
                        predicate(m);
            }
            else if (eventId > 0)
            {
                predicate2 =
                    m => m.EventID == eventId &&
                         m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                         m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                         m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value &&
                         predicate(m);
            }
            else if (userId > 0)
            {
                predicate2 =
                    m => (m.UserID == userId || m.User.TransferUserID == userId) &&
                         m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                         m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                         m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value &&
                         predicate(m);
            }
            else
            {
                predicate2 =
                    m =>
                        m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                        m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                        m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value &&
                        predicate(m);
            }

            return _db.Set<Lead>()
                .Include(m=>m.LeadStatusRecord)
                .Include(m=>m.User)
                .Where(predicate2).AsEnumerable();

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
            info = Create(info);
            if (info != null)
            {
                info.Company = Get<Company>(info.CompanyID);
            }
            return info;
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
