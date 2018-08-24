using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class PaymentStatus : Enumeration
    {
        public static readonly PaymentStatus Initial = New<PaymentStatus>(0, "Live");
        public static readonly PaymentStatus Paid = New<PaymentStatus>(1, "Paid");
        public static readonly PaymentStatus Cancelled = New<PaymentStatus>(2, "Cancelled");
        public static readonly PaymentStatus PartialCancelledPaid = New<PaymentStatus>(3, "Partial Cancelled & Paid");
    }

    public class AttendanceStatus : Enumeration
    {
        public static readonly AttendanceStatus Initial = New<AttendanceStatus>(0, "Live");
        public static readonly AttendanceStatus Replaced = New<AttendanceStatus>(1, "Attended");
        public static readonly AttendanceStatus Cancelled = New<AttendanceStatus>(2, "Cancelled");
    }
    public class DelegateAttendanceStatus : Enumeration
    {
        public static readonly DelegateAttendanceStatus Initial = New<DelegateAttendanceStatus>(0, "Live");
        public static readonly DelegateAttendanceStatus Replaced = New<DelegateAttendanceStatus>(1, "Replaced");
        public static readonly DelegateAttendanceStatus Cancelled = New<DelegateAttendanceStatus>(2, "Cancelled");
        public static readonly DelegateAttendanceStatus Replacement = New<DelegateAttendanceStatus>(3, "Replacement");
    }

}
