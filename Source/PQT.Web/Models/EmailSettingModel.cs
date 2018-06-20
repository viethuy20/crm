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
        }
        // file name template
        public EmailSetting EmailSetting { get; set; }
        public string EMailBody { get; set; }
    }
}