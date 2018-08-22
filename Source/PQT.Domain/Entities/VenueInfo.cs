using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using NS;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class VenueInfo : EntityBase
    {
        #region Factory

        protected VenueInfo(int entryID, InfoStatus status, int? userId, string hotelVenue,string hotelContract)
        {
            EntryId = entryID;
            UserId = userId;
            Status = status;
            HotelVenue = hotelVenue;
            HotelContract = hotelContract;
            UpdatedTime = DateTime.Now;
        }

        #endregion

        #region Properties

        public string HotelVenue { get; set; }
        public string HotelContract { get; set; }
        public int EntryId { get; set; }
        public int? UserId { get; set; }
        public DateTime UpdatedTime { get; set; }
        public InfoStatus Status { get; set; }

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
}
