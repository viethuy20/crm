using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class ReservationStatus : Enumeration
    {
        public static readonly LeadStatus Initial = New<LeadStatus>(1, "Initial");
        public static readonly LeadStatus Cancelled = New<LeadStatus>(2, "Cancelled");
        public static readonly LeadStatus Paid = New<LeadStatus>(3, "Paid");
    }
}
