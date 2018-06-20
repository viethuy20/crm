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
        public static void SendEmail(string[] to, string subject, RazorMailMessage message)
        {
            Mailer.UseMessage(message)
                  .From("info@pri-qua.com")
                  .To(to)
                  .Subject(subject)
                  .SendAsync();
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



        #endregion Config

        public static void LogsSendEmail(Audit audit)
        {
            var domainRoot = Helpers.UrlHelper.Root;
            new Thread(() =>
            {
                string subject = "PQT Logs Exception " + domainRoot;
                var message = new RazorMailMessage("Logs/Exception", audit, domainRoot).Render();
                var receiveEmail = ConfigurationManager.AppSettings["LogsEmail"];
                if (receiveEmail != null && !string.IsNullOrEmpty(receiveEmail))
                {
                    var emails = receiveEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    EmailHelper.SendEmail(emails, subject, message);
                }
            }).Start();
        }
    }
}