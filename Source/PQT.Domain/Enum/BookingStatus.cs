using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class BookingStatus : Enumeration
    {
        public static readonly BookingStatus Deleted = New<BookingStatus>(0, "Deleted");
        public static readonly BookingStatus Initial = New<BookingStatus>(1, "Request Booking");//request to ma& finance
        public static readonly BookingStatus Approved = New<BookingStatus>(2, "Approved");
        public static readonly BookingStatus Rejected = New<BookingStatus>(3, "Rejected");
    }
}
