using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class CandidateStatus : Enumeration
    {
        public static readonly CandidateStatus Deleted = New<CandidateStatus>(0, "Deleted");
        public static readonly CandidateStatus Initial = New<CandidateStatus>(1, "Initial");
        public static readonly CandidateStatus RequestEmployment = New<CandidateStatus>(2, "Request Employment");
        public static readonly CandidateStatus ApprovedEmployment = New<CandidateStatus>(3, "Approved Employment");
        public static readonly CandidateStatus RejectedEmployment = New<CandidateStatus>(4, "Rejected Employment");
    }
}
