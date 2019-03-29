using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class Leave : Entity
    {
        public Leave()
        {
            LeaveDateFrom = DateTime.Today;
            LeaveDateTo = DateTime.Today;
            TypeOfLeave = TypeOfLeave.None;
            TypeOfLatenes = TypeOfLatenes.None;
            //LeaveStatus = LeaveStatus.None;
        }
        public DateTime LeaveDateFrom { get; set; }
        public DateTime LeaveDateTo { get; set; }
        public string Summary { get; set; }
        //public LeaveStatus LeaveStatus { get; set; }
        public LeaveType LeaveType { get; set; }
        public TypeOfLeave TypeOfLeave { get; set; }
        public TypeOfLatenes TypeOfLatenes { get; set; }
        public string Document { get; set; }
        public string ReasonReject { get; set; }
        public int? UserID { get; set; }//leave user 
        [ForeignKey("UserID")]
        public User User { get; set; }
        public int? CreatedUserID { get; set; }//approve or reject user
        [ForeignKey("CreatedUserID")]
        public User CreatedUser { get; set; }
        //public int? AprroveUserID { get; set; }//approve or reject user
        //[ForeignKey("AprroveUserID")]
        //public User AprroveUser { get; set; }

        public string LeaveDateDesc
        {
            get
            {
                if (LeaveDateFrom < LeaveDateTo)
                {
                    return LeaveDateFrom.ToString("dd/MM/yyyy") + " - " + LeaveDateTo.ToString("dd/MM/yyyy");
                }
                return LeaveDateFrom.ToString("dd/MM/yyyy");
            }
        }
        public string LeaveDateFromDisplay
        {
            get { return LeaveDateFrom.ToString("dd/MM/yyyy"); }
        }
        public string LeaveDateToDisplay
        {
            get { return LeaveDateTo.ToString("dd/MM/yyyy"); }
        }

        public string UserDisplay
        {
            get
            {
                if (User != null)
                    return User.DisplayName;
                return "";
            }
        }
        public string AprroveUserDisplay
        {
            get
            {
                if (CreatedUser != null)
                    return CreatedUser.DisplayName;
                return "";
            }
        }
        public string ReasonLeave
        {
            get
            {
                if (LeaveType.Value == LeaveType.Leave)
                    return TypeOfLeave.DisplayName;
                if (LeaveType.Value == LeaveType.Lateness)
                    return TypeOfLatenes.DisplayName;
                return LeaveType.DisplayName;
            }
        }
    }
    public class NonSalesDay : Entity
    {
        public NonSalesDay()
        {
        }
        [NotMapped]
        public string TempMonth { get; set; }
        public DateTime IssueMonth { get; set; }
        public int NonSalesDays { get; set; }
        public string Remarks { get; set; }
        public int? UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }
        public string UserDisplay
        {
            get
            {
                if (User != null)
                    return User.DisplayName;
                return "";
            }
        }
        public string IssueMonthDisplay
        {
            get { return IssueMonth.ToString("MMM yyyy"); }
        }
    }
    public class TechnicalIssueDay : Entity
    {
        public TechnicalIssueDay()
        {
        }

        [NotMapped]
        public string TempMonth { get; set; }
        public DateTime IssueMonth { get; set; }
        public int TechnicalIssueDays { get; set; }
        public string Remarks { get; set; }
        public int? UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }
        public string UserDisplay
        {
            get
            {
                if (User != null)
                    return User.DisplayName;
                return "";
            }
        }
        public string IssueMonthDisplay
        {
            get { return IssueMonth.ToString("MMM yyyy"); }
        }
    }
}
