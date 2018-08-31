using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using NS;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public abstract class InterviewSummary : EntityBase
    {
        //#region Factory

        protected InterviewSummary(int entryID, RecruitmentStatus status, int? userId, string remarks, DateTime? dateSelected)
        {
            EntryId = entryID;
            UserId = userId;
            Status = status;
            Remarks = remarks;
            DateSelected = dateSelected;
            UpdatedTime = DateTime.Now;
        }

        //#endregion
        public InterviewSummary()
        {
            UpdatedTime = DateTime.Now;
        }
        #region Properties

        public string Remarks { get; set; }
        public string ReasonRejected { get; set; }
        public int EntryId { get; set; }
        public DateTime? DateSelected { get; set; }
        public DateTime UpdatedTime { get; set; }
        public RecruitmentStatus Status { get; set; }

        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; } // Interviewer
        #endregion

        public override string ToString()
        {
            return Status.DisplayName;
        }

        public string DateSelectedDisplay
        {
            get { return DateSelected != null ? Convert.ToDateTime(DateSelected).ToString("dd/MM/yyyy") : ""; }
        }
        public string Interviewer
        {
            get { return User != null ? User.DisplayName : ""; }
        }
        public string StatusDisplay
        {
            get { return Status.DisplayName; }
        }
    }

    public class PsSummary : InterviewSummary
    {

    }
    public class OneFaceToFaceSummary : InterviewSummary
    {

    }
    public class TwoFaceToFaceSummary : InterviewSummary
    {

    }
}
