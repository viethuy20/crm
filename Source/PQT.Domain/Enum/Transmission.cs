using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class Transmission : Enumeration
    {
        public static readonly Transmission None = New<Transmission>(0, "None");
        public static readonly Transmission Auto = New<Transmission>(1, "Automatic");
        public static readonly Transmission Manual = New<Transmission>(2, "Manual");
    }
}
