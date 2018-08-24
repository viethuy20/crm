using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public abstract class LocalInfo : EntityBase
    {
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string PricingUnit { get; set; }
        public string Remarks { get; set; }
    }
    public class DriverInfo : LocalInfo
    {
    }
    public class PhotographerInfo : LocalInfo
    {
    }
    public class LocalVisaAgentInfo : LocalInfo
    {
    }

    public class PostEventInfo : Entity
    {
        public string DelegateAttendance { get; set; }
        public string AttendanceUpload { get; set; }
        public string DelegateFeedback { get; set; }
        public string FeedbackUpload { get; set; }


        public string AttendanceUrl
        {
            get { return "/data/postevent/" + AttendanceUpload; }
        }
        public string FeedbackUrl
        {
            get { return "/data/postevent/" + FeedbackUpload; }
        }
    }

}
