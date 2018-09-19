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
        public static readonly NotifyType NewEvent = New<NotifyType>(3, "New Event");
    }
}
