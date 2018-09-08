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

        public VenueInfo()
        {
            UpdatedTime = DateTime.Now;
            Status = InfoStatus.Initial;
        }
        #endregion

        #region Properties

        public string HotelVenue { get; set; }
        public string HotelContract { get; set; }
        public string RejectMessage { get; set; }
        public int EntryId { get; set; }
        public int? UserId { get; set; }
        public DateTime UpdatedTime { get; set; }
        public InfoStatus Status { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        #endregion

        public string StatusDisplay
        {
            get { return Status.DisplayName; }
        }
        public string UserDisplay()
        {
            return User != null ? User.DisplayName : "";
        }
        public override string ToString()
        {
            return Status.DisplayName;
        }


        public string ContractUrl
        {
            get { return "/data/venue/" + HotelContract; }
        }
    }
}
