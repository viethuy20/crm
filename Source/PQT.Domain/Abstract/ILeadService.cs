using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface ILeadService
    {
        IEnumerable<Lead> GetAllLeads(Func<Lead, bool> predicate);
        IEnumerable<Lead> GetAllLeadsForKPI(int eventId, int userId,Func<Lead, bool> predicate);
        Lead GetLead(int id);
        Lead CreateLead(Lead info);
        bool UpdateLead(Lead info);
        bool UpdateAttachment(LeadStatusRecord record);
        bool DeleteLead(int id);


        PhoneCall CreatePhoneCall(PhoneCall info);
        bool UpdatePhoneCall(PhoneCall info);
        PhoneCall GetPhoneCall(int id);
    }
}
