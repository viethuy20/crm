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
    }
}
