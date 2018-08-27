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

        public FinanceAdminUnit FinanceAdminUnit { get; set; }
        public ProductionUnit ProductionUnit { get; set; }
        public OperationUnit OperationUnit { get; set; }
        public HumanResourceUnit HumanResourceUnit { get; set; }
        public MarketingManagementUnit MarketingManagementUnit { get; set; }
        public ProcurementManagementUnit ProcurementManagementUnit { get; set; }
        public ProjectManagementUnit ProjectManagementUnit { get; set; }

        public BusinessDevelopmentUnit BusinessDevelopmentUnit { get; set; }
        public SalesManagementUnit SalesManagementUnit { get; set; }
        public SalesSupervision SalesSupervision { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
        public DateTime? EmploymentDate { get; set; }
        public DateTime? FirstEvaluationDate { get; set; }
        public decimal BasicSalary { get; set; }
        public UserStatus UserStatus { get; set; }
        public SalaryCurrency SalaryCurrency { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
