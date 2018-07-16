using System;
using System.ComponentModel;
using System.Web.Mvc;
using System.Web.Security;
using PQT.Domain.Abstract;
using PQT.Domain.Concrete;
using PQT.Domain.Entities;
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
                        ModelState.AddModelError("", Resource.InvalidEmailOrPassword);
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
            if (string.IsNullOrEmpty(model.OldPassword))
            {
                ModelState.AddModelError("OldPassword", Resource.TheFieldShouldNotBeEmpty);
            }
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

                CurrentUser.Identity.DisplayName = model.Username;
                CurrentUser.Identity.BusinessPhone = model.BusinessPhone;
                CurrentUser.Identity.MobilePhone = model.MobilePhone;
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
    }
}
