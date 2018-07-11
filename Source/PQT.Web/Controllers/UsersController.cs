using System;
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
            //IEnumerable<User> users = _membershipService.GetUsers(m => m.Status == EntityStatus.Normal);
            ViewBag.roles = _roleService.GetAllRoles();
            return View(new List<User>());
        }

        [DisplayName("List deleted user")]
        public ActionResult ListDeletedUsers()
        {
            IEnumerable<User> users = _membershipService.GetUsersDeleted();
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

            _roleService.AssignRoles(user, userRoles);
            TempData["message"] = Resource.AddSuccessful;
            return RedirectToAction("Index");
        }


        public ActionResult Edit(int id)
        {
            User user = _membershipService.GetUserIncludeAll(id);
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
            user.Extension = model.Extension;
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.Password = EncryptHelper.EncryptPassword(model.Password);
            }
            //user.LastAccess = model.LastAccess;
            var success = _membershipService.UpdateUser(user);

            _roleService.AssignRoles(user, userRoles);

            _loginTracker.ReloadUser(user.Email, user);
            if (success)
            {
                TempData["message"] = Resource.SaveSuccessful;
                return RedirectToAction("Edit", new { id = model.ID });
            }
            ViewBag.Success = true;
            TempData["error"] = Resource.SaveFailed;
            return RedirectToAction("Edit", new { id = model.ID });
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



        [AjaxOnly]
        public ActionResult GetPossibleSalesman(string q)
        {
            var bookings =
                _membershipService.GetUsers(m => m.Roles.Any(r => r.RoleLevel == RoleLevel.SalesLevel) &&
                (m.DisplayName != null && m.DisplayName.ToLower().Contains(q.ToLower())) ||
                                        (m.Email != null && m.Email.ToLower().Contains(q.ToLower())));
            return Json(bookings.Select(m => new { id = m.ID, text = m.DisplayName }), JsonRequestBehavior.AllowGet);
        }


        [AjaxOnly]
        public ActionResult AjaxGetIndexView()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            // ReSharper disable once AssignNullToNotNullAttribute
            var start = Request.Form.GetValues("start").FirstOrDefault();
            // ReSharper disable once AssignNullToNotNullAttribute
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            // ReSharper disable once AssignNullToNotNullAttribute
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            #region For Search

            var searchValue = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("search[value]").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                searchValue = Request.Form.GetValues("search[value]").FirstOrDefault().Trim().ToLower();
            }

            var roleID = 0;
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("RoleID").FirstOrDefault() != null && !string.IsNullOrEmpty(Request.Form.GetValues("RoleID").FirstOrDefault()))
            {
                // ReSharper disable once PossibleNullReferenceException
                roleID = Convert.ToInt32(Request.Form.GetValues("RoleID").FirstOrDefault().Trim());
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            IEnumerable<User> users = new HashSet<User>();

            if (!string.IsNullOrEmpty(searchValue))
            {
                Func<User, bool> predicate = m =>
                m.Status == EntityStatus.Normal && 
                    (roleID == 0 || m.Roles.Select(r => m.ID).Contains(roleID)) &&
                    ((m.DisplayName.ToLower().Contains(searchValue)) ||
                     (m.Email != null && m.Email.ToLower().Contains(searchValue)) ||
                     (m.BusinessPhone != null && m.BusinessPhone.ToLower().Contains(searchValue)) ||
                     (m.MobilePhone != null && m.MobilePhone.ToLower().Contains(searchValue)) ||
                     (m.Roles != null && m.Roles.Any(r => r.Name.ToLower().Contains(searchValue))));
                users = _membershipService.GetUsers(predicate, sortColumnDir, sortColumn,
                    skip, pageSize);
                recordsTotal = _membershipService.GetCountUsers(predicate);
            }
            else
            {
                Func<User, bool> predicate = m =>
                    m.Status == EntityStatus.Normal &&
                    (roleID == 0 || m.Roles.Select(r => m.ID).Contains(roleID));
                users = _membershipService.GetUsers(predicate, sortColumnDir, sortColumn,
                    skip, pageSize);
                recordsTotal = _membershipService.GetCountUsers(predicate);
            }
            #endregion For Search

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = users.Select(m => new
                {
                    m.ID,
                    m.DisplayName,
                    m.Email,
                    m.MobilePhone,
                    m.BusinessPhone,
                    m.Extension,
                    m.RolesHtml,
                    DisplayNameHtml = !string.IsNullOrEmpty(m.Picture) ? "<img src='/data/user_img/" + m.ID + "/" + m.Picture + "' class='img-rounded user-picture-small' style='max-width: 50px; max-height: 50px;' /> " + m.DisplayName : m.DisplayName,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }

}
