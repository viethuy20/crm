﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;
using PQT.Web.Models;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Utility;
using NS.Entity;
using Resources;
using StringHelper = PQT.Web.Infrastructure.Utility.StringHelper;

namespace PQT.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IMembershipService _membershipService;
        private readonly IRoleService _roleService;
        private readonly ILoginTracker _loginTracker;
        private readonly IUnitRepository _repoUnit;

        public UsersController(IMembershipService membershipService, IRoleService roleService, ILoginTracker loginTracker, IUnitRepository repoUnit)
        {
            _membershipService = membershipService;
            _roleService = roleService;
            _loginTracker = loginTracker;
            _repoUnit = repoUnit;
        }
        [DisplayName("User management")]
        public ActionResult Index(int role = 0)
        {
            IEnumerable<User> users = _membershipService.GetUsers(m => m.Status == EntityStatus.Normal);
            ViewBag.roles = _roleService.GetAllRoles();
            return View(users);
        }

        [DisplayName("List deleted user")]
        public ActionResult ListDeletedUsers()
        {
            IEnumerable<User> users = _membershipService.GetUsers(m => m.Status == EntityStatus.Deleted);
            ViewBag.roles = _roleService.GetAllRoles();
            return View(users);
        }
        public ActionResult Create()
        {
            var model = new CreateUserModel
            {
                UserRoles = new List<int>(),
                Roles = _roleService.GetAllRoles(),
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(CreateUserModel model)
        {
            IEnumerable<int> userRoles = StringHelper.Ensure(Request.Form["SelectedRoles"])
                                                     .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(id => Convert.ToInt32(id));


            if (!ModelState.IsValid)
            {
                model.UserRoles = userRoles.ToList();
                model.Roles = _roleService.GetAllRoles();
                return View(model);
            }
            var user = new User
            {
                DisplayName = model.DisplayName,
                Email = model.Email,
                Password = model.Password,
                BusinessPhone = model.BusinessPhone,
                MobilePhone = model.MobilePhone,
                Address = model.Address,
                LevelSalesman = model.LevelSalesman,
                DateOfContract = model.DateOfContract,
                DateOfStarting = model.DateOfStarting,
                BasicSalary = model.BasicSalary,
            };

            user = _membershipService.CreateUser(user);

            string userPicture = UserPicture.Upload(user.ID, model.Picture);
            if (!string.IsNullOrEmpty(userPicture))
                _membershipService.UpdateUserPicture(user.ID, userPicture);

            _roleService.AssignRoles(user, userRoles);
            TempData["message"] = Resource.AddSuccessful;
            return RedirectToAction("Index");
        }


        public ActionResult Edit(int id)
        {
            User user = _membershipService.GetUser(id);
            var model = new EditUserModel(user)
            {
                Roles = _roleService.GetAllRoles(),
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(EditUserModel model)
        {
            User user = _membershipService.GetUserByEmail(model.Email);
            if (string.IsNullOrEmpty(model.Password) && model.Password != model.ConfirmPassword)
                ModelState.AddModelError("User.Password", Resource.PasswordMismatch);

            IEnumerable<int> userRoles = StringHelper.Ensure(Request.Form["SelectedRoles"])
                                                     .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(id => Convert.ToInt32(id));


            if (!ModelState.IsValid)
            {

                //var oldUser = _membershipService.GetUser(model.ID);
                model.UserRoles = _roleService.GetAllRoles().Where(m => userRoles.Contains(m.ID));
                model.Roles = _roleService.GetAllRoles();
                return View(model);
            }

            user = _membershipService.GetUser(model.ID);
            //save salary history
            if (user.BasicSalary > 0 && user.BasicSalary != model.BasicSalary)
            {
                user.UserSalaryHistories.Add(new UserSalaryHistory
                {
                    LevelSalesman = user.LevelSalesman,
                    DateOfStarting = user.DateOfStarting,
                    DateOfContract = user.DateOfContract,
                    BasicSalary = Convert.ToDecimal(user.BasicSalary)
                });
            }

            user.DisplayName = model.DisplayName;
            user.Email = model.Email;
            user.BusinessPhone = model.BusinessPhone;
            user.MobilePhone = model.MobilePhone;
            user.LevelSalesman = model.LevelSalesman;
            user.DateOfStarting = model.DateOfStarting;
            user.DateOfContract = model.DateOfContract;
            user.BasicSalary = model.BasicSalary;
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.Password = EncryptHelper.EncryptPassword(model.Password);
            }
            //user.LastAccess = model.LastAccess;
            var success = _membershipService.UpdateUser(user);
            string userPicture = UserPicture.Upload(model.ID, model.Picture);
            if (!string.IsNullOrEmpty(userPicture))
                _membershipService.UpdateUserPicture(user.ID, userPicture);

            _roleService.AssignRoles(user, userRoles);

            _loginTracker.ReloadUser(user.Email, user);
            if (success)
            {
                TempData["message"] = Resource.SaveSuccessful;
                return RedirectToAction("Index");
            }
            ViewBag.Success = true;
            ViewBag.Message = Resource.SaveFailed;
            return RedirectToAction("Edit", new { Id = model.ID });
        }
        [AjaxOnly]
        public ActionResult Delete_keeptrack(int id)
        {
            User user = _membershipService.GetUser(id);
            if (user != null)
            {
                _membershipService.DeleteUser(user.ID);
            }
            return Json(1);
        }
        [HttpPost, ActionName("ReActive")]
        public ActionResult ReActive(int id)
        {
            try
            {
                _membershipService.ReActiveUser(id);
            }
            catch (Exception)
            {
                TempData["Message"] = Resource.UnableToReactiveUser;
                return RedirectToAction("ListDeletedUsers");
            }
            TempData["Message"] = Resource.UserEmailActive;
            return RedirectToAction("ListDeletedUsers");
        }
    }

}
