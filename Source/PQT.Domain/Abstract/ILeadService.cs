using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface ILeadService
    {
        IEnumerable<Lead> GetAllLeads();
        Lead GetLead(int id);
        Lead CreateLead(Lead info);
        bool UpdateLead(Lead info);
        bool DeleteLead(int id);
    }
}
