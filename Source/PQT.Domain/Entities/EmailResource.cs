using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Mail;
using System.Text;
using NS;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class EmailResource : Entity
    {
        public EmailResource()
        {
        }
        public int PassRound { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public int? UserReportID { get; set; }
        [ForeignKey("UserReportID")]
        public virtual User UserReport { get; set; }
        public string Remarks { get; set; }
        public string ReportRemarks { get; set; }
        public int? SublistEmailID { get; set; }
        [ForeignKey("SublistEmailID")]
        public virtual SublistEmail SublistEmail { get; set; }
    }

    public class SublistEmail : Entity
    {
        public SublistEmail()
        {
        }
        public string Description { get; set; }
        public int? RoundRecordID { get; set; }
        [ForeignKey("RoundRecordID")]
        public virtual RoundRecord RoundRecord { get; set; }
        public DateTime AssignDate { get; set; }
        public int AssignUserID { get; set; }
        [ForeignKey("AssignUserID")]
        public virtual User AssignUser { get; set; }
        public string Status { get; set; }
        public string RejectionRemarks { get; set; }
    }


    public class RoundRecord : RoundRecord<RoundStatus>
    {
        public RoundRecord(int entryId)
            : base(entryId)
        {
        }

        public RoundRecord(int entryId, RoundStatus status, string message = "")
            : base(entryId, status, message)
        {
        }
    }


    public abstract class RoundRecord<T> : EntityBase
        where T : Enumeration
    {
        #region Factory

        protected RoundRecord(int entryID)
            : this(entryID, Enumeration.GetAll<T>().FirstOrDefault())
        {
        }

        protected RoundRecord(int entryID, T status, string message = "")
            : this()
        {
            EntryId = entryID;
            Status = status;
            Message = message;
        }

        protected RoundRecord()
        {
            UpdatedTime = DateTime.Now;
        }

        #endregion

        #region Properties

        public string Message { get; set; }
        public int EntryId { get; set; }
        public DateTime UpdatedTime { get; set; }
        public T Status { get; set; }

        #endregion

        #region Conversion

        public static implicit operator T(RoundRecord<T> record)
        {
            return record.Status;
        }

        #endregion

        public override string ToString()
        {
            return Status.DisplayName;
        }
    }
}
