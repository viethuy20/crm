using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class LeadStatus : Enumeration
    {
        public static readonly LeadStatus Initial = New<LeadStatus>(1, "Initial");
        public static readonly LeadStatus RequestNCL = New<LeadStatus>(2, "Request NCL");
        public static readonly LeadStatus RequestLOI = New<LeadStatus>(3, "Request LOI");
        public static readonly LeadStatus RequestBook = New<LeadStatus>(4, "Request Booking");
        public static readonly LeadStatus Booked = New<LeadStatus>(5, "Booked");
        public static readonly LeadStatus Live = New<LeadStatus>(6, "Live");//Approval NCL
        public static readonly LeadStatus LOI = New<LeadStatus>(7, "LOI");//Approval LOI
        public static readonly LeadStatus Blocked = New<LeadStatus>(8, "Blocked");
        public static readonly LeadStatus Reject = New<LeadStatus>(9, "Reject");//Reject request NCL or LOI or Book
    }

    public class FinalStatus : Enumeration
    {
        public static readonly FinalStatus InterestsCertification = New<FinalStatus>(1, "Interests | Certification");
        public static readonly FinalStatus InterestsTopic = New<FinalStatus>(2, "Interests | Topic?");
        public static readonly FinalStatus InterestsTrainerProfile = New<FinalStatus>(3, "Interests | Trainer Profile");
        public static readonly FinalStatus InterestsOthers = New<FinalStatus>(4, "Interests | Others?");
        public static readonly FinalStatus RejectedBudget = New<FinalStatus>(5, "Rejected | Budget");
        public static readonly FinalStatus RejectedPricing = New<FinalStatus>(6, "Rejected | Pricing");
        public static readonly FinalStatus RejectedAvailability = New<FinalStatus>(7, "Rejected | Availability");
        public static readonly FinalStatus RejectedProcedure = New<FinalStatus>(8, "Rejected | Procedure");
        public static readonly FinalStatus RejectedFinancialYear = New<FinalStatus>(9, "Rejected | Financial Year");
        public static readonly FinalStatus RejectedOthers = New<FinalStatus>(10, "Rejected | Others?");
        public static readonly FinalStatus Pending = New<FinalStatus>(11, "Pending");
        public static readonly FinalStatus Neutral = New<FinalStatus>(12, "Neutral");
    }
    public class FollowUpStatus : Enumeration
    {
        public static readonly FollowUpStatus InterestsCertification = New<FollowUpStatus>(1, "Interests | Certification");
        public static readonly FollowUpStatus InterestsTopic = New<FollowUpStatus>(2, "Interests | Topic?");
        public static readonly FollowUpStatus InterestsTrainerProfile = New<FollowUpStatus>(3, "Interests | Trainer Profile");
        public static readonly FollowUpStatus InterestsOthers = New<FollowUpStatus>(4, "Interests | Others?");
        public static readonly FollowUpStatus RejectedBudget = New<FollowUpStatus>(5, "Rejected | Budget");
        public static readonly FollowUpStatus RejectedPricing = New<FollowUpStatus>(6, "Rejected | Pricing");
        public static readonly FollowUpStatus RejectedAvailability = New<FollowUpStatus>(7, "Rejected | Availability");
        public static readonly FollowUpStatus RejectedProcedure = New<FollowUpStatus>(8, "Rejected | Procedure");
        public static readonly FollowUpStatus RejectedFinancialYear = New<FollowUpStatus>(9, "Rejected | Financial Year");
        public static readonly FollowUpStatus RejectedOthers = New<FollowUpStatus>(10, "Rejected | Others?");
        public static readonly FollowUpStatus Pending = New<FollowUpStatus>(11, "Pending");
        public static readonly FollowUpStatus Neutral = New<FollowUpStatus>(12, "Neutral");
    }
}
