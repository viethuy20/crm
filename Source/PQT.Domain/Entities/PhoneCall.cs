using System;
using System.Collections.Generic;
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
    }
}
