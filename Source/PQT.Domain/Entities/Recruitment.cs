using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Recruitment : Entity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
        public string PersonalEmail { get; set; }
        public string ApplicationSource { get; set; }
        public string Resume { get; set; }
        public int? OfficeLocationID { get; set; }
        [ForeignKey("OfficeLocationID")]
        public OfficeLocation OfficeLocation { get; set; }

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
        public RecruitmentPosition RecruitmentPosition { get; set; }
    }

    public class RecruitmentPosition : EntityBase
    {
        public string Department { get; set; }
        public string Position { get; set; }
    }
}
