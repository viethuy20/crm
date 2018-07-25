using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class PaymentStatus : Enumeration
    {
        public static readonly PaymentStatus Initial = New<PaymentStatus>("", "Initial");
        public static readonly PaymentStatus Paid = New<PaymentStatus>(1, "Paid");
        public static readonly PaymentStatus Cancelled = New<PaymentStatus>(2, "Cancelled");
        public static readonly PaymentStatus PartialCancelledPaid = New<PaymentStatus>(3, "Partial Cancelled & Paid");
    }
}
