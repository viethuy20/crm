using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class NotifySetting : EntityBase
    {
        public NotifyType NotifyType { get; set; }
        public NotifyAction NotifyAction { get; set; }
        public string Roles { get; set; }

        public string[] AllRoles
        {
            get
            {
                if (!string.IsNullOrEmpty(Roles))
                {
                    return Roles.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries);
                }
                return new List<string>().ToArray();
            }
        }

        public string NotifyTypeDisplay
        {
            get { return NotifyType.DisplayName; }
        }
        public string NotifyActionDisplay
        {
            get { return NotifyAction.DisplayName; }
        }
    }
}
