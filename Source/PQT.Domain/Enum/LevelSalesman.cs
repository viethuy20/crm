using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class BusinessDevelopmentUnit : Enumeration
    {
        public static readonly BusinessDevelopmentUnit None = New<BusinessDevelopmentUnit>(0, "None");
        public static readonly BusinessDevelopmentUnit Level1 = New<BusinessDevelopmentUnit>(1, "Regional Sales Director");
        public static readonly BusinessDevelopmentUnit Level2 = New<BusinessDevelopmentUnit>(2, "Regional Advisor");
        public static readonly BusinessDevelopmentUnit Level3 = New<BusinessDevelopmentUnit>(3, "Senior Business Partner");
        public static readonly BusinessDevelopmentUnit Level4 = New<BusinessDevelopmentUnit>(4, "Business Partner");
        public static readonly BusinessDevelopmentUnit Level5 = New<BusinessDevelopmentUnit>(5, "Business Development Team Lead");
        public static readonly BusinessDevelopmentUnit Level6 = New<BusinessDevelopmentUnit>(6, "Senior Business Development Executive");
        public static readonly BusinessDevelopmentUnit Level7 = New<BusinessDevelopmentUnit>(7, "Business Development Executive");
        public static readonly BusinessDevelopmentUnit Level8 = New<BusinessDevelopmentUnit>(8, "Business Development Trainee Tier 1");
        public static readonly BusinessDevelopmentUnit Level9 = New<BusinessDevelopmentUnit>(9, "Business Development Trainee");
        public static readonly BusinessDevelopmentUnit Level10 = New<BusinessDevelopmentUnit>(10, "Business Development Intern");
    }
    public class SalesManagementUnit : Enumeration
    {
        public static readonly SalesManagementUnit None = New<SalesManagementUnit>(0, "None");
        public static readonly SalesManagementUnit Level1 = New<SalesManagementUnit>(1, "Regional Sales Director");
        public static readonly SalesManagementUnit Level2 = New<SalesManagementUnit>(2, "Regional Senior Sales Manager");
        public static readonly SalesManagementUnit Level3 = New<SalesManagementUnit>(3, "Regional Sales Manager");
        public static readonly SalesManagementUnit Level4 = New<SalesManagementUnit>(4, "Regional Assistant Sales Manager");
        public static readonly SalesManagementUnit Level5 = New<SalesManagementUnit>(5, "Sales Manager");
        public static readonly SalesManagementUnit Level6 = New<SalesManagementUnit>(6, "Assistant Sales Manager");
        public static readonly SalesManagementUnit Level7 = New<SalesManagementUnit>(7, "Team Lead Sales Management");
        public static readonly SalesManagementUnit Level8 = New<SalesManagementUnit>(8, "Senior Sales Management Executive");
        public static readonly SalesManagementUnit Level9 = New<SalesManagementUnit>(9, "Sales Management Executive");
        public static readonly SalesManagementUnit Level10 = New<SalesManagementUnit>(10, "Sales Management Trainee");
    }
    public class SalesSupervision : Enumeration
    {
        public static readonly SalesSupervision None = New<SalesSupervision>(0, "None");
        public static readonly SalesSupervision Level1 = New<SalesSupervision>(1, "Sales Supervision Trainee");
    }
    public class FinanceAdminUnit : Enumeration
    {
        public static readonly FinanceAdminUnit None = New<FinanceAdminUnit>(0, "None");
        public static readonly FinanceAdminUnit Intern = New<FinanceAdminUnit>(1, "Finance & Admin Intern");
        public static readonly FinanceAdminUnit Manager = New<FinanceAdminUnit>(2, "Finance & Admin Manager");
    }
    public class ProductionUnit : Enumeration
    {
        public static readonly ProductionUnit None = New<ProductionUnit>(0, "None");
        public static readonly ProductionUnit Trainee = New<ProductionUnit>(1, "Production Trainee");
        public static readonly ProductionUnit Executive = New<ProductionUnit>(2, "Production Executive");
        public static readonly ProductionUnit Supervisor = New<ProductionUnit>(3, "Production Supervisor");
    }
    public class OperationUnit  : Enumeration
    {
        public static readonly OperationUnit None = New<OperationUnit>(0, "None");
        public static readonly OperationUnit Trainee = New<OperationUnit>(1, "Operation Trainee");
        public static readonly OperationUnit Executive = New<OperationUnit>(2, "Operation Executive");
        public static readonly OperationUnit AssistantManager = New<OperationUnit>(3, "Assistant Operation Manager");
        public static readonly OperationUnit Manager = New<OperationUnit>(4, "Operation Manager");
        public static readonly OperationUnit Director = New<OperationUnit>(5, "Operation Director");
    }
    public class HumanResourceUnit : Enumeration
    {
        public static readonly HumanResourceUnit None = New<HumanResourceUnit>(0, "None");
        public static readonly HumanResourceUnit Coordinator = New<HumanResourceUnit>(1, "HR Coordinator");
    }
    public class MarketingManagementUnit : Enumeration
    {
        public static readonly MarketingManagementUnit None = New<MarketingManagementUnit>(0, "None");
        public static readonly MarketingManagementUnit Intern = New<MarketingManagementUnit>(1, "Marketing Intern");
        public static readonly MarketingManagementUnit Trainee = New<MarketingManagementUnit>(2, "Marketing Trainee");
        public static readonly MarketingManagementUnit Executive = New<MarketingManagementUnit>(3, "Marketing Executive");
    }
    public class ProcurementManagementUnit  : Enumeration
    {
        public static readonly ProcurementManagementUnit None = New<ProcurementManagementUnit>(0, "None");
        public static readonly ProcurementManagementUnit Trainee = New<ProcurementManagementUnit>(1, "Procurement Trainee");
        public static readonly ProcurementManagementUnit Executive = New<ProcurementManagementUnit>(2, "Procurement Executive");
    }
    public class ProjectManagementUnit  : Enumeration
    {
        public static readonly ProjectManagementUnit None = New<ProjectManagementUnit>(0, "None");
        public static readonly ProjectManagementUnit Coordinator = New<ProjectManagementUnit>(1, "Project Coordinator");
        public static readonly ProjectManagementUnit Manager = New<ProjectManagementUnit>(2, "Project Manager");
        public static readonly ProjectManagementUnit SeniorManager = New<ProjectManagementUnit>(3, "Senior Project Manager");
        public static readonly ProjectManagementUnit Director = New<ProjectManagementUnit>(4, "Project Director");
    }
}
