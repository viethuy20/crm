using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        }
        public List<int> UserRoles { get; set; }
        public IEnumerable<Role> Roles { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        public string DisplayName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldPasswordMustBeaMinimumLengthOf6")]
        public string Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        [Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ConfirmPasswordAndPasswordDoNotMatch")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        [RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailIsInvalid")]
        public string Email { get; set; }

        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string MobilePhone { get; set; }

        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string BusinessPhone { get; set; }

        public string Address { get; set; }
        public string Extension { get; set; }//clid
        public LevelSalesman LevelSalesman { get; set; }
        public DateTime? DateOfContract { get; set; }
        public DateTime? DateOfStarting { get; set; }
        public decimal? BasicSalary { get; set; }
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
                MobilePhone = user.MobilePhone;
                UserRoles = user.Roles;
                UserPicture = user.Picture;
                LastAccess = user.LastAccess;
                Address = user.Address;
                LevelSalesman = user.LevelSalesman;
                DateOfContract = user.DateOfContract;
                DateOfStarting = user.DateOfStarting;
                BasicSalary = user.BasicSalary;
                UserSalaryHistories = user.UserSalaryHistories;
                Extension = user.Extension;
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

        [Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ConfirmPasswordAndPasswordDoNotMatch")]
        public string ConfirmPassword { get; set; }

        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        //[RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheEmailAddressEnteredIsInvalid")]
        public string Email { get; set; }

        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string MobilePhone { get; set; }


        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string BusinessPhone { get; set; }


        public string Extension { get; set; }
        public string UserPicture { get; set; }
        public DateTime? LastAccess { get; set; }
        public string Address { get; set; }

        public LevelSalesman LevelSalesman { get; set; }
        public DateTime? DateOfContract { get; set; }
        public DateTime? DateOfStarting { get; set; }
        public decimal? BasicSalary { get; set; }
        public ICollection<UserSalaryHistory> UserSalaryHistories { get; set; }
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
            Email = user.Email;
            MobilePhone = user.MobilePhone;
            BusinessPhone = user.BusinessPhone;
            LevelSalesman = user.LevelSalesman.DisplayName;
            DateOfContract = user.DateOfContract != null ? Convert.ToDateTime(user.DateOfContract).ToString("dd-MMM-yyyy") : "";
            DateOfStarting = user.DateOfStarting != null ? Convert.ToDateTime(user.DateOfStarting).ToString("dd-MMM-yyyy") : "";
            BasicSalary = user.BasicSalary != null ? Convert.ToDecimal(user.BasicSalary).ToString("N") : "";
            Roles = string.Join(", ", user.Roles.Select(m => m.Name));
            UserSalaryHistories = user.UserSalaryHistories;
        }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        public string Username { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        public string OldPassword { get; set; }

        [MinLength(6, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldPasswordMustBeaMinimumLengthOf6")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ConfirmPasswordAndPasswordDoNotMatch")]
        public string ConfirmPassword { get; set; }

        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        //[RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailIsInvalid")]
        public string Email { get; set; }

        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string MobilePhone { get; set; }


        [RegularExpression(@"^[0-9\-\+\ \(\)]*$", ErrorMessage = "Phone number is invalid")]
        public string BusinessPhone { get; set; }

        //public HttpPostedFileBase Picture { get; set; }
        public string PictureBase64 { get; set; }
        public string LevelSalesman { get; set; }
        public string DateOfContract { get; set; }
        public string DateOfStarting { get; set; }
        public string BasicSalary { get; set; }
        public string Roles { get; set; }
        public ICollection<UserSalaryHistory> UserSalaryHistories { get; set; }
    }
}
