using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{


    public class CandidateStatusRecord : StatusRecord<CandidateStatus>
    {
        public CandidateStatusRecord()
        {
        }

        public CandidateStatusRecord(int entryId)
            : base(entryId)
        {
        }

        public CandidateStatusRecord(int entryId, CandidateStatus status, int? userId, string message = "")
            : base(entryId, status, userId, message)
        {
        }
    }
}
