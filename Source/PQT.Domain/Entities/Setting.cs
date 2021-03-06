using System;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace PQT.Domain.Entities
{
    public class Setting : EntityBase
    {
        #region Primitive

        public string Module { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
        public string Summary { get; set; }
        public string TableToGetField { get; set; }
        public int MaxLenght { get; set; }
        public string Description { get; set; }


        #endregion

        #region Navigation


        public List<string> ListOfSettingType()
        {
            return null;
        }

        #endregion

        #region function
      
        #endregion

        #region define class and structure

        public enum ModuleType
        {
            System,
            Lead,
            KPI
        }

        public enum TypeOfValue
        {
            Textbox,
            InputTags,
            Checkbox
        }

        public static Func<Setting, bool> IsActive()
        {
            return setting => true;
        }

        public class ModuleKey
        {
            public enum Lead
            {
                MaxBlockeds,
                MaxLOIs,
                NumberDaysExpired,
                DiscountPercent
            }

            public enum System
            {
                NotificationNumber,
                AccessIPs,
            }
            public enum KPI
            {
                VoIpBuffer,
                ExceptCode,
                BufferForNewUser,
                DailyRequiredCallKpiForIntern,
                DailyRequiredCallKpiForFull
            }
        }

        #endregion

    }
}
