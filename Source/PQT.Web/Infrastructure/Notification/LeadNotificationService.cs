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
        }

        public override void NotifyUser(IEnumerable<User> users, Lead lead)
        {
            if (lead == null)
                return;
            SendEmailNotify(lead, users.Select(m => m.Email).ToArray());
        }

        public override void NotifyRole(IEnumerable<Role> roles, Lead lead)
        {
            if (lead == null)
                return;
        }

        #endregion

        private void SendEmailNotify(Lead lead, string[] users)
        {
            if (lead == null) return;
            SendEmail_RequestNCL(lead, users);

        }

        private void SendEmail_RequestNCL(Lead lead, string[] users)
        {
            string subject = lead.StatusDisplay + " #" + lead.ID;
            SendEmail(users, subject, "Lead", "RequestNoCallList", lead);
        }

    }
}