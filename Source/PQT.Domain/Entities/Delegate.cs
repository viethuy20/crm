using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class Delegate : Entity
    {
        public Delegate()
        {
            AttendanceStatus = DelegateAttendanceStatus.Initial;
            Salutation = NameTitle.None;
        }
        public int Order { get; set; }
        public bool ToBeAdvisedLater { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string JobTitle { get; set; }
        public string DirectLine { get; set; }
        public string MobilePhone1 { get; set; }
        public string MobilePhone2 { get; set; }
        public string MobilePhone3 { get; set; }
        public string WorkEmail { get; set; }
        public string PersonalEmail { get; set; }
        public int? LeadID { get; set; }
        public int? BookingID { get; set; }
        public string OverallFeedbacks { get; set; }
        public string OpTopicsInterested { get; set; }
        public string OpLocationsInterested { get; set; }
        public string OpGoodTrainingMonth { get; set; }

        public DelegateAttendanceStatus AttendanceStatus { get; set; }

        public string AttendanceStatusDisplay
        {
            get
            {
                if (AttendanceStatus != null)
                {
                    return AttendanceStatus.DisplayName;
                }
                return "";
            }
        }
        public string CreatedTimeStr
        {
            get
            {
                return CreatedTime.ToString("dd/MM/yyyy HH:mm");
            }
        }
        public string Session
        {
            get
            {
                return string.Join(", ", EventSessions.Select(m => m.SessionTitle));
            }
        }
        [NotMapped]
        public string EventName { get; set; }
        [NotMapped]
        public string EventCode { get; set; }
        [NotMapped]
        public string EventDate { get; set; }
        [NotMapped]
        public string EventID { get; set; }
        [NotMapped]
        public string Country { get; set; }
        [NotMapped]
        public string Company { get; set; }
        [NotMapped]
        public string Salesman { get; set; }
        [NotMapped]
        public IEnumerable<EventSession> EventSessions { get; set; }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName)) return "To Be Advised Later";
                return Salutation + " " + FirstName + " " + LastName;
            }
        }
        public string DelegateEmail
        {
            get
            {
                var contact = "";
                if (!string.IsNullOrEmpty(WorkEmail))
                {
                    contact += WorkEmail;
                }
                if (!string.IsNullOrEmpty(PersonalEmail))
                {
                    if (!string.IsNullOrEmpty(contact))
                        contact += " / " + PersonalEmail;
                    else
                        contact += PersonalEmail;
                }
                return contact;
            }
        }
        public string DelegateContact
        {
            get
            {
                var contact = "";
                if (!string.IsNullOrEmpty(MobilePhone1))
                {
                    contact += MobilePhone1;
                }
                if (!string.IsNullOrEmpty(MobilePhone2))
                {
                    if (!string.IsNullOrEmpty(contact))
                        contact += " / " + MobilePhone2;
                    else
                        contact += MobilePhone2;
                }
                if (!string.IsNullOrEmpty(MobilePhone3))
                {
                    if (!string.IsNullOrEmpty(contact))
                        contact += " / " + MobilePhone3;
                    else
                        contact += MobilePhone3;
                }
                return contact;
            }
        }

        public Delegate PassInfo(string country, string company, string salesman, IEnumerable<EventSession> eventSessions)
        {
            Country = country;
            Company = company;
            Salesman = salesman;
            EventSessions = eventSessions;
            return this;
        }
        public Delegate PassInfoForTierCompany(string eventId, string eventName, string eventCode, string eventDate)
        {
            EventID = eventId;
            EventName = eventName;
            EventCode = eventCode;
            EventDate = eventDate;
            return this;
        }

    }
}
