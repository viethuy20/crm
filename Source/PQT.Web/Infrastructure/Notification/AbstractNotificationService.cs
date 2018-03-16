using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using NS.Mail;
using PQT.Web.Hubs;

namespace PQT.Web.Infrastructure.Notification
{
    public abstract class AbstractNotificationService
    {
        protected IMembershipService MembershipService
        {
            get { return DependencyResolver.Current.GetService<IMembershipService>(); }
        }

        public static void SendEmail(string[] to, string[] cc, string[] bcc, string subject, RazorMailMessage message)
        {
            Mailer.UseMessage(message)
                  .From("info@t8gallery.com.sg")
                  .To(to)
                  .CC(cc)
                  .BCC(bcc)
                  .Subject(subject)
                  .SendAsync();
        }

        public void SendEmail(string subject, string typeTemplate, string template, object model, object viewBag = null)
        {
            var message = new RazorMailMessage(typeTemplate + "/" + template, model, viewBag).Render();
            var membershipService = DependencyHelper.GetService<IMembershipService>();
            var emailTos =
                membershipService.GetAllUserOfEmailTemplate(typeTemplate, template, EmailType.To).ToList();
            if (emailTos.Any())
            {
                var emailCcs =
                membershipService.GetAllUserOfEmailTemplate(typeTemplate, template, EmailType.Cc).ToList();
                var emailBccs =
                membershipService.GetAllUserOfEmailTemplate(typeTemplate, template, EmailType.Bcc).Select(m => m.Email).ToList();

                var bccAddition = ConfigurationManager.AppSettings["BccAllEmails"];
                if (bccAddition != null && !string.IsNullOrEmpty(bccAddition))
                {
                    var emails = bccAddition.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    emailBccs.AddRange(emails);
                }


                SendEmail(emailTos.Select(m => m.Email).ToArray(),
                    emailCcs.Select(m => m.Email).ToArray(), emailBccs.ToArray(), subject,
                    message);
            }
        }
    }

    public abstract class AbstractNotificationService<T> : AbstractNotificationService, INotificationService<T> where T : class
    {
        #region INotificationService<T> Members

        public abstract void NotifyAll(T entity);

        public abstract void NotifyUser(User user, T entity);

        public abstract void NotifyRole(Role role, T entity);

        #endregion
    }
}
