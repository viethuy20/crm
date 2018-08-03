using System;
using System.ComponentModel;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Concrete;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;
using PQT.Web.Models;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Infrastructure.Utility;
using Resources;

namespace PQT.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILoginTracker _loginTracker;
        private readonly IMembershipService _membership;

        public AccountController(IMembershipService membership, ILoginTracker loginTracker, IUnitRepository unitRepo)
        {
            _membership = membership;
            _loginTracker = loginTracker;
        }

        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        private RedirectResult RedirectToLocal(string returnUrl)
        {
            return Redirect(Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action("Index", "Home"));
        }

        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        public ActionResult Login(string returnUrl)
        {
            //Session["DbName"] = null;

            CurrentUser.Logout();

            ViewBag.returnUrl = returnUrl;
            var model = new LoginViewModel()
            {
                RememberMe = true,
            };
            return View(model);
        }

        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        [HttpPost]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    if (!CurrentUser.Login(model.Email, model.Password, model.RememberMe))
                    {
                        var user = _membership.GetUserByEmail(model.Email);
                        if (user == null)
                        {
                            ModelState.AddModelError("", @"Invalid email");
                        }
                        else if (user.Status == EntityStatus.Deleted)
                        {
                            ModelState.AddModelError("", @"Account has been deleted");
                        }
                        else if (user.UserStatus == UserStatus.Resigned)
                        {
                            ModelState.AddModelError("", @"Account has been resigned");
                        }
                        else if (user.UserStatus == UserStatus.Terminated)
                        {
                            ModelState.AddModelError("", @"Account has been terminated");
                        }
                        else
                        {
                            ModelState.AddModelError("", @"Invalid password");
                        }
                    }
                    else
                    {
                        return RedirectToLocal(returnUrl);
                    }
                }
                catch (ConcurrentLoginLimitException ex)
                {
                    ModelState.AddModelError("", "Your account is currently logged in somewhere else. Please try again later!");
                }
            }
            return View(model);
        }

        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        public ActionResult Logout(string returnUrl = "/")
        {
            CurrentUser.Logout();
            return RedirectToAction("Login", "Account", new { returnUrl });
        }

        [ActionName("Profile")]
        [DisplayName("Edit Profile")]
        public ActionResult EditProfile()
        {
            User user = _membership.GetUserIncludeAll(CurrentUser.Identity.ID);
            var model = new AccountModel(user);
            return View(model);
        }

        [HttpPost]
        [ActionName("Profile")]
        [DisplayName("Edit Profile")]
        public ActionResult EditProfile(AccountModel model)
        {
            //if (string.IsNullOrEmpty(model.OldPassword))
            //{
            //    ModelState.AddModelError("OldPassword", Resource.TheFieldShouldNotBeEmpty);
            //}
            //else
            //{
            //    var oldPassword = EncryptHelper.EncryptPassword(model.OldPassword);
            //    if (oldPassword != CurrentUser.Identity.Password)
            //    {
            //        ModelState.AddModelError("OldPassword", Resource.TheOldPasswordDoNotMatch);
            //    }
            //}
            if (ModelState.IsValid)
            {
                // Update user profile picture
                if (!string.IsNullOrEmpty(model.PictureBase64))
                {
                    UserPicture.Delete(CurrentUser.Identity.ID, CurrentUser.Identity.Picture);
                    string pictureFileName = UserPicture.Upload(CurrentUser.Identity.ID, model.PictureBase64);
                    CurrentUser.Identity.Picture = pictureFileName;
                }
                if (!string.IsNullOrEmpty(model.BackgroundBase64))
                {
                    UserPicture.Delete(CurrentUser.Identity.ID, CurrentUser.Identity.Background);
                    string pictureFileName = UserPicture.UploadBackground(CurrentUser.Identity.ID, model.BackgroundBase64);
                    CurrentUser.Identity.Background = pictureFileName;
                }

                CurrentUser.Identity.DisplayName = model.Username;
                CurrentUser.Identity.BusinessPhone = model.BusinessPhone;
                CurrentUser.Identity.MobilePhone = model.MobilePhone;
                CurrentUser.Identity.PersonalEmail = model.PersonalEmail;
                CurrentUser.Identity.PassportID = model.PassportID;
                //if (!string.IsNullOrEmpty(model.Password))
                //{
                //    CurrentUser.Identity.Password = EncryptHelper.EncryptPassword(model.Password);
                //}

                if (_membership.UpdateUser(CurrentUser.Identity))
                {

                    FormsAuthentication.SetAuthCookie(CurrentUser.Identity.Email, false);
                }

                _loginTracker.ReloadUser(CurrentUser.Identity.Email, CurrentUser.Identity);

                TempData["message"] = Resource.YourProfileHasBeenUpdated;
                return RedirectToAction("Profile");
            }
            model.UserSalaryHistories = CurrentUser.Identity.UserSalaryHistories;
            return View(model);
        }

        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        public ActionResult BypassLogin(string token)
        {
            return Redirect(BypassAuth.Decrypt(token));
        }


        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        public ActionResult ActiveKey()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["SecretKey"]) ||
                ConfigurationManager.AppSettings["ActiveKey"] == ConfigurationManager.AppSettings["SecretKey"])
            {
                return RedirectToAction("Index", "Home");
            }
            return View(0);
        }
        [HttpPost]
        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        public ActionResult ActiveKey(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                PermissionHelper.AddOrUpdateAppSettings("ActiveKey", key);
                return RedirectToAction("Index", "Home");
            }
            return View(0);
        }

    }
}
