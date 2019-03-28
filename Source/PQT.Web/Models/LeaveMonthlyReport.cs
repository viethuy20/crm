using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NS.Helpers;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Web.Models
{
    public class LeaveMonthlyReport
    {
        public string Month { get; set; }

        public List<UserMonthlyReport> UserMonthlyReports { get; set; }
        public LeaveMonthlyReport()
        {
            Month = DateTime.Today.ToString("MM/yyyy");
        }
        public void Prepare(IEnumerable<Leave> leaves,DateTime monthReport)
        {
            UserMonthlyReports = new List<UserMonthlyReport>();
            var users = leaves.DistinctBy(m => m.UserID).Select(m => m.User).ToList();
            foreach (var user in users.DistinctBy(m => m.ID))
            {
                var item = new UserMonthlyReport
                {
                    User = user
                };
                item.Prepare(leaves.Where(m => m.UserID == user.ID), monthReport);
                UserMonthlyReports.Add(item);
            }
        }
    }

    public class UserMonthlyReport
    {
        public User User { get; set; }
        public double Leaves { get; set; }
        public double Lateness { get; set; }
        public double Resignation { get; set; }
        public double Total { get; set; }
        public void Prepare(IEnumerable<Leave> leads, DateTime monthReport)
        {
            Leaves = leads.Where(m => m.LeaveType == LeaveType.Leave &&
                                      m.TypeOfLeave != TypeOfLeave.HalfDayUnpaid).Sum(m => m.GetLeaveDaysByMonth(monthReport)) +
                     ((double)leads.Count(m => m.LeaveType == LeaveType.Leave &&
                                      m.TypeOfLeave == TypeOfLeave.HalfDayUnpaid) / 2);
            Lateness = leads.Count(m => m.LeaveType == LeaveType.Lateness);
            Resignation = leads.Count(m => m.LeaveType == LeaveType.Resignation);
            Total = Leaves + Lateness + Resignation;
        }
    }
}