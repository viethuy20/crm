using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class UserSalaryHistory : EntityBase
    {
        public UserSalaryHistory()
        {
            CreatedAt = DateTime.Now;
        }
        public LevelSalesman LevelSalesman { get; set; }
        public DateTime? DateOfContract { get; set; }
        public DateTime? DateOfStarting { get; set; }
        public decimal BasicSalary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
