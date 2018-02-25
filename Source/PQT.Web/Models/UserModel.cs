using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Entities;
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
        [RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheEmailAddressEnteredIsInvalid")]
        public string Email { get; set; }


        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[0-9\+]{1,}[0-9\-\ ]{3,15}$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldIsInvalid")]
        public string MobilePhone { get; set; }


        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[0-9\+]{1,}[0-9\-\ ]{3,15}$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldIsInvalid")]
        public string BusinessPhone { get; set; }
        public HttpPostedFileBase Picture { get; set; }

        public string Address { get; set; }
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
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ConfirmPasswordAndPasswordDoNotMatch")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        //[RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheEmailAddressEnteredIsInvalid")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[0-9\+]{1,}[0-9\-\ ]{3,15}$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldIsInvalid")]
        public string MobilePhone { get; set; }


        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[0-9\+]{1,}[0-9\-\ ]{3,15}$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldIsInvalid")]
        public string BusinessPhone { get; set; }

        public HttpPostedFileBase Picture { get; set; }

        public string UserPicture { get; set; }
        public DateTime? LastAccess { get; set; }
        public string Address { get; set; }

        public string AvatarUrl
        {
            get
            {
                return UrlHelper.Root + "/data/user_img/" + ID + "/" + UserPicture;
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
        }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        public string Username { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        public string OldPassword { get; set; }

        [MinLength(6, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldPasswordMustBeaMinimumLengthOf6")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ConfirmPasswordAndPasswordDoNotMatch")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldShouldNotBeEmpty")]
        //[RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheEmailAddressEnteredIsInvalid")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[0-9\+]{1,}[0-9\-\ ]{3,15}$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldIsInvalid")]
        public string MobilePhone { get; set; }


        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[0-9\+]{1,}[0-9\-\ ]{3,15}$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "TheFieldIsInvalid")]
        public string BusinessPhone { get; set; }

        public HttpPostedFileBase Picture { get; set; }
    }
}
