using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class NameTitle : Enumeration
    {
        public static readonly NameTitle Mr = New<NameTitle>("Mr", "Mr");
        public static readonly NameTitle Mrs = New<NameTitle>("Mrs", "Mrs");
        public static readonly NameTitle Ms = New<NameTitle>("Ms", "Ms");
        public static readonly NameTitle Mdm = New<NameTitle>("Mdm", "Mdm");
        public static readonly NameTitle Dr = New<NameTitle>("Dr", "Dr");
    }
}
