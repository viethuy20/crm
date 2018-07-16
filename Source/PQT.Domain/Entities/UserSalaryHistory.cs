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
        public BusinessDevelopmentUnit BusinessDevelopmentUnit { get; set; }
        public SalesManagementUnit SalesManagementUnit { get; set; }
        public SalesSupervision SalesSupervision { get; set; }
        public DateTime? DateOfContract { get; set; }
        public DateTime? DateOfStarting { get; set; }
        public decimal BasicSalary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
