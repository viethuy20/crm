using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using NS;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;

namespace PQT.Domain.Entities
{
    public class LeadNew : Entity
    {
        public LeadNew()
        {
            FirstFollowUpStatus = FollowUpStatus.Pending;
            FinalStatus = FinalStatus.Pending;
        }
        public string DirectLine { get; set; } //Direct Line/Mobile number
        public string JobTitle { get; set; } //Client Name/Job Title
        public string LineExtension { get; set; }
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
        public int? AssignUserID { get; set; }
        [ForeignKey("AssignUserID")]
        public virtual User AssignUser { get; set; }
        public DateTime AssignDate { get; set; }
        public DateTime FirstAssignDate { get; set; }
        public int? EstimatedDelegateNumber { get; set; }
        public int GoodTrainingMonth { get; set; }
        public string NewTopics { get; set; }
        public string NewLocations { get; set; }
        public DateTime? NewDateFrom { get; set; }

        public DateTime? NewDateTo { get; set; }
        public string NewTrainingType { get; set; }
        public string RequestBrochure { get; set; }
        public decimal? TrainingBudgetPerHead { get; set; }//USD

        public string AssignUserDisplay
        {
            get
            {
                if (AssignUser!=null)
                {
                    return AssignUser.DisplayName;
                }
                return "";
            }
        }
        public string Sales
        {
            get
            {
                if (AssignUser!=null)
                {
                    return AssignUser.DisplayName;
                }
                return User.DisplayName;
            }
        }
        public string FullName
        {
            get { return Salutation + " " + FirstName + " " + LastName; }
        }
        public string Name
        {
            get { return FirstName + " " + LastName; }
        }
        public string NewDatesDisplay
        {
            get { return NewDateFromDisplay + " - " + NewDateToDisplay; }
        }
        public string NewTrainingTypeDisplay
        {
            get
            {
                var monthEnum = Enumeration.FromValue<NewTrainingType>(NewTrainingType);
                return monthEnum != null ? monthEnum.DisplayName : "";
            }
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
                return CreatedTime.ToString("dd/MM/yyyy HH:mm");
            }
        }

        public string AssignDateDisplay
        {
            get
            {
                if (AssignDate != default(DateTime))
                    return Convert.ToDateTime(AssignDate).ToString("dd/MM/yyyy");
                return "";
            }
        }
        public string NewDateFromDisplay
        {
            get
            {
                if (NewDateFrom != null)
                    return Convert.ToDateTime(NewDateFrom).ToString("dd/MM/yyyy");
                return "";
            }
        }

        public string NewDateToDisplay
        {
            get
            {
                if (NewDateTo != null)
                    return Convert.ToDateTime(NewDateTo).ToString("dd/MM/yyyy");
                return "";
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

        public string StatusCreatedTimeStr
        {
            get
            {
                return CreatedTime.ToString("dd-MMM-yyyy HH:mm:ss");
            }
        }
        public object Serializing()
        {
            return new
            {
                EventID,
                EventColor,
                Salesman,
                NewDateFrom,
                NewDateTo,
                CountryCode,
                NewTrainingTypeDisplay,
                CompanyName,
                ID
            };
        }
        public object SerializingFull()
        {
            return new
            {
                ID,
                EventID,
                CreatedTime = StatusCreatedTimeStr,
                Company = Company.CompanyName,
                Country = Company.CountryCodeAndDialing,
                JobTitle,
                DirectLine,
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
                NewTopics,
                NewLocations,
                NewDateFrom,
                NewDateTo,
                NewTrainingTypeDisplay,
                FirstFollowUpStatus = FirstFollowUpStatusDisplay,
                FinalStatus = FinalStatusDisplay,
                EventColor,
                Salesman,
                DateCreatedDisplay = StatusCreatedTimeStr,
                CountryCode,
                CompanyName,
            };
        }
        [NotMapped]
        public Booking Booking { get; set; }
    }
}
