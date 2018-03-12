using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class LeadStatus : Enumeration
    {
        public static readonly LeadStatus Initial = New<LeadStatus>(1, "Initial");
        public static readonly LeadStatus Reject = New<LeadStatus>(2, "Reject");//Reject request NCL or LOI
        public static readonly LeadStatus Live = New<LeadStatus>(3, "Live");//Approval NCL
        public static readonly LeadStatus Blocked = New<LeadStatus>(4, "Blocked");
        public static readonly LeadStatus LOI = New<LeadStatus>(5, "LOI");//Approval LOI
        public static readonly LeadStatus Booked = New<LeadStatus>(6, "Booked");
        public static readonly LeadStatus RequestNCL = New<LeadStatus>(7, "Request NCL");
        public static readonly LeadStatus RequestLOI = New<LeadStatus>(8, "Request LOI");
    }
}
