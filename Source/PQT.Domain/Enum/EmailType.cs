using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class EmailType : Enumeration
    {
        public static readonly EmailType To = New<EmailType>(0, "To");
        public static readonly EmailType Cc = New<EmailType>(1, "Cc");
        public static readonly EmailType Bcc = New<EmailType>(2, "Bcc");
    }
}
