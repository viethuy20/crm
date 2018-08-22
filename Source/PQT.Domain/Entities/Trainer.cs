using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Trainer : Entity
    {
        public Trainer()
        {
            TrainerBanks = new HashSet<TrainerBank>();
        }
        public string Name { get; set; }
        public string Passport { get; set; }
        public string Email { get; set; }
        public string BusinessPhone { get; set; }
        public string MobilePhone { get; set; }
        public string Address { get; set; }
        public string NearestInternationalAirport { get; set; }
        public string AirportIATACode { get; set; }
        public string Picture { get; set; }
        public int? TrainerInfoID { get; set; }
        [ForeignKey("TrainerInfoID")]
        public virtual TrainerInfo TrainerInfo { get; set; }
        public virtual ICollection<TrainerBank> TrainerBanks { get; set; }
    }



    public class TrainerInfo : EntityBase
    {
        #region Factory
        protected TrainerInfo(int entryID,int eventId, int? userId, string trainerVisa, string trainerVisaPhoto, 
            string trainerTicket, string trainerTicketPhoto)
        {
            EntryId = entryID;
            EventId = eventId;
            UserId = userId;
            TrainerVisa = trainerVisa;
            TrainerVisaPhoto = trainerVisaPhoto;
            TrainerTicket = trainerTicket;
            TrainerTicketPhoto = trainerTicketPhoto;
            UpdatedTime = DateTime.Now;
        }

        public TrainerInfo(TrainerInfo e)
        {
            ID = e.ID;
            TrainerVisa = e.TrainerVisa;
            TrainerVisaPhoto = e.TrainerVisaPhoto;
            TrainerTicket = e.TrainerTicket;
            TrainerTicketPhoto = e.TrainerTicketPhoto;
            EventId = e.EventId;
            EntryId = e.EntryId;
            UserId = e.UserId;
            UpdatedTime = e.UpdatedTime;
        }
        #endregion

        #region Properties

        public string TrainerVisa { get; set; }
        public string TrainerVisaPhoto { get; set; }
        public string TrainerTicket { get; set; }
        public string TrainerTicketPhoto { get; set; }
        public int EventId { get; set; }
        public int EntryId { get; set; }
        public int? UserId { get; set; }
        public DateTime UpdatedTime { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        #endregion

        public string UserDisplay()
        {
            return User != null ? User.DisplayName : "";
        }
    }
}
