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
    public class CreateUserModel
    {
        public CreateUserModel()
        {
            BusinessDevelopmentUnit = BusinessDevelopmentUnit.None;
            SalesManagementUnit = SalesManagementUnit.None;
            SalesSupervision = SalesSupervision.None;
        }
        public List<int> UserRoles { get; set; }
        public IEnumerable<Role> Roles { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        public string DisplayName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldPasswordMustBeaMinimumLengthOf6")]
        public string Password { get; set; }

        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        //[Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ConfirmPasswordAndPasswordDoNotMatch")]
        //[DataType(DataType.Password)]
        //public string ConfirmPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        [RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailIsInvalid")]
        public string Email { get; set; }

        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string MobilePhone { get; set; }

        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string BusinessPhone { get; set; }
        [RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailIsInvalid")]
        public string PersonalEmail { get; set; }
        public string PassportID { get; set; }//Passport or ID
        public string Nationality { get; set; }
        public string Address { get; set; }
        public string Extension { get; set; }//clid
        public BusinessDevelopmentUnit BusinessDevelopmentUnit { get; set; }
        public SalesManagementUnit SalesManagementUnit { get; set; }
        public SalesSupervision SalesSupervision { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
        public DateTime? EmploymentDate { get; set; }
        public DateTime? FirstEvaluationDate { get; set; }
        public decimal? BasicSalary { get; set; }
        public UserStatus UserStatus { get; set; }
        public SalaryCurrency SalaryCurrency { get; set; }
        public int? DirectSupervisorID { get; set; }
        public int? OfficeLocationID { get; set; }

        public IEnumerable<string> GetAllCountries()
        {
            //create a new Generic list to hold the country names returned
            List<string> cultureList = new List<string>();

            //create an array of CultureInfo to hold all the cultures found, these include the users local cluture, and all the
            //cultures installed with the .Net Framework

            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => c.Name.Contains("-"));//CultureInfo.GetCultures(CultureTypes.AllCultures & CultureTypes.NeutralCultures);

            //loop through all the cultures found
            foreach (CultureInfo culture in cultures.Where(m => !string.IsNullOrEmpty(m.Name)))
            {
                try
                {
                    var region = new RegionInfo(culture.LCID);
                    if (!(cultureList.Contains(region.EnglishName)))
                        cultureList.Add(region.EnglishName);
                }
                catch (Exception)
                {
                }
            }
            return cultureList.OrderBy(m => m);
        }
    }

    public class EditUserModel
    {
        public EditUserModel()
        {
        }

        public EditUserModel(User user)
        {
            if (user != null)
            {
                ID = user.ID;
                DisplayName = user.DisplayName;
                Email = user.Email;
                BusinessPhone = user.BusinessPhone;
                Password = user.Password;
                //ConfirmPassword = user.Password;
                MobilePhone = user.MobilePhone;
                UserRoles = user.Roles;
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
            }
        }

        public IEnumerable<Role> UserRoles { get; set; }
        public IEnumerable<Role> Roles { get; set; }
        public List<int> SelectedRoles { get; set; }

        [HiddenInput]
        public int ID { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        public string DisplayName { get; set; }

        [MinLength(6, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldPasswordMustBeaMinimumLengthOf6")]
        public string Password { get; set; }

        //[Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ConfirmPasswordAndPasswordDoNotMatch")]
        //public string ConfirmPassword { get; set; }

        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        //[RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheEmailAddressEnteredIsInvalid")]
        public string Email { get; set; }

        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string MobilePhone { get; set; }


        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
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
        public string AvatarUrl
        {
            get
            {
                return "/data/user_img/" + ID + "/" + UserPicture;
            }
        }


        public IEnumerable<string> GetAllCountries()
        {
            //create a new Generic list to hold the country names returned
            List<string> cultureList = new List<string>();

            //create an array of CultureInfo to hold all the cultures found, these include the users local cluture, and all the
            //cultures installed with the .Net Framework

            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => c.Name.Contains("-"));//CultureInfo.GetCultures(CultureTypes.AllCultures & CultureTypes.NeutralCultures);

            //loop through all the cultures found
            foreach (CultureInfo culture in cultures.Where(m => !string.IsNullOrEmpty(m.Name)))
            {
                try
                {
                    var region = new RegionInfo(culture.LCID);
                    if (!(cultureList.Contains(region.EnglishName)))
                        cultureList.Add(region.EnglishName);
                }
                catch (Exception)
                {
                }
            }
            return cultureList.OrderBy(m => m);
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
        }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        public string Username { get; set; }
        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        //public string OldPassword { get; set; }
        public string Email { get; set; }

        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string MobilePhone { get; set; }
        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
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
        public ICollection<UserSalaryHistory> UserSalaryHistories { get; set; }
    }
}
