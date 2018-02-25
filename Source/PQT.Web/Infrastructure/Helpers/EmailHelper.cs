using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Utility;
using PQT.Web.Models;
using NS.Helpers;
using NS.Mail;

namespace PQT.Web.Infrastructure.Helpers
{
    public class EmailHelper
    {
        #region Config
        public static string RenderMessage(string path, object model = null)
        {
            try
            {
                var mail = new RazorMailMessage(path, model).Render();
                var stream = mail.AlternateViews[0].ContentStream;
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                //var a = new ExternalException("template file:" + path + "\n\n" + ex.Message);
                //throw a;
                return "";
            }
        }
        public static void SendSMS(string phone, string message)
        {
            var host = HttpContext.Current.Request.Url.Host;
            new Thread(() =>
            {
                try
                {
                    MailMessage mail =
                        new MailMessage("info@t8gallery.com.sg", string.Format("sms{0}@t8gallery.com.sg", phone.Replace("+", "")));
                    var client = new SmtpClient("paging.tcil.com.sg", 25);
                    mail.Subject = "T8 Message Center";
                    mail.Body = message;
                    client.Send(mail);
                }
                catch (Exception ex)
                {
                    var error = ex.Message;
                    if (ex.InnerException != null)
                    {
                        error += "\n InnerException: " + ex.InnerException.Message;
                    }
                    var audit = new Audit
                    {
                        Controller = "SendSMS",
                        Message = "SendSMS Exception phone number = " + phone,
                        Data = "'" + message + "' ==> SendSMS ex:" + error,
                        UrlAccessed = "SendSMS",
                        TimeAccessed = DateTime.Now
                    };
                    LogsSendEmail(audit, host);
                }
            }).Start();
        }
        public static void SendEmail(string[] to, string subject, RazorMailMessage message)
        {
            Mailer.UseMessage(message)
                  .From("info@t8gallery.com.sg")
                  .To(to)
                  .Subject(subject)
                  .SendAsync();
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

        private static void SendEmail(string subject, string type, string template, object model, object viewBag = null)
        {
            var message = new RazorMailMessage(type + "/" + template, model, viewBag).Render();
            var membershipService = DependencyHelper.GetService<IMembershipService>();
            var emailTos =
                membershipService.GetAllUserOfEmailTemplate(type, template, EmailType.To).ToList();
            if (emailTos.Any())
            {
                var emailCcs =
                membershipService.GetAllUserOfEmailTemplate(type, template, EmailType.Cc).ToList();
                var emailBccs =
                membershipService.GetAllUserOfEmailTemplate(type, template, EmailType.Bcc).Select(m => m.Email).ToList();

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


        #endregion Config

        public static void LogsSendEmail(Audit audit, string host)
        {
            string subject = "T8 Gallery Logs Exception " + host;
            var message = new RazorMailMessage("Logs/Exception", audit).Render();
            var receiveEmail = ConfigurationManager.AppSettings["LogsEmail"];
            if (receiveEmail != null && !string.IsNullOrEmpty(receiveEmail))
            {
                var emails = receiveEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                EmailHelper.SendEmail(emails, subject, message);
            }
        }
    }
}