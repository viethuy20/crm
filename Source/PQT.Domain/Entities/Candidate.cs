using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NS;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;

namespace PQT.Domain.Entities
{
    public class Candidate : Entity
    {
        public Candidate()
        {
        }
        public string CandidateNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EnglishName { get; set; }
        public string MobileNumber { get; set; }
        public string PersonalEmail { get; set; }
        public string ApplicationSource { get; set; }
        public string Remarks { get; set; }
        public string Resume { get; set; }
        public int? OfficeLocationID { get; set; }
        [ForeignKey("OfficeLocationID")]
        public virtual OfficeLocation OfficeLocation { get; set; }

        public int? CandidateStatusID { get; set; }
        [ForeignKey("CandidateStatusID")]
        public virtual CandidateStatusRecord CandidateStatusRecord { get; set; }

        public int? PsSummaryID { get; set; }
        [ForeignKey("PsSummaryID")]
        public virtual PsSummary PsSummary { get; set; }

        public int? OneFaceToFaceSummaryID { get; set; }
        [ForeignKey("OneFaceToFaceSummaryID")]
        public virtual OneFaceToFaceSummary OneFaceToFaceSummary { get; set; }//1 Face2Face

        public int? TwoFaceToFaceSummaryID { get; set; }
        [ForeignKey("TwoFaceToFaceSummaryID")]
        public virtual TwoFaceToFaceSummary TwoFaceToFaceSummary { get; set; }//2 Face2Face

        public int? RecruitmentPositionID { get; set; }
        [ForeignKey("RecruitmentPositionID")]
        public virtual RecruitmentPosition RecruitmentPosition { get; set; }
        public int? UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        public int? EmployeeID { get; set; }
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
        public string OfficeLocationDisplay
        {
            get
            {
                if (OfficeLocation != null) return OfficeLocation.Name;
                return "";
            }
        }
        public string StatusDisplay
        {
            get
            {
                if (CandidateStatusRecord != null) return CandidateStatusRecord.Status.DisplayName;
                return "";
            }
        }
        public string StatusUpdateTimeStr
        {
            get
            {
                if (CandidateStatusRecord != null) return CandidateStatusRecord.UpdatedTime.ToString("dd/MM/yyyy");
                return "";
            }
        }
        public string StatusCode
        {
            get
            {
                if (CandidateStatusRecord != null) return CandidateStatusRecord.Status.Value;
                return "";
            }
        }
        public string ClassStatus
        {
            get
            {
                return StringHelper.RemoveSpecialCharacters(StatusDisplay);
            }
        }
        public string RecruitmentPositionDisplay
        {
            get
            {
                if (RecruitmentPosition != null) return RecruitmentPosition.Description;
                return "";
            }
        }
        public string PsSummaryStatusDisplay
        {
            get
            {
                if (PsSummary != null) return PsSummary.Status.DisplayName;
                return "";
            }
        }
        public string PsSummaryStatusReason
        {
            get
            {
                if (PsSummary != null) return PsSummary.ReasonRejected;
                return "";
            }
        }

        public string OneFaceToFaceSummaryStatusDisplay
        {
            get
            {
                if (OneFaceToFaceSummary != null) return OneFaceToFaceSummary.Status.DisplayName;
                return "";
            }
        }
        public string OneFaceToFaceSummaryStatusReason
        {
            get
            {
                if (OneFaceToFaceSummary != null) return OneFaceToFaceSummary.ReasonRejected;
                return "";
            }
        }

        public string TwoFaceToFaceSummaryStatusDisplay
        {
            get
            {
                if (TwoFaceToFaceSummary != null) return TwoFaceToFaceSummary.Status.DisplayName;
                return "";
            }
        }
        public string TwoFaceToFaceSummaryStatusReason
        {
            get
            {
                if (TwoFaceToFaceSummary != null) return TwoFaceToFaceSummary.ReasonRejected;
                return "";
            }
        }

        public string SalesmanName
        {
            get
            {
                if (User != null)
                {
                    return User.DisplayName;
                }

                return "";
            }
        }
    }

    public class RecruitmentPosition : EntityBase
    {
        public RecruitmentPosition()
        {
            RecruitmentPositionStatus = RecruitmentPositionStatus.Request;
        }
        public string Department { get; set; }
        public string Position { get; set; }
        public string PositionNo { get; set; }
        public int? OfficeLocationID { get; set; }
        [ForeignKey("OfficeLocationID")]
        public virtual OfficeLocation OfficeLocation { get; set; }

        public RecruitmentPositionStatus RecruitmentPositionStatus { get; set; }
        public DateTime? StatusDateTime { get; set; }
        public string StatusMessage { get; set; }

        public string Description { get { return OfficeLocationName + " - " + Department + " - " + Position; } }
        public string OfficeLocationName
        {
            get
            {
                if (OfficeLocation != null)
                    return OfficeLocation.Name;
                return "";
            }
        }
        public string StatusDateTimeDisplay
        {
            get
            {
                if (StatusDateTime != null)
                    return Convert.ToDateTime(StatusDateTime).ToString("dd/MM/yyyy HH:mm:ss");
                return "";
            }
        }
        public string RecruitmentPositionStatusDisplay
        {
            get
            {
                if (RecruitmentPositionStatus != null)
                    return RecruitmentPositionStatus.DisplayName;
                return "";
            }
        }
        public string RecruitmentPositionStatusDisplayHtml
        {
            get
            {
                if (RecruitmentPositionStatus != null &&
                    RecruitmentPositionStatus == RecruitmentPositionStatus.Rejected)
                    return RecruitmentPositionStatus.DisplayName +
                        "<br/><span class='text-danger'>" + StatusMessage + "</span>";
                if (RecruitmentPositionStatus != null)
                    return RecruitmentPositionStatus.DisplayName;
                return "";
            }
        }
    }

    public class RecruitmentPositionStatus : Enumeration
    {
        public static readonly RecruitmentPositionStatus Deleted = New<RecruitmentPositionStatus>(0, "Deleted");
        public static readonly RecruitmentPositionStatus Request = New<RecruitmentPositionStatus>(1, "Request");
        public static readonly RecruitmentPositionStatus Approved = New<RecruitmentPositionStatus>(2, "Approved");
        public static readonly RecruitmentPositionStatus Rejected = New<RecruitmentPositionStatus>(3, "Rejected");
    }
}
