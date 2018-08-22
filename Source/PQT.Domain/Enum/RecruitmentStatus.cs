using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class RecruitmentStatus : Enumeration
    {
        public static readonly RecruitmentStatus Passed = New<RecruitmentStatus>(1, "Passed");//request to ma& finance
        public static readonly RecruitmentStatus Rejected = New<RecruitmentStatus>(2, "Rejected");
    }
    public class Interviewer: Enumeration
    {
        public static readonly Interviewer None = New<Interviewer>(0, "None");
        public static readonly Interviewer BusinessDevelopment = New<Interviewer>(1, "Business Development Unit");
        public static readonly Interviewer FinAdm = New<Interviewer>(2, "Fin&Adm Unit");
        public static readonly Interviewer SalesManagement = New<Interviewer>(3, "Sales Management Unit");
        public static readonly Interviewer ProjectManagement = New<Interviewer>(4, "Project Management Unit");
    }


}
