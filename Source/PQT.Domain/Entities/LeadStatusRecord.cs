using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class LeadStatusRecord : StatusRecord<LeadStatus>
    {
        public LeadStatusRecord()
        {
        }

        public LeadStatusRecord(int entryId)
            : base(entryId)
        {
        }

        public LeadStatusRecord(int entryId, LeadStatus status, string message = "")
            : base(entryId, status, message)
        {
        }
        public LeadStatusRecord(int entryId, LeadStatus status, int? userId, string message = "")
            : base(entryId, status, userId, message)
        {
        }
    }
}
