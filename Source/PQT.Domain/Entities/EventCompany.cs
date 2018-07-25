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
        public int BudgetMonth { get; set; }
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
