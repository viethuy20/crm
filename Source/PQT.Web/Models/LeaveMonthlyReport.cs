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
        public void Prepare(IEnumerable<Leave> leaves)
        {
            UserMonthlyReports = new List<UserMonthlyReport>();
            var users = leaves.DistinctBy(m=>m.UserID).Select(m => m.User).ToList();
            foreach (var user in users.DistinctBy(m => m.ID))
            {
                var item = new UserMonthlyReport
                {
                    User = user
                };
                item.Prepare(leaves.Where(m => m.UserID == user.ID));
                UserMonthlyReports.Add(item);
            }
        }
    }

    public class UserMonthlyReport
    {
        public User User { get; set; }
        public int Leaves { get; set; }
        public int Lateness { get; set; }
        public int Resignation { get; set; }
        public int Total { get; set; }
        public void Prepare(IEnumerable<Leave> leads)
        {
            Leaves = leads.Count(m=>m.LeaveType == LeaveType.Leave);
            Lateness = leads.Count(m=>m.LeaveType == LeaveType.Lateness);
            Resignation = leads.Count(m=>m.LeaveType == LeaveType.Resignation);
            Total = Leaves + Lateness + Resignation;
        }
    }
}