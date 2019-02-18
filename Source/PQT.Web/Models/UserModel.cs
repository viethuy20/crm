using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using Resources;
using UrlHelper = PQT.Domain.Helpers.UrlHelper;

namespace PQT.Web.Models
{
    public class EditUserModel
    {
        public EditUserModel()
        {
            BusinessDevelopmentUnit = BusinessDevelopmentUnit.None;
            SalesManagementUnit = SalesManagementUnit.None;
            SalesSupervision = SalesSupervision.None;
            UserStatus = UserStatus.Live;
            FinanceAdminUnit = FinanceAdminUnit.None;
            ProductionUnit = ProductionUnit.None;
            OperationUnit = OperationUnit.None;
            HumanResourceUnit = HumanResourceUnit.None;
            MarketingManagementUnit = MarketingManagementUnit.None;
            ProcurementManagementUnit = ProcurementManagementUnit.None;
            ProjectManagementUnit = ProjectManagementUnit.None;
            UserContracts = new HashSet<UserContract>();
        }

        public EditUserModel(User user)
        {
            if (user != null)
            {
                ID = user.ID;
                DisplayName = user.DisplayName;
                LastName = user.LastName;
                FirstName = user.FirstName;
                Email = user.Email;
                BusinessPhone = user.BusinessPhone;
                Password = user.Password;
                //ConfirmPassword = user.Password;
                MobilePhone = user.MobilePhone;
                SelectedRoles = user.Roles.Select(m => m.ID).ToList();
                UserPicture = user.Picture;
                LastAccess = user.LastAccess;
                Address = user.Address;
                PersonalEmail = user.PersonalEmail;
                PassportID = user.PassportID;
                Nationality = user.Nationality;
                DateOfBirth = user.DateOfBirth;
                BusinessDevelopmentUnit = user.BusinessDevelopmentUnit;
                SalesManagementUnit = user.SalesManagementUnit;
                SalesSupervision = user.SalesSupervision;
                EmploymentEndDate = user.EmploymentEndDate;
                EmploymentDate = user.EmploymentDate;
                FirstEvaluationDate = user.FirstEvaluationDate;
                UserStatus = user.UserStatus;
                SalaryCurrency = user.SalaryCurrency;
                BasicSalary = user.BasicSalary;
                DirectSupervisorID = user.DirectSupervisorID;
                UserSalaryHistories = user.UserSalaryHistories;
                Extension = user.Extension;
                OfficeLocationID = user.OfficeLocationID;
                //SignedContract = user.SignedContract;
                FinanceAdminUnit = user.FinanceAdminUnit;
                ProductionUnit = user.ProductionUnit;
                OperationUnit = user.OperationUnit;
                HumanResourceUnit = user.HumanResourceUnit;
                MarketingManagementUnit = user.MarketingManagementUnit;
                ProcurementManagementUnit = user.ProcurementManagementUnit;
                ProjectManagementUnit = user.ProjectManagementUnit;
                CandidateID = user.CandidateID;
                UserContracts = user.UserContracts;
            }
        }

        public IEnumerable<User> Supervisors { get; set; }
        public IEnumerable<Role> Roles { get; set; }
        public List<int> SelectedRoles { get; set; }

        [HiddenInput]
        public int ID { get; set; }
        public int? CandidateID { get; set; }
        public Candidate Candidate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        public string DisplayName { get; set; }

        [MinLength(6, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldPasswordMustBeaMinimumLengthOf6")]
        public string Password { get; set; }

        //[Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ConfirmPasswordAndPasswordDoNotMatch")]
        //public string ConfirmPassword { get; set; }

        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]

        [RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailIsInvalid")]
        public string Email { get; set; }

        [RegularExpression(@"^[0-9]*$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "PhoneNumberIsInvalid")]
        public string MobilePhone { get; set; }


        [RegularExpression(@"^[0-9]*$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "PhoneNumberIsInvalid")]
        public string BusinessPhone { get; set; }
        [RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailIsInvalid")]
        public string PersonalEmail { get; set; }
        public string PassportID { get; set; }//Passport or ID
        public string Nationality { get; set; }

        public string Extension { get; set; }
        public string UserPicture { get; set; }
        public DateTime? LastAccess { get; set; }
        public string Address { get; set; }

        public BusinessDevelopmentUnit BusinessDevelopmentUnit { get; set; }
        public SalesManagementUnit SalesManagementUnit { get; set; }
        public SalesSupervision SalesSupervision { get; set; }


        public FinanceAdminUnit FinanceAdminUnit { get; set; }
        public ProductionUnit ProductionUnit { get; set; }
        public OperationUnit OperationUnit { get; set; }
        public HumanResourceUnit HumanResourceUnit { get; set; }
        public MarketingManagementUnit MarketingManagementUnit { get; set; }
        public ProcurementManagementUnit ProcurementManagementUnit { get; set; }
        public ProjectManagementUnit ProjectManagementUnit { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
        public DateTime? EmploymentDate { get; set; }
        public DateTime? FirstEvaluationDate { get; set; }
        public decimal? BasicSalary { get; set; }
        public UserStatus UserStatus { get; set; }
        public SalaryCurrency SalaryCurrency { get; set; }
        public int? DirectSupervisorID { get; set; }
        public int? OfficeLocationID { get; set; }
        public ICollection<UserSalaryHistory> UserSalaryHistories { get; set; }
        public ICollection<UserContract> UserContracts { get; set; }
        public HttpPostedFileBase SignedContractFile { get; set; }
        public string SignedContract { get; set; }
        public string AvatarUrl
        {
            get
            {
                return "/data/user_img/" + ID + "/" + UserPicture;
            }
        }
    }
    public class AccountModel
    {
        public AccountModel()
        {
        }

        public AccountModel(User user)
        {
            Username = user.DisplayName;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            MobilePhone = user.MobilePhone;
            BusinessPhone = user.BusinessPhone;
            Nationality = user.Nationality;
            PersonalEmail = user.PersonalEmail;
            PassportID = user.PassportID;
            DateOfBirth = user.DateOfBirth;
            BusinessDevelopmentUnit = user.BusinessDevelopmentUnit.DisplayName;
            SalesManagementUnit = user.SalesManagementUnit.DisplayName;
            SalesSupervision = user.SalesSupervision.DisplayName;
            UserStatus = user.UserStatus.DisplayName;
            SalaryCurrency = user.SalaryCurrency.DisplayName;
            OfficeLocation = user.OfficeLocation != null ? user.OfficeLocation.Name : "";

            EmploymentEndDate = user.EmploymentEndDate != null ? Convert.ToDateTime(user.EmploymentEndDate).ToString("dd-MMM-yyyy") : "";
            EmploymentDate = user.EmploymentDate != null ? Convert.ToDateTime(user.EmploymentDate).ToString("dd-MMM-yyyy") : "";
            FirstEvaluationDate = user.FirstEvaluationDate != null ? Convert.ToDateTime(user.FirstEvaluationDate).ToString("dd-MMM-yyyy") : "";
            BasicSalary = user.BasicSalary != null ? Convert.ToDecimal(user.BasicSalary).ToString("N") : "";
            Roles = string.Join(", ", user.Roles.Select(m => m.Name));
            UserSalaryHistories = user.UserSalaryHistories;
            //SignedContract = user.SignedContract;
            CandidateID = user.CandidateID;
        }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        //public string OldPassword { get; set; }
        public string Email { get; set; }

        [RegularExpression(@"^[0-9]*$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "PhoneNumberIsInvalid")]
        public string MobilePhone { get; set; }
        [RegularExpression(@"^[0-9]*$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "PhoneNumberIsInvalid")]
        public string BusinessPhone { get; set; }
        [RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailIsInvalid")]
        public string PersonalEmail { get; set; }
        public string PassportID { get; set; }//Passport or ID
        public DateTime? DateOfBirth { get; set; }
        public string Nationality { get; set; }
        public string PictureBase64 { get; set; }
        public string BusinessDevelopmentUnit { get; set; }
        public string SalesManagementUnit { get; set; }
        public string SalesSupervision { get; set; }
        public string EmploymentEndDate { get; set; }
        public string EmploymentDate { get; set; }
        public string FirstEvaluationDate { get; set; }
        public string BasicSalary { get; set; }
        public string UserStatus { get; set; }
        public string SalaryCurrency { get; set; }
        public string Roles { get; set; }
        public string OfficeLocation { get; set; }
        public string BackgroundBase64 { get; set; }
        public string SignedContract { get; set; }
        public int? CandidateID { get; set; }
        public ICollection<UserSalaryHistory> UserSalaryHistories { get; set; }
    }
}
