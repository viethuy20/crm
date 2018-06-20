using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class KPIStatus : Enumeration
    {
        public static readonly KPIStatus All = New<KPIStatus>("", "All");
        public static readonly KPIStatus KPI = New<KPIStatus>(2, "KPI");
        public static readonly KPIStatus NoKPI = New<KPIStatus>(3, "No KPI");
        public static readonly KPIStatus NoCheck = New<KPIStatus>(4, "No Check KPI");
    }
}
