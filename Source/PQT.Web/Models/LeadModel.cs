using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PQT.Domain.Entities;

namespace PQT.Web.Models
{
    public class CallSheetModel
    {
        public Lead Lead { get; set; }
        public int EventID { get; set; }
        public IEnumerable<Company> Companies { get; set; }

        public CallSheetModel(int eventId)
        {
            Lead= new Lead();
            EventID = eventId;
            Companies = new List<Company>();
        }
    }
    public class CallingModel
    {
        public Lead Lead { get; set; }
        public int LeadID { get; set; }
        public PhoneCall PhoneCall { get; set; }
        public IEnumerable<Company> Companies { get; set; }

        public CallingModel(int leadId)
        {
            Lead= new Lead();
            LeadID = leadId;
            PhoneCall = new PhoneCall();
            Companies = new List<Company>();
        }
    }
}