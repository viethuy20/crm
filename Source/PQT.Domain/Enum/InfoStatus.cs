using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class InfoStatus : Enumeration
    {
        public static readonly InfoStatus Initial = New<InfoStatus>(1, "Request Info");//request to ma& finance
        public static readonly InfoStatus Approved = New<InfoStatus>(2, "Approved");
        public static readonly InfoStatus Rejected = New<InfoStatus>(3, "Rejected");
    }
}
