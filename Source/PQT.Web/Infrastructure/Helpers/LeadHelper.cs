using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Infrastructure.Helpers
{
    public static class LeadHelper
    {
        public static bool CheckPossibleBlock(this Lead lead)
        {
            if (lead.LeadStatusRecord == LeadStatus.Booked)
                return false;
            return true;
        }
    }
}