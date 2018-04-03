using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using NS;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class LeadStatusRecord : LeadStatusRecord<LeadStatus>
    {
        public LeadStatusRecord()
        {
        }

        public LeadStatusRecord(int entryId)
            : base(entryId)
        {
        }

        public LeadStatusRecord(int entryId, LeadStatus status, int? userId, string message = "")
            : base(entryId, status, userId, message)
        {
        }
        public LeadStatusRecord(int entryId, LeadStatus status, int? userId, string attachment, string message = "")
            : base(entryId, status, userId, attachment, message)
        {
        }
    }


    public abstract class LeadStatusRecord<T> : EntityBase
        where T : Enumeration
    {
        #region Factory

        protected LeadStatusRecord(int entryID)
            : this(entryID, Enumeration.GetAll<T>().FirstOrDefault())
        {
        }

        protected LeadStatusRecord(int entryID, T status, string message = "")
            : this()
        {
            EntryId = entryID;
            Status = status;
            Message = message;
        }
        protected LeadStatusRecord(int entryID, T status, int? userId, string attachment, string message = "")
            : this()
        {
            EntryId = entryID;
            UserId = userId;
            Status = status;
            Message = message;
            Attachment = attachment;
        }
        protected LeadStatusRecord(int entryID, T status, int? userId, string message = "")
            : this()
        {
            EntryId = entryID;
            UserId = userId;
            Status = status;
            Message = message;
        }

        protected LeadStatusRecord()
        {
            UpdatedTime = DateTime.Now;
        }

        #endregion

        #region Properties

        public string Message { get; set; }
        public string Attachment { get; set; }
        public int EntryId { get; set; }
        public int? UserId { get; set; }
        public DateTime UpdatedTime { get; set; }
        public T Status { get; set; }


        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        #endregion

        #region Conversion

        public static implicit operator T(LeadStatusRecord<T> record)
        {
            return record.Status;
        }

        #endregion

        public string UserDisplay()
        {
            return User != null ? User.DisplayName : "";
        }
        public override string ToString()
        {
            return Status.DisplayName;
        }

        public string AttachmentUrl
        {
            get { return "/data/lead/" + Attachment; }
        }
    }
}
