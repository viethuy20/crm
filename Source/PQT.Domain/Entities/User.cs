using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using PQT.Domain.Helpers;
using NS.Entity;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class User : EntityBase
    {
        public User()
        {
            Roles = new HashSet<Role>();
            Status = EntityUserStatus.Normal;
            UserStatus = UserStatus.Live;
            UserSalaryHistories = new HashSet<UserSalaryHistory>();

            BusinessDevelopmentUnit = BusinessDevelopmentUnit.None;
            SalesManagementUnit = SalesManagementUnit.None;
            SalesSupervision = SalesSupervision.None;
            FinanceAdminUnit = FinanceAdminUnit.None;
            ProductionUnit = ProductionUnit.None;
            OperationUnit = OperationUnit.None;
            HumanResourceUnit = HumanResourceUnit.None;
            MarketingManagementUnit = MarketingManagementUnit.None;
            ProcurementManagementUnit = ProcurementManagementUnit.None;
            ProjectManagementUnit = ProjectManagementUnit.None;
        }

        public User(User user)
        {
            ID = user.ID;
            DisplayName = user.DisplayName;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            BusinessPhone = user.BusinessPhone;
            MobilePhone = user.MobilePhone;
            if (user.Roles != null)
            {
                Roles = user.Roles.Select(r => new Role(r)).ToList();
            }
            else
            {
                Roles = new HashSet<Role>();
            }
            LastAccess = user.LastAccess;
            Address = user.Address;
            PersonalEmail = user.PersonalEmail;
            PassportID = user.PassportID;
            Nationality = user.Nationality;
            DateOfBirth = user.DateOfBirth;
            BusinessDevelopmentUnit = user.BusinessDevelopmentUnit;
            SalesManagementUnit = user.SalesManagementUnit;
            SalesSupervision = user.SalesSupervision;
            FinanceAdminUnit = user.FinanceAdminUnit;
            ProductionUnit = user.ProductionUnit;
            OperationUnit = user.OperationUnit;
            HumanResourceUnit = user.HumanResourceUnit;
            MarketingManagementUnit = user.MarketingManagementUnit;
            ProcurementManagementUnit = user.ProcurementManagementUnit;
            ProjectManagementUnit = user.ProjectManagementUnit;

            EmploymentEndDate = user.EmploymentEndDate;
            EmploymentDate = user.EmploymentDate;
            FirstEvaluationDate = user.FirstEvaluationDate;
            Status = user.Status;
            UserStatus = user.UserStatus;
            SalaryCurrency = user.SalaryCurrency;
            BasicSalary = user.BasicSalary;
            DirectSupervisorID = user.DirectSupervisorID;
            UserSalaryHistories = user.UserSalaryHistories;
            Extension = user.Extension;
            OfficeLocationID = user.OfficeLocationID;
            SignedContract = user.SignedContract;
            CandidateID = user.CandidateID;
            NotifyNumber = user.NotifyNumber;
        }

        #region Primitive

        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string BusinessPhone { get; set; }
        public string MobilePhone { get; set; }
        public string PersonalEmail { get; set; }
        public string PassportID { get; set; }//Passport or ID
        public string Picture { get; set; }
        public string Background { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? LastAccess { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public EntityUserStatus Status { get; set; }
        public UserStatus UserStatus { get; set; }
        public SalaryCurrency SalaryCurrency { get; set; }
        public int? TransferUserID { get; set; }
        //[ForeignKey("TransferUserID")]
        //public User TransferUser { get; set; }

        public int NotifyNumber { get; set; }
        public BusinessDevelopmentUnit BusinessDevelopmentUnit { get; set; }
        public SalesManagementUnit SalesManagementUnit { get; set; }
        public SalesSupervision SalesSupervision { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
        public DateTime? EmploymentDate { get; set; }
        public DateTime? FirstEvaluationDate { get; set; } //First Evaluation Date tu dong tinh la last day of 03 months from Employment Date
        public decimal? BasicSalary { get; set; }
        public ICollection<UserSalaryHistory> UserSalaryHistories { get; set; }

        public string Ip { get; set; }
        public string SignedContract { get; set; }
        public string Extension { get; set; }//private number for employees

        public int? DirectSupervisorID { get; set; }

        [ForeignKey("DirectSupervisorID")]
        public User DirectSupervisor { get; set; }

        public int? OfficeLocationID { get; set; }
        [ForeignKey("OfficeLocationID")]
        public OfficeLocation OfficeLocation { get; set; }


        public int? CandidateID { get; set; }

        public FinanceAdminUnit FinanceAdminUnit { get; set; }
        public ProductionUnit ProductionUnit { get; set; }
        public OperationUnit OperationUnit { get; set; }
        public HumanResourceUnit HumanResourceUnit { get; set; }
        public MarketingManagementUnit MarketingManagementUnit { get; set; }
        public ProcurementManagementUnit ProcurementManagementUnit { get; set; }
        public ProjectManagementUnit ProjectManagementUnit { get; set; }
        #endregion

        #region Navigation properties

        public virtual ICollection<Role> Roles { get; set; }
        #endregion

        #region Helpers

        public static Func<User, bool> HasName(string username)
        {
            return u => u.DisplayName.Trim().ToLower() == username.Trim().ToLower();
        }

        public string AvatarUrl
        {
            get
            {
                if (string.IsNullOrEmpty(Picture))
                {
                    return "/content/img/profile.png";
                }
                return "/data/user_img/" + ID + "/" + Picture;
            }
        }
        public string BackgroundUrl
        {
            get
            {
                if (string.IsNullOrEmpty(Background))
                {
                    return "/content/img/no-image.png";
                }
                return "/data/user_img/" + ID + "/" + Background;
            }
        }
        #endregion

        public string DateOfBirthDisplay
        {
            get
            {
                if (DateOfBirth != null) return Convert.ToDateTime(DateOfBirth).ToString("dd/MM/yyyy");
                return "";
            }
        }
        public string DirectSupervisorDisplay
        {
            get
            {
                if (DirectSupervisor != null) return DirectSupervisor.DisplayName;
                return "";
            }
        }
        public string UserStatusDisplay
        {
            get
            {
                return UserStatus.DisplayName;
            }
        }
        public string EmploymentDateDisplay
        {
            get
            {
                if (EmploymentDate != null) return Convert.ToDateTime(EmploymentDate).ToString("dd/MM/yyyy");
                return "";
            }
        }
        public string EmploymentEndDateDisplay
        {
            get
            {
                if (EmploymentEndDate != null) return Convert.ToDateTime(EmploymentEndDate).ToString("dd/MM/yyyy");
                return "";
            }
        }
        public string FirstEvaluationDateDisplay
        {
            get
            {
                if (FirstEvaluationDate != null) return Convert.ToDateTime(FirstEvaluationDate).ToString("dd/MM/yyyy");
                return "";
            }
        }
        public string RolesHtml
        {
            get
            {
                if (Roles != null)
                {
                    return string.Join("<br/>", Roles.Select(m => m.Name).OrderBy(m => m).ToArray());
                }
                return "";
            }
        }
    }
}
