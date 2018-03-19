using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NS.Mail;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Hubs;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Infrastructure.Notification
{
    public class LeadNotificationService : AbstractNotificationService<Lead>
    {
        #region Override

        public override void NotifyAll(Lead lead)
        {
            SendEmailNotify(lead);
        }

        public override void NotifyUser(IEnumerable<User> users, Lead lead)
        {
            if (lead == null)
                return;
        }

        public override void NotifyRole(IEnumerable<Role> roles, Lead lead)
        {
            if (lead == null)
                return;
        }

        #endregion

        private void SendEmailNotify(Lead lead)
        {
            if (lead == null) return;
            if (lead.LeadStatusRecord == LeadStatus.RequestNCL)
                SendEmail_RequestNCL(lead);
            else if (lead.LeadStatusRecord == LeadStatus.RequestLOI)
                SendEmail_RequestLOI(lead);
            else if (lead.LeadStatusRecord == LeadStatus.RequestNCL)
                SendEmail_RequestNCL(lead);

        }

        private void SendEmail_RequestNCL(Lead lead)
        {
            string subject = "Request NCL " + lead.ID;
            SendEmail(subject, "Lead", "RequestNoCallList", lead);
        }
        private void SendEmail_RequestLOI(Lead lead)
        {
            string subject = "Request LOI " + lead.ID;
            SendEmail(subject, "Lead", "RequestLOI", lead);
        }

    }
}