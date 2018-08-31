using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class RecruitmentStatus : Enumeration
    {
        public static readonly RecruitmentStatus Initial = New<RecruitmentStatus>(1, "Initial");
        public static readonly RecruitmentStatus Passed = New<RecruitmentStatus>(2, "Passed");
        public static readonly RecruitmentStatus Rejected = New<RecruitmentStatus>(3, "Rejected");
    }
}
