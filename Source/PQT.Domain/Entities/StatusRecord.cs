using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using PQT.Domain.Helpers;
using NS;

namespace PQT.Domain.Entities
{
    public abstract class StatusRecord<T> : EntityBase
        where T : Enumeration
    {
        #region Factory

        protected StatusRecord(int entryID)
            : this(entryID, Enumeration.GetAll<T>().FirstOrDefault())
        {
        }

        protected StatusRecord(int entryID, T status, string message = "")
            : this()
        {
            EntryId = entryID;
            Status = status;
            Message = message;
        }
        protected StatusRecord(int entryID, T status, int? userId, string message = "")
            : this()
        {
            EntryId = entryID;
            UserId = userId;
            Status = status;
            Message = message;
        }

        protected StatusRecord()
        {
            UpdatedTime = DateTime.Now;
        }

        #endregion

        #region Properties

        public string Message { get; set; }
        public int EntryId { get; set; }
        public int? UserId { get; set; }
        public DateTime UpdatedTime { get; set; }
        public T Status { get; set; }


        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        #endregion

        #region Conversion

        public static implicit operator T(StatusRecord<T> record)
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
    }
}
