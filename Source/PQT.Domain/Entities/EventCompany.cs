using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using NS;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class EventCompany : Entity
    {
        public EventCompany()
        {
        }
        public EventCompany(int eventId, int companyId)
        {
            EventID = eventId;
            CompanyID = companyId;
        }
        public int? EventID { get; set; }
        [ForeignKey("EventID")]
        public virtual Event Event { get; set; }
        public int EstimatedDelegateNumber { get; set; }
        //public string FirstFollowUpStatus { get; set; }
        //public string FinalStatus { get; set; }
        //[DataType(DataType.Date)]
        //public DateTime DateNextFollowUp { get; set; }
        public int BudgetMonth { get; set; }
        public int GoodTrainingMonth { get; set; }
        public string TopicsInterested { get; set; }
        public string LocationInterested { get; set; }
        public decimal TrainingBudget { get; set; }//USD
        public string Remarks { get; set; }
        public int CompanyID { get; set; }
        [ForeignKey("CompanyID")]
        public virtual Company Company { get; set; }

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
    }

    public class MonthStatus : Enumeration
    {
        public static readonly MonthStatus NotSet = New<MonthStatus>(0, "Not Set");
        public static readonly MonthStatus January = New<MonthStatus>(1, "January");
        public static readonly MonthStatus February = New<MonthStatus>(2, "February");
        public static readonly MonthStatus March = New<MonthStatus>(3, "March");
        public static readonly MonthStatus April = New<MonthStatus>(4, "April");
        public static readonly MonthStatus May = New<MonthStatus>(5, "May");
        public static readonly MonthStatus June = New<MonthStatus>(6, "June");
        public static readonly MonthStatus July = New<MonthStatus>(7, "July");
        public static readonly MonthStatus August = New<MonthStatus>(8, "August");
        public static readonly MonthStatus September = New<MonthStatus>(9, "September");
        public static readonly MonthStatus October = New<MonthStatus>(10, "October");
        public static readonly MonthStatus November = New<MonthStatus>(11, "November");
        public static readonly MonthStatus December = New<MonthStatus>(12, "December");
    }
}
