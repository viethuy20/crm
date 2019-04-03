using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using NS.Mail;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Infrastructure.Helpers
{
    public static class LeaveHelper
    {

        public static double GetLeaveDays(this Leave leave, DateTime dateFrom, DateTime dateTo)
        {
            if (dateTo < leave.LeaveDateFrom)
                return 0;
            if (leave.LeaveDateTo < dateFrom)
                return 0;
            if (dateFrom <= leave.LeaveDateFrom && leave.LeaveDateTo <= dateTo)
                return (leave.LeaveDateTo - leave.LeaveDateFrom).TotalDays + 1 -
                       GetWeekendOrHolidays(leave.User.OfficeLocation?.CountryID ?? 0, leave.LeaveDateFrom, leave.LeaveDateTo);
            if (leave.LeaveDateFrom <= dateFrom && leave.LeaveDateTo <= dateTo)
                return (leave.LeaveDateTo - dateFrom).TotalDays + 1 -
                       GetWeekendOrHolidays(leave.User.OfficeLocation?.CountryID ?? 0, dateFrom, leave.LeaveDateTo);
            if (leave.LeaveDateFrom <= dateFrom && dateTo <= leave.LeaveDateTo)
                return (dateTo - dateFrom).TotalDays + 1 -
                       GetWeekendOrHolidays(leave.User.OfficeLocation?.CountryID ?? 0, dateFrom, dateTo);
            if (dateFrom <= leave.LeaveDateFrom && dateTo <= leave.LeaveDateTo)
                return (dateTo - leave.LeaveDateFrom).TotalDays + 1 -
                       GetWeekendOrHolidays(leave.User.OfficeLocation?.CountryID ?? 0, leave.LeaveDateFrom, dateTo);
            return 0;
        }
        public static double GetLeaveDaysByMonth(this Leave leave, DateTime reportMonth)
        {
            if (reportMonth.Month == leave.LeaveDateFrom.Month &&
                reportMonth.Month == leave.LeaveDateTo.Month)
                return (leave.LeaveDateTo - leave.LeaveDateFrom).TotalDays + 1 -
                       GetWeekendOrHolidays(leave.User.OfficeLocation?.CountryID ?? 0, leave.LeaveDateFrom, leave.LeaveDateTo);
            if (reportMonth.Month == leave.LeaveDateFrom.Month &&
                reportMonth.Month < leave.LeaveDateTo.Month)
                return (reportMonth.AddMonths(1) - leave.LeaveDateFrom).TotalDays -
                       GetWeekendOrHolidays(leave.User.OfficeLocation?.CountryID ?? 0, leave.LeaveDateFrom, reportMonth.AddMonths(1).AddDays(-1));
            if (leave.LeaveDateFrom.Month < reportMonth.Month &&
                reportMonth.Month == leave.LeaveDateTo.Month)
                return (leave.LeaveDateTo - reportMonth).TotalDays + 1 -
                       GetWeekendOrHolidays(leave.User.OfficeLocation?.CountryID ?? 0, reportMonth, leave.LeaveDateTo);
            return 0;
        }

        private static int GetWeekendOrHolidays(int countryId, DateTime dateFrom, DateTime dateTo)
        {
            var totalSundayDays = DateTimeHelper.CountDays(DayOfWeek.Sunday, dateFrom, dateTo);
            var totalSaturdayDays = DateTimeHelper.CountDays(DayOfWeek.Saturday, dateFrom, dateTo);
            var settingService = NS.Mvc.DependencyHelper.GetService<ISettingRepository>();
            return settingService.TotalHolidays(dateFrom, dateTo, countryId) + totalSaturdayDays + totalSundayDays;
        }
    }
}