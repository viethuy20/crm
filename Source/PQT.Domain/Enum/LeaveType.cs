using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class LeaveStatus : Enumeration
    {
        public static readonly LeaveStatus None = New<LeaveStatus>("", "None");
        public static readonly LeaveStatus Request = New<LeaveStatus>(1, "Request");
        public static readonly LeaveStatus Approved = New<LeaveStatus>(2, "Approved");
        public static readonly LeaveStatus Rejected = New<LeaveStatus>(3, "Rejected");
    }
    public class LeaveType : Enumeration
    {
        public static readonly LeaveType None = New<LeaveType>("", "None");
        public static readonly LeaveType Leave = New<LeaveType>(1, "Leave");
        public static readonly LeaveType Lateness = New<LeaveType>(2, "Lateness");
        public static readonly LeaveType Resignation = New<LeaveType>(3, "Resignation");
    }
    public class TypeOfLeave : Enumeration
    {
        public static readonly TypeOfLeave None = New<TypeOfLeave>("", "None");
        public static readonly TypeOfLeave Annual = New<TypeOfLeave>(1, "Annual Leave");
        public static readonly TypeOfLeave MedicalPaid = New<TypeOfLeave>(2, "Medical Paid Leave");
        public static readonly TypeOfLeave PersonalPaid = New<TypeOfLeave>(3, "Personal paid leave");
        public static readonly TypeOfLeave Maternity = New<TypeOfLeave>(4, "Maternity Leave");
        public static readonly TypeOfLeave FullDayUnpaid = New<TypeOfLeave>(5, "Full day Unpaid Leave");
        public static readonly TypeOfLeave HalfDayUnpaid = New<TypeOfLeave>(6, "Half day Unpaid Leave");
        public static readonly TypeOfLeave Absentism = New<TypeOfLeave>(7, "Absentism");
    }
    public class TypeOfLatenes : Enumeration
    {
        public static readonly TypeOfLatenes None = New<TypeOfLatenes>("", "None");
        public static readonly TypeOfLatenes Late05MinInform = New<TypeOfLatenes>(1, "Late 05 minutes or less (With inform)");
        public static readonly TypeOfLatenes Late14MinInform = New<TypeOfLatenes>(2, "Late 14 minutes or less (With inform)");
        public static readonly TypeOfLatenes Late15MinInform = New<TypeOfLatenes>(3, "Late beyond 15 minutes (With inform)");
        public static readonly TypeOfLatenes Late05MinNoInform = New<TypeOfLatenes>(4, "Late 05 minutes or less (Without inform)");
        public static readonly TypeOfLatenes Late14MinNoInform = New<TypeOfLatenes>(5, "Late 14 minutes or less (Without inform)");
        public static readonly TypeOfLatenes Late15MinNoInform = New<TypeOfLatenes>(6, "Late beyond 15 minutes (Without inform) ");
    }
}
