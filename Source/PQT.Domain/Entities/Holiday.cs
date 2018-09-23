using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Holiday : Entity
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public int? CountryID { get; set; }
        [ForeignKey("CountryID")]
        public virtual Country Country { get; set; }
        public Holiday()
        {
        }
        public Holiday(Holiday c)
        {
            ID = c.ID;
            StartDate = c.StartDate;
            EndDate = c.EndDate;
            Description = c.Description;
            CountryID = c.CountryID;
            Country = c.Country;
            CreatedTime = c.CreatedTime;
            UpdatedTime = c.UpdatedTime;
            EntityStatus = c.EntityStatus;
        }
        public string HolidayDate()
        {
            if (EndDate != default(DateTime) && EndDate.AddDays(-1).Date > StartDate.Date)
            {
                return StartDate.ToString("ddd, dd MMM yyyy") + " - " + EndDate.AddDays(-1).ToString("ddd, dd MMM yyyy");
            }
            return StartDate.ToString("ddd, dd MMM yyyy");
        }

        public int TotalHolidays(DateTime start, DateTime end)
        {
            var holiday = 0;
            if (start.Date <= StartDate.Date && EndDate.Date <= end.Date)
            {
                holiday = (EndDate - StartDate).Days;
            }
            else if (start.Date <= StartDate.Date && end.Date <= EndDate.Date)
            {
                holiday = (end - StartDate).Days;
            }
            else if (StartDate.Date <= start.Date && EndDate.Date <= end.Date)
            {
                holiday = (EndDate - start.Date).Days;
            }
            return holiday > 0 ? holiday : 0;
        }

        public string Location
        {
            get { return Country != null ? Country.Name : ""; }
        }
    }
}
