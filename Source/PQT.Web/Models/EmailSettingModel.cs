using System.Collections.Generic;
using System.Web.Mvc;
using PQT.Domain.Entities;

namespace PQT.Web.Models
{
    public class EmailSettingModel
    {
        public string Type { get; set; }
        public ICollection<EmailSettingItem> EmailReceiveItems { get; set; }
    }

    public class EmailSettingItem
    {
        public EmailSettingItem()
        {
            EmailSetting = new EmailSetting();
            Emails = new List<SelectListItem>();
            Tos = new List<string>();
            Ccs = new List<string>();
            Bccs = new List<string>();
        }
        // file name template
        public EmailSetting EmailSetting { get; set; }
        public IEnumerable<SelectListItem> Emails { get; set; }
        public IEnumerable<string> Tos { get; set; }
        public IEnumerable<string> Ccs { get; set; }
        public IEnumerable<string> Bccs { get; set; }
        public string EMailBody { get; set; }
    }
}