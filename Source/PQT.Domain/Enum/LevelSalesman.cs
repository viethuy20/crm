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
}
