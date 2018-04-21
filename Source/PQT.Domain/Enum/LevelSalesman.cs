using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class LevelSalesman : Enumeration
    {
        public static readonly LevelSalesman None = New<LevelSalesman>(0, "None");
        public static readonly LevelSalesman Level1 = New<LevelSalesman>(1, "Level 1");
        public static readonly LevelSalesman Level2 = New<LevelSalesman>(2, "Level 2");
        public static readonly LevelSalesman Level3 = New<LevelSalesman>(3, "Level 3");
        public static readonly LevelSalesman Level4 = New<LevelSalesman>(4, "Level 4");
        public static readonly LevelSalesman Level5 = New<LevelSalesman>(5, "Level 5");
    }
}
