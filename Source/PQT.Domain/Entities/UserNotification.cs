using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class UserNotification : EntityBase
    {
        public UserNotification()
        {
            CreatedTime = DateTime.Now;
        }
        public int UserID { get; set; }
        public int EntryId { get; set; }
        public bool Seen { get; set; }
        public NotifyType NotifyType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string HighlightColor { get; set; }
        public DateTime CreatedTime { get; set; }

        public string NotifyTypeDisplay
        {
            get { return NotifyType.DisplayName; }
        }
    }
}
