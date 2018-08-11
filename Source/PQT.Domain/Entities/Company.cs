using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Forms.VisualStyles;
using NS;

namespace PQT.Domain.Entities
{
    public class Company : Entity
    {
        public Company()
        {
            ManagerUsers = new HashSet<User>();
            Tier = 3;
        }
        public Company(Company c)
        {
            ID = c.ID;
            CountryID = c.CountryID;
            Country = c.Country;
            CompanyName = c.CompanyName;
            ProductOrService = c.ProductOrService;
            Sector = c.Sector;
            Industry = c.Industry;
            Ownership = c.Ownership;
            BusinessUnit = c.BusinessUnit;
            BudgetMonth = c.BudgetMonth;
            BudgetPerHead = c.BudgetPerHead;
            FinancialYear = c.FinancialYear;
            Tier = c.Tier;
            Address = c.Address;
            Tel = c.Tel;
            Fax = c.Fax;
            Remarks = c.Remarks;
            ManagerUsers = c.ManagerUsers;
            CreatedTime = c.CreatedTime;
            UpdatedTime = c.UpdatedTime;
        }
        public int? CountryID { get; set; }
        [ForeignKey("CountryID")]
        public virtual Country Country { get; set; }
        public string CompanyName { get; set; }
        public string ProductOrService { get; set; }
        public string Sector { get; set; }
        public string Industry { get; set; }
        public string Ownership { get; set; }
        public string BusinessUnit { get; set; }
        public int BudgetMonth { get; set; }
        public decimal BudgetPerHead { get; set; }
        public int FinancialYear { get; set; }
        public int Tier { get; set; }//tier: 1-red 2-blue -0-black
        public string CountryName
        {
            get
            {
                if (Country != null)
                {
                    return Country.Name;
                }

                return "";
            }
        }
        public string CountryCode
        {
            get
            {
                if (Country != null)
                {
                    return Country.Code + " (" + Country.DialingCode + ")";
                }

                return "";
            }
        }
        public string DialingCode
        {
            get
            {
                if (Country != null)
                {
                    return Country.DialingCode;
                }

                return "";
            }
        }

        public string Address { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string Remarks { get; set; }
        public virtual ICollection<User> ManagerUsers { get; set; }

        public string BudgetMonthStr
        {
            get
            {
                var monthEnum = Enumeration.FromValue<MonthStatus>(BudgetMonth.ToString());
                return monthEnum != null ? monthEnum.DisplayName : "";
            }
        }
    }


    public class TierType : Enumeration
    {
        public static readonly TierType Tier3 = New<TierType>(3, "Tier 3");
        public static readonly TierType Tier1 = New<TierType>(1, "Tier 1");
        public static readonly TierType Tier2 = New<TierType>(2, "Tier 2");
    }
}
