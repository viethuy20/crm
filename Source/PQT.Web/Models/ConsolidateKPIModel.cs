using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NS.Helpers;
using NS.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Models
{
    public class ConsolidateKPIModel
    {
        public List<ConsolidateKPI> ConsolidateKpis { get; set; }
        public string EventName { get; set; }
        public string Date { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public ConsolidateKPIModel()
        {
            EventName = "All";
            Date = "All";
        }
        public void Prepare(IEnumerable<Lead> leads, IEnumerable<LeadNew> leadNews, IEnumerable<Booking> bookings)
        {
            var settingService = DependencyHelper.GetService<ISettingRepository>();
            var leaveService = DependencyHelper.GetService<ILeaveService>();
            var leaves = leaveService.GetAllLeavesNotInclude(m => m.LeaveDate >= DateFrom &&
                                                     m.LeaveDate <= DateTo &&
                                                     m.LeaveType.Value == LeaveType.Leave.Value &&
                                                     m.LeaveStatus.Value == LeaveStatus.Approved.Value
            );
            var nonSalesDays = leaveService.GetAllNonSalesDaysNotInclude(m => m.IssueMonth.Month >= DateFrom.Month &&
                                                                    m.IssueMonth.Month <= DateTo.Month).Sum(m => m.NonSalesDays);
            var technicialIssueDays = leaveService.GetAllTechnicalIssueDaysNotInclude(m => m.IssueMonth.Month >= DateFrom.Month &&
                                                                                 m.IssueMonth.Month <= DateTo.Month).Sum(m => m.TechnicalIssueDays);
            var totalSundayDays = DateTimeHelper.CountDays(DayOfWeek.Sunday, DateFrom, DateTo);
            var totalSaturdayDays = DateTimeHelper.CountDays(DayOfWeek.Saturday, DateFrom, DateTo);
            var totalWorkingDays = (DateTo - DateFrom).TotalDays + 1 - totalSaturdayDays - totalSundayDays;
            var defaultBufferForNew = Settings.KPI.BufferForNewUser();
            ConsolidateKpis = new List<ConsolidateKPI>();
            var users = leads.DistinctBy(m => m.UserID).Select(m => m.User).ToList();
            users.AddRange(leadNews.DistinctBy(m => m.UserID).Select(m => m.User));
            foreach (var user in users.Where(m => m.UserStatus == UserStatus.Live).DistinctBy(m => m.ID))
            {
                int? country = null;
                if (user.OfficeLocation != null)
                    country = user.OfficeLocation.CountryID;
                var totalHolidays = settingService.TotalHolidays(DateFrom, DateTo, country);
                var totalLeave = leaves.Count(m => m.UserID == user.ID && m.TypeOfLeave != TypeOfLeave.HalfDayUnpaid) +
                    ((double)leaves.Count(m => m.UserID == user.ID && m.TypeOfLeave == TypeOfLeave.HalfDayUnpaid) / 2);
                var actualWorkingDays = totalWorkingDays - totalHolidays - totalLeave;

                //var totalNonSalesDays = nonSalesDays.Where(m => m.UserID == user.ID).Sum(m => m.NonSalesDays);

                var bufferForNewSales = GetBufferForNewSales(user, country, defaultBufferForNew, settingService);

                var actualRequiredCallKpis =
                    (actualWorkingDays - bufferForNewSales - nonSalesDays - technicialIssueDays) * 3;
                var item = new ConsolidateKPI
                {
                    User = user
                };
                item.Prepare(leads.Where(m => m.UserID == user.ID),
                    leadNews.Where(m => m.UserID == user.ID),
                    bookings.Where(m => m.SalesmanID == user.ID),
                    actualRequiredCallKpis);
                ConsolidateKpis.Add(item);
            }
        }

        public double GetBufferForNewSales(User user, int? country, int defaultBufferForNewUser, ISettingRepository settingService)
        {
            if (user.EmploymentDate == null ||
                user.UserContracts.Count > 1 ||
                user.EmploymentDate > DateTo ||
                user.EmploymentDate < DateTime.Today.AddDays(-31))
                return 0;
            var employmentDate = Convert.ToDateTime(user.EmploymentDate);
            var employmentDateEndBuffer = employmentDate.AddDays(defaultBufferForNewUser);
            var totalSundayDays = DateTimeHelper.CountDays(DayOfWeek.Sunday, employmentDate, employmentDateEndBuffer);
            var totalSaturdayDays = DateTimeHelper.CountDays(DayOfWeek.Saturday, employmentDate, employmentDateEndBuffer);
            employmentDateEndBuffer = employmentDateEndBuffer.AddDays(totalSundayDays + totalSaturdayDays);//add weekend days
            var totalHolidays = settingService.TotalHolidays(employmentDate, employmentDateEndBuffer, country);
            employmentDateEndBuffer = employmentDateEndBuffer.AddDays(totalHolidays);//add hoiday days

            if (employmentDateEndBuffer < DateFrom)
            {
                return 0;
            }
            if (DateFrom <= employmentDate && employmentDateEndBuffer <= DateTo)
            {
                return defaultBufferForNewUser;
            }

            if (employmentDate <= DateFrom && employmentDateEndBuffer <= DateTo)
            {
                var totalDays = (employmentDateEndBuffer - DateFrom).TotalDays + 1;

                var totalSundayDays1 = DateTimeHelper.CountDays(DayOfWeek.Sunday, DateFrom, employmentDateEndBuffer);
                var totalSaturdayDays1 = DateTimeHelper.CountDays(DayOfWeek.Saturday, DateFrom, employmentDateEndBuffer);
                var totalHolidays1 = settingService.TotalHolidays(DateFrom, employmentDateEndBuffer, country);
                return totalDays - totalSundayDays1 - totalSaturdayDays1 - totalHolidays1;
            }

            if (employmentDate <= DateFrom && DateTo <= employmentDateEndBuffer)
            {
                var totalDays = (DateTo - DateFrom).TotalDays + 1;
                var totalSundayDays1 = DateTimeHelper.CountDays(DayOfWeek.Sunday, DateFrom, DateTo);
                var totalSaturdayDays1 = DateTimeHelper.CountDays(DayOfWeek.Saturday, DateFrom, DateTo);
                var totalHolidays1 = settingService.TotalHolidays(DateFrom, DateTo, country);
                return totalDays - totalSundayDays1 - totalSaturdayDays1 - totalHolidays1;
            }

            if (DateFrom <= employmentDate && DateTo <= employmentDateEndBuffer)
            {
                var totalDays = (DateTo - employmentDate).TotalDays + 1;
                var totalSundayDays1 = DateTimeHelper.CountDays(DayOfWeek.Sunday, employmentDate, DateTo);
                var totalSaturdayDays1 = DateTimeHelper.CountDays(DayOfWeek.Saturday, employmentDate, DateTo);
                var totalHolidays1 = settingService.TotalHolidays(employmentDate, DateTo, country);
                return totalDays - totalSundayDays1 - totalSaturdayDays1 - totalHolidays1;
            }
            return 0;
        }
        public void Prepare(IEnumerable<Booking> bookings)
        {
            ConsolidateKpis = new List<ConsolidateKPI>();
            var users = bookings.DistinctBy(m => m.SalesmanID).Select(m => m.Salesman).ToList();
            foreach (var user in users.DistinctBy(m => m.ID))
            {
                var item = new ConsolidateKPI
                {
                    User = user
                };
                item.Prepare(bookings.Where(m => m.SalesmanID == user.ID));
                ConsolidateKpis.Add(item);
            }
        }

    }
    public class HRConsolidateKPIModel
    {
        public List<HRConsolidateKPI> HrConsolidateKpis { get; set; }
        public string Date { get; set; }

        public HRConsolidateKPIModel()
        {
            Date = "All";
        }

        public void Prepare(IEnumerable<Candidate> leads)
        {
            HrConsolidateKpis = new List<HRConsolidateKPI>();
            var users = leads.DistinctBy(m => m.UserID).Select(m => m.User);
            foreach (var user in users)
            {
                var item = new HRConsolidateKPI
                {
                    User = user
                };
                item.Prepare(leads.Where(m => m.UserID == user.ID));
                HrConsolidateKpis.Add(item);
            }
        }
    }
    public class ConsolidateKPI
    {
        public User User { get; set; }
        public int NewEventRequest { get; set; }
        public decimal WrittenRevenue { get; set; }
        public decimal WrittenRevenue1 { get; set; }
        public decimal WrittenRevenue2 { get; set; }
        public decimal WrittenRevenue3 { get; set; }
        public decimal TotalWrittenRevenue { get; set; }
        public int KPI { get; set; }
        public int NoKPI { get; set; }
        public int NoCheck { get; set; }
        public double ActualCallKpis { get; set; }
        public double ActualRequiredCallKpis { get; set; }
        public void Prepare(IEnumerable<Lead> leads, IEnumerable<LeadNew> leadNews, IEnumerable<Booking> bookings, double actualRequiredCallKpis)
        {
            NewEventRequest = leadNews.Count();
            WrittenRevenue = bookings.Sum(m => m.TotalWrittenRevenue);
            KPI = leads.Count(m => m.MarkKPI);
            NoKPI = leads.Count(m => !m.MarkKPI && !string.IsNullOrEmpty(m.FileNameImportKPI));
            NoCheck = leads.Count(m => string.IsNullOrEmpty(m.FileNameImportKPI));
            ActualCallKpis = KPI + NoKPI + NoCheck + NewEventRequest;
            ActualRequiredCallKpis = actualRequiredCallKpis;
        }
        public void Prepare(IEnumerable<Booking> bookings)
        {
            var month1 = DateTime.Today.AddMonths(-2);
            WrittenRevenue1 = bookings.Where(m => m.CreatedTime.Month == month1.Month && m.CreatedTime.Year == month1.Year).Sum(m => m.TotalWrittenRevenue);
            var month2 = DateTime.Today.AddMonths(-1);
            WrittenRevenue2 = bookings.Where(m => m.CreatedTime.Month == month2.Month && m.CreatedTime.Year == month2.Year).Sum(m => m.TotalWrittenRevenue);
            var month3 = DateTime.Today;
            WrittenRevenue3 = bookings.Where(m => m.CreatedTime.Month == month3.Month && m.CreatedTime.Year == month3.Year).Sum(m => m.TotalWrittenRevenue);
            TotalWrittenRevenue = WrittenRevenue1 + WrittenRevenue2 + WrittenRevenue3;
        }
    }
    public class HRConsolidateKPI
    {
        public User User { get; set; }
        public int RecruitmentCallKPIs { get; set; }
        public int EmployeeKPIs { get; set; }
        public void Prepare(IEnumerable<Candidate> leads)
        {
            RecruitmentCallKPIs = leads.Count();
            EmployeeKPIs = leads.Count(m => m.CandidateStatusRecord.Status.Value == CandidateStatus.ApprovedEmployment.Value);
        }
    }
}