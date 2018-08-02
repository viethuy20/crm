using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class PhoneCall : EntityBase
    {
        public PhoneCall()
        {
            StartTime = DateTime.Now;
        }
        public string Remark { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? CallBackDate { get; set; }
        public TimeSpan? CallBackTiming { get; set; }
        public int LeadID { get; set; }
        [ForeignKey("LeadID")]
        public Lead Lead { get; set; }

        public string RemarkHtml
        {
            get { return Remark != null ? Remark.Replace("\n", "</br>") : ""; }
        }
        public string CallBackDateTimeStr
        {
            get
            {
                if (CallBackDate != null && CallBackTiming != null)
                {
                    return CallBackDateStr + " " + CallBackTimingStr;
                }
                if (CallBackDate !=null)
                {
                    return CallBackDateStr;
                }
                return "";
            }
        }
        public string CallBackDateStr
        {
            get { return CallBackDate != null ? Convert.ToDateTime(CallBackDate).ToString("dd/MM/yyyy") : ""; }
        }
        public string CallBackTimingStr
        {
            get { return CallBackTiming != null ? ((TimeSpan)CallBackTiming).Hours.ToString("00") + ":" + ((TimeSpan)CallBackTiming).Minutes.ToString("00") : ""; }
        }
    }
}
