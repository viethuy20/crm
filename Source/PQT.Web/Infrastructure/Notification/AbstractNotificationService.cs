using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
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
                  .From("info@pri-qua.com")
                  .To(to)
                  .CC(cc)
                  .BCC(bcc)
                  .Subject(subject)
                  .SendAsync();
        }

        public void SendEmail(IEnumerable<string> to, string subject, string typeTemplate, string template, object model)
        {
            var domainRoot = Helpers.UrlHelper.Root;
            new Thread(() =>
            {
                var message = new RazorMailMessage(typeTemplate + "/" + template, model, domainRoot).Render();
                var membershipService = DependencyHelper.GetService<IMembershipService>();
                var emailTemplate =
                    membershipService.GetEmailTemplate(typeTemplate, template);
                if ((emailTemplate != null) || (to != null && to.Any()))
                {
                    var emailTos = new List<string>();
                    var emailCcs = new List<string>();
                    var emailBccs = new List<string>();
                    if (emailTemplate != null)
                    {
                        if (!string.IsNullOrEmpty(emailTemplate.EmailTo))
                            emailTos.AddRange(emailTemplate.EmailTo.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
                        if (!string.IsNullOrEmpty(emailTemplate.EmailCc))
                            emailCcs.AddRange(emailTemplate.EmailCc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
                        if (!string.IsNullOrEmpty(emailTemplate.EmailBcc))
                            emailBccs.AddRange(emailTemplate.EmailBcc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                    if (to != null)
                    {
                        emailTos.AddRange(to);
                    }
                    if (emailTos.Any())
                    {
                        var bccAddition = ConfigurationManager.AppSettings["BccAllEmails"];
                        if (bccAddition != null && !string.IsNullOrEmpty(bccAddition))
                        {
                            var emails = bccAddition.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            emailBccs.AddRange(emails);
                        }
                        SendEmail(emailTos.Distinct().ToArray(),
                            emailCcs.Distinct().ToArray(), emailBccs.Distinct().ToArray(), subject,
                            message);
                    }
                }

            }).Start();
        }
        //public void SendEmail(string subject, string typeTemplate, string template, object model, object viewBag = null)
        //{
        //    var message = new RazorMailMessage(typeTemplate + "/" + template, model, viewBag).Render();
        //    var membershipService = DependencyHelper.GetService<IMembershipService>();
        //    var emailTos =
        //        membershipService.GetAllUserOfEmailTemplate(typeTemplate, template, EmailType.To).ToList();
        //    if (emailTos.Any())
        //    {
        //        var emailCcs =
        //        membershipService.GetAllUserOfEmailTemplate(typeTemplate, template, EmailType.Cc).ToList();
        //        var emailBccs =
        //        membershipService.GetAllUserOfEmailTemplate(typeTemplate, template, EmailType.Bcc).Select(m => m.Email).ToList();

        //        var bccAddition = ConfigurationManager.AppSettings["BccAllEmails"];
        //        if (bccAddition != null && !string.IsNullOrEmpty(bccAddition))
        //        {
        //            var emails = bccAddition.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        //            emailBccs.AddRange(emails);
        //        }


        //        SendEmail(emailTos.Select(m => m.Email).ToArray(),
        //            emailCcs.Select(m => m.Email).ToArray(), emailBccs.ToArray(), subject,
        //            message);
        //    }
        //}
    }

    public abstract class AbstractNotificationService<T> : AbstractNotificationService, INotificationService<T> where T : class
    {
        #region INotificationService<T> Members

        public abstract void NotifyAll(T entity);

        public abstract void NotifyUser(IEnumerable<User> users, T entity);

        public abstract void NotifyRole(IEnumerable<Role> roles, T entity);

        #endregion
    }
}
