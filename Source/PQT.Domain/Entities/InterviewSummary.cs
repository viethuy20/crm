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

        //protected InterviewSummary(int entryID, RecruitmentStatus status, int? userId, string remarks, DateTime dateSelected, Interviewer interviewer)
        //{
        //    EntryId = entryID;
        //    UserId = userId;
        //    Status = status;
        //    Remarks = remarks;
        //    DateSelected = dateSelected;
        //    Interviewer = interviewer;
        //    UpdatedTime = DateTime.Now;
        //}

        //#endregion

        #region Properties

        public string Remarks { get; set; }
        public int EntryId { get; set; }
        public DateTime DateSelected { get; set; }
        public DateTime UpdatedTime { get; set; }
        public Interviewer Interviewer { get; set; }
        public RecruitmentStatus Status { get; set; }

        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        #endregion
        
        public string UserDisplay()
        {
            return User != null ? User.DisplayName : "";
        }
        public override string ToString()
        {
            return Status.DisplayName;
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
