using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web.Compilation;
using NS;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;

namespace PQT.Domain.Entities
{
    public class Lead : Entity
    {
        public Lead()
        {
            PhoneCalls = new HashSet<PhoneCall>();
            FirstFollowUpStatus = FollowUpStatus.Neutral;
            FinalStatus = FinalStatus.Neutral;
        }
        public string DirectLine { get; set; } //Direct Line/Mobile number
        public string JobTitle { get; set; } //Client Name/Job Title
        public string LineExtension { get; set; } //Client Name/Job Title
        public string Remark { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobilePhone1 { get; set; }
        public string MobilePhone2 { get; set; }
        public string MobilePhone3 { get; set; }
        public string WorkEmail { get; set; }
        public string WorkEmail1 { get; set; }
        public string PersonalEmail { get; set; }
        public FollowUpStatus FirstFollowUpStatus { get; set; }
        public FinalStatus FinalStatus { get; set; }
        public int? LeadStatusID { get; set; }
        [ForeignKey("LeadStatusID")]
        public virtual LeadStatusRecord LeadStatusRecord { get; set; }
        public int EventID { get; set; }
        [ForeignKey("EventID")]
        public virtual Event Event { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please choose company")]
        public int CompanyID { get; set; }
        [ForeignKey("CompanyID")]
        public virtual Company Company { get; set; }
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        public virtual ICollection<PhoneCall> PhoneCalls { get; set; }

        public bool MarkKPI { get; set; }
        public string FileNameImportKPI { get; set; }
        public string KPIRemarks { get; set; }

        public int? EstimatedDelegateNumber { get; set; }
        public int GoodTrainingMonth { get; set; }

        //public string NewTopics { get; set; }
        //public string NewLocations { get; set; }

        //public DateTime? NewDateFrom { get; set; }
        //public DateTime? NewDateTo { get; set; }
        //public string NewTrainingType { get; set; }
        public decimal? TrainingBudgetPerHead { get; set; }//USD

        public string FullName
        {
            get { return Salutation + " " + FirstName + " " + LastName; }
        }

        public string Name
        {
            get { return FirstName + " " + LastName; }
        }
        public string GoodTrainingMonthStr
        {
            get
            {
                var monthEnum = Enumeration.FromValue<MonthStatus>(GoodTrainingMonth.ToString());
                return monthEnum != null ? monthEnum.ToString() : "";
            }
        }

        public string SalesmanName
        {
            get
            {
                if (User != null)
                {
                    return User.DisplayName;
                }

                return "";
            }
        }
        public string EventName
        {
            get
            {
                if (Event != null)
                {
                    return Event.EventName;
                }

                return "";
            }
        }
        public string CompanyName
        {
            get
            {
                if (Company != null)
                {
                    return Company.CompanyName;
                }

                return "";
            }
        }
        public DateTime CallBackDate
        {
            get
            {
                var call = PhoneCalls.LastOrDefault();
                if (call != null && call.CallBackDate != null)
                {
                    return Convert.ToDateTime(call.CallBackDate);
                }
                return default(DateTime);
            }
        }
        public DateTime CallBackDateTime
        {
            get
            {
                var call = PhoneCalls.LastOrDefault();
                if (call != null && call.CallBackDate != null && call.CallBackTiming != null)
                {
                    return Convert.ToDateTime(call.CallBackDateTimeStr);
                }
                if (call != null && call.CallBackDate != null)
                {
                    return Convert.ToDateTime(call.CallBackDate);
                }
                return default(DateTime);
            }
        }
        public string CallBackDateDisplay
        {
            get
            {
                var call = PhoneCalls.LastOrDefault();
                if (call != null && call.CallBackDate != null)
                {
                    return call.CallBackDateStr;
                }
                return "";
            }
        }
        public string CallBackDateTimeDisplay
        {
            get
            {
                var call = PhoneCalls.LastOrDefault();
                if (call != null)
                {
                    return call.CallBackDateTimeStr;
                }
                return "";
            }
        }

        public string FirstFollowUpStatusDisplay
        {
            get
            {
                if (FirstFollowUpStatus != null)
                {
                    return FirstFollowUpStatus.DisplayName;
                }
                return "";
            }
        }
        public string FirstFollowUpStatusClass
        {
            get
            {
                if (FirstFollowUpStatusDisplay.IndexOf("|", StringComparison.Ordinal) > 0)
                {
                    return StringHelper.RemoveSpecialCharacters(FirstFollowUpStatusDisplay.Substring(0, FirstFollowUpStatusDisplay.IndexOf("|", StringComparison.Ordinal))).ToLower();
                }
                return StringHelper.RemoveSpecialCharacters(FirstFollowUpStatusDisplay).ToLower();
            }
        }
        public string FinalStatusDisplay
        {
            get
            {
                if (FinalStatus != null)
                {
                    return FinalStatus.DisplayName;
                }
                return "";
            }
        }
        public string StatusDisplay
        {
            get
            {
                if (LeadStatusRecord != null)
                {
                    return LeadStatusRecord.Status.DisplayName;
                }
                return "";
            }
        }
        public string StatusUser
        {
            get
            {
                if (LeadStatusRecord != null && LeadStatusRecord.User != null)
                {
                    return LeadStatusRecord.User.DisplayName;
                }
                return "";
            }
        }
        public DateTime StatusUpdateTime
        {
            get
            {
                if (LeadStatusRecord != null)
                {
                    return LeadStatusRecord.UpdatedTime;
                }
                return default(DateTime);
            }
        }
        public string StatusUpdateTimeStr
        {
            get
            {
                if (LeadStatusRecord != null)
                {
                    return LeadStatusRecord.UpdatedTime.ToString("dd-MMM-yyyy HH:mm:ss");
                }
                return "";
            }
        }
        public string CountryCode
        {
            get
            {
                if (Company != null)
                {
                    return Company.CountryCode;
                }
                return "";
            }
        }
        public string DialingCode
        {
            get
            {
                if (Company != null)
                {
                    return Company.DialingCode;
                }
                return "";
            }
        }
        public string Salesman
        {
            get
            {
                if (User != null)
                {
                    return User.DisplayName;
                }
                return "";
            }
        }
        public string DateCreatedDisplay
        {
            get
            {
                return CreatedTime.ToString("dd/MM/yy");
            }
        }

        public int StatusCode
        {
            get
            {
                if (LeadStatusRecord != null)
                {
                    return Convert.ToInt32(LeadStatusRecord.Status.Value);
                }
                return 0;
            }
        }
        public string EventColor
        {
            get
            {
                if (Event != null)
                {
                    return Event.BackgroundColor;
                }

                return "#0aa89e";
            }
        }

        public string ClassStatus
        {
            get
            {
                return StringHelper.RemoveSpecialCharacters(StatusDisplay);
            }
        }
        public KPIStatus KPIStatus
        {
            get
            {
                if (MarkKPI)
                {
                    return KPIStatus.KPI;
                }
                if (!string.IsNullOrEmpty(KPIRemarks))
                {
                    return KPIStatus.NoKPI;
                }
                return KPIStatus.NoCheck;
            }
        }

        public bool CheckNCLExpired(int daysExpired)
        {
            return LeadStatusRecord != null && (LeadStatusRecord == LeadStatus.Live || LeadStatusRecord == LeadStatus.LOI) &&
                    LeadStatusRecord.UpdatedTime.Date <= DateTime.Today.AddDays(-daysExpired);
        }

        public bool CheckInNCL(int daysExpired)
        {
            return (LeadStatusRecord == LeadStatus.Blocked ||
                    LeadStatusRecord == LeadStatus.Live ||
                    LeadStatusRecord == LeadStatus.LOI ||
                    LeadStatusRecord == LeadStatus.Booked) &&
                   !CheckNCLExpired(daysExpired);
        }
        public string ClassKPIStatus
        {
            get
            {
                return StringHelper.RemoveSpecialCharacters(KPIStatus.DisplayName);
            }
        }
        public object Serializing()
        {
            return new
            {
                EventID,
                EventColor,
                StatusCode,
                Salesman,
                DateCreatedDisplay = StatusUpdateTimeStr,
                CountryCode,
                StatusDisplay,
                CompanyName,
                ID
            };
        }
        public object SerializingFull(int daysExpired)
        {
            return new
            {
                ID,
                EventID,
                CreatedTime = StatusUpdateTimeStr,
                Company = Company.CompanyName,
                Country = Company.CountryCodeAndDialing,
                JobTitle,
                DirectLine,
                CallBackDate = CallBackDate == default(DateTime) ? "" : CallBackDate.ToString("dd/MM/yyyy"),
                Event.EventName,
                Event.EventCode,
                Salutation,
                FirstName,
                LastName,
                MobilePhone1,
                MobilePhone2,
                MobilePhone3,
                PersonalEmail,
                WorkEmail,
                EstimatedDelegateNumber,
                TrainingBudgetPerHead = TrainingBudgetPerHead != null ? Convert.ToDecimal(TrainingBudgetPerHead).ToString("N2") : "",
                GoodTrainingMonth,
                StatusCode,
                ClassStatus,
                MarkKPI,
                KPIRemarks,
                FirstFollowUpStatus = FirstFollowUpStatusDisplay,
                FinalStatus = FinalStatusDisplay,
                CallBackDateTime = CallBackDateTimeDisplay,
                NCLExpired = CheckNCLExpired(daysExpired),
                actionBlock = LeadStatusRecord == LeadStatus.Blocked ? "Unblock" : "Block",
                EventColor,
                Salesman,
                DateCreatedDisplay = StatusUpdateTimeStr,
                CountryCode,
                StatusDisplay,
                CompanyName,
            };
        }
        [NotMapped]
        public Booking Booking { get; set; }
    }
}
