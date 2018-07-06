using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;

namespace PQT.Domain.Entities
{
    public class UserNotification : EntityBase
    {
        public UserNotification()
        {
            CreatedTime = DateTime.Now;
        }
        public int UserID { get; set; }
        public int EntryId { get; set; } // as leadId
        public int EventId { get; set; }
        public bool Seen { get; set; }
        public NotifyType NotifyType { get; set; }

        public string NotifyTypeCode
        {
            get { return NotifyType != null ? NotifyType.Value : ""; }
        }
        public string EventCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string HighlightColor { get; set; }
        public DateTime CreatedTime { get; set; }
        [NotMapped]
        public int ReloadNCL { get; set; }
        public string NotifyTypeDisplay
        {
            get { return NotifyType.DisplayName; }
        }

        public string TimeAgo
        {
            get
            {
                return CreatedTime.TimeAgo();
            }
        }
        public string TimeToString
        {
            get
            {
                return CreatedTime.ToString("ddMMyyyyHHmmss");
            }
        }
        public string Timestamp
        {
            get
            {
                return String.Format("{0} at {1}", CreatedTime.ToString("dd MMM yyyy"),
                    CreatedTime.ToString("HH:mm"));
            }
        }
    }
}
