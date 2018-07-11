﻿using System;
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
        }
        public string DirectLine { get; set; } //Direct Line/Mobile number
        public string ClientName { get; set; } //Client Name/Job Title
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
        public int BudgetMonth { get; set; }
        public int GoodTrainingMonth { get; set; }
        public string TopicsInterested { get; set; }
        public string LocationInterested { get; set; }

        public string BudgetMonthStr
        {
            get
            {
                var monthEnum = Enumeration.FromValue<MonthStatus>(BudgetMonth.ToString());
                return monthEnum != null ? monthEnum.ToString() : "";
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
        public string CallBackDateDisplay
        {
            get
            {
                var call = PhoneCalls.LastOrDefault();
                if (call != null && call.CallBackDate != null)
                {
                    return Convert.ToDateTime(call.CallBackDate).ToString("dd/MM/yyyy");
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
        [NotMapped]
        public Booking Booking { get; set; }
    }
}
