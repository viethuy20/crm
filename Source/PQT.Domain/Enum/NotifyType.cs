using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class NotifyType : Enumeration
    {
        public static readonly NotifyType Lead = New<NotifyType>(1, "Lead");
        public static readonly NotifyType Booking = New<NotifyType>(2, "Booking");
        public static readonly NotifyType Invoice = New<NotifyType>(3, "Invoice");
        public static readonly NotifyType Recruitment = New<NotifyType>(4, "Recruitment");
        public static readonly NotifyType OpeEvent = New<NotifyType>(5, "Ope Event");
        public static readonly NotifyType NewEvent = New<NotifyType>(6, "New Event");
    }
    public class NotifyAction : Enumeration
    {
        public static readonly NotifyAction Alert = New<NotifyAction>(1, "Alert");
        public static readonly NotifyAction Confirm = New<NotifyAction>(2, "Confirm");
        public static readonly NotifyAction Request = New<NotifyAction>(3, "Request");
        public static readonly NotifyAction CancelRequest = New<NotifyAction>(4, "Cancel Request");
        public static readonly NotifyAction Approved = New<NotifyAction>(5, "Approved");
        public static readonly NotifyAction Rejected = New<NotifyAction>(6, "Rejected");
        public static readonly NotifyAction Assign = New<NotifyAction>(7, "Assign");
    }
}
