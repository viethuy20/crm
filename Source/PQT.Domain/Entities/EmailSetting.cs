using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class EmailSetting : EntityBase
    {
        public EmailSetting()
        {
        }
        //root folder contains template
        public string Type { get; set; }
        // file name template
        public string TemplateName { get; set; }

        public string EmailTo { get; set; }
        public string EmailCc { get; set; }
        public string EmailBcc { get; set; }
    }
}
