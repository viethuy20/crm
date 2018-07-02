using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class EventCompany : Entity
    {
        public EventCompany()
        {
        }
        public EventCompany(int eventId,int companyId)
        {
            EventID = eventId;
            CompanyID = companyId;
        }
        public int? EventID { get; set; }
        [ForeignKey("EventID")]
        public virtual Event Event { get; set; }
        public int EstimatedDelegateNumber { get; set; }
        public string FirstFollowUpStatus { get; set; }
        public string FinalStatus { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateNextFollowUp { get; set; }
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
                return "";
            }
        }
        public string GoodTrainingMonthStr
        {
            get
            {
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
    }
}
