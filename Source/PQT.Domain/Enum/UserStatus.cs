using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class UserStatus : Enumeration
    {
        public static readonly UserStatus Live = New<UserStatus>(1, "Live");
        public static readonly UserStatus Terminated = New<UserStatus>(2, "Terminated");
        public static readonly UserStatus Resigned = New<UserStatus>(3, "Resigned");
    }
}
