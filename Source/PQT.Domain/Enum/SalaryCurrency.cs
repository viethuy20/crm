using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class SalaryCurrency : Enumeration
    {
        public static readonly SalaryCurrency VND = New<SalaryCurrency>(1, "VND");
        public static readonly SalaryCurrency USD = New<SalaryCurrency>(2, "USD");
        public static readonly SalaryCurrency SGD = New<SalaryCurrency>(3, "SGD");
        public static readonly SalaryCurrency EUR = New<SalaryCurrency>(4, "EUR");
        public static readonly SalaryCurrency AED = New<SalaryCurrency>(5, "AED ");
        public static readonly SalaryCurrency GBP = New<SalaryCurrency>(6, "GBP");
    }
}
