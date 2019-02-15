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
using PQT.Web.Infrastructure.Helpers;
using Resources;
using StringHelper = PQT.Web.Infrastructure.Utility.StringHelper;

namespace PQT.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IMembershipService _membershipService;
        private readonly IRoleService _roleService;
        private readonly ILoginTracker _loginTracker;
        private readonly IRecruitmentService _recruitmentService;

        public UsersController(IMembershipService membershipService, IRoleService roleService, ILoginTracker loginTracker, IRecruitmentService recruitmentService)
        {
            _membershipService = membershipService;
            _roleService = roleService;
            _loginTracker = loginTracker;
            _recruitmentService = recruitmentService;
        }
        [DisplayName("User management")]
        public ActionResult Index(int role = 0)
        {
            //IEnumerable<User> users = _membershipService.GetUsers(m => m.Status == EntityUserStatus.Normal);
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
            var allSupervisors = _membershipService.GetUsers(m => m.FinanceAdminUnit != FinanceAdminUnit.None ||
                                                            m.SalesManagementUnit != SalesManagementUnit.None ||
                                                            m.ProjectManagementUnit != ProjectManagementUnit.None);
            //var supervisors = new List<SelectListItem>();
            //supervisors.AddRange(allSupervisors.Where(m => m.FinanceAdminUnit != FinanceAdminUnit.None).Select(m => new SelectListItem
            //{
            //    Value = m.ID.ToString(),
            //    Text = m.FinanceAdminUnit.DisplayName + " | " + m.DisplayName
            //}));
            //supervisors.AddRange(allSupervisors.Where(m => m.SalesManagementUnit != SalesManagementUnit.None).Select(m => new SelectListItem
            //{
            //    Value = m.ID.ToString(),
            //    Text = m.SalesManagementUnit.DisplayName + " | " + m.DisplayName
            //}));
            //supervisors.AddRange(allSupervisors.Where(m => m.ProjectManagementUnit != ProjectManagementUnit.None).Select(m => new SelectListItem
            //{
            //    Value = m.ID.ToString(),
            //    Text = m.ProjectManagementUnit.DisplayName + " | " + m.DisplayName
            //}));
            var model = new EditUserModel
            {
                SelectedRoles = new List<int>(),
                Roles = _roleService.GetAllRoles(),
                Supervisors = allSupervisors
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(EditUserModel model)
        {
            IEnumerable<int> userRoles = StringHelper.Ensure(Request.Form["SelectedRoles"])
                                                     .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(id => Convert.ToInt32(id));
            var exist = _membershipService.GetUserByEmail(model.Email);
            var allSupervisors = _membershipService.GetUsers(m => m.FinanceAdminUnit != FinanceAdminUnit.None ||
                                                                  m.SalesManagementUnit != SalesManagementUnit.None ||
                                                                  m.ProjectManagementUnit != ProjectManagementUnit.None);
            if (exist != null)
                ModelState.AddModelError("Email", Resource.EmailExists);

            if (!ModelState.IsValid)
            {
                model.SelectedRoles = userRoles.ToList();
                model.Roles = _roleService.GetAllRoles();
                model.Supervisors = allSupervisors;
                return View(model);
            }
            if (model.SignedContractFile != null)
            {
                string uploadPicture = UserPicture.UploadContract(model.SignedContractFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    model.SignedContract = uploadPicture;
                }
            }
            var user = new User
            {
                DisplayName = model.DisplayName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                BusinessPhone = model.BusinessPhone,
                MobilePhone = model.MobilePhone,
                Address = model.Address,
                BusinessDevelopmentUnit = model.BusinessDevelopmentUnit,
                SalesSupervision = model.SalesSupervision,
                SalesManagementUnit = model.SalesManagementUnit,
                EmploymentEndDate = model.EmploymentEndDate,
                EmploymentDate = model.EmploymentDate,
                BasicSalary = model.BasicSalary,
                PersonalEmail = model.PersonalEmail,
                PassportID = model.PassportID,
                Nationality = model.Nationality,
                DateOfBirth = model.DateOfBirth,
                UserStatus = model.UserStatus,
                SalaryCurrency = model.SalaryCurrency,
                FirstEvaluationDate = model.FirstEvaluationDate,
                DirectSupervisorID = model.DirectSupervisorID,
                OfficeLocationID = model.OfficeLocationID,
                FinanceAdminUnit = model.FinanceAdminUnit,
                ProductionUnit = model.ProductionUnit,
                OperationUnit = model.OperationUnit,
                HumanResourceUnit = model.HumanResourceUnit,
                MarketingManagementUnit = model.MarketingManagementUnit,
                ProcurementManagementUnit = model.ProcurementManagementUnit,
                ProjectManagementUnit = model.ProjectManagementUnit,
                SignedContract = model.SignedContract,
            };

            user = _membershipService.CreateUser(user);

            _roleService.AssignRoles(user, userRoles);
            TempData["message"] = Resource.AddSuccessful;
            return RedirectToAction("Index");
        }


        public ActionResult Edit(int id)
        {
            User user = _membershipService.GetUserIncludeAll(id);
            var allSupervisors = _membershipService.GetUsers(m => m.FinanceAdminUnit != FinanceAdminUnit.None ||
                                                                  m.SalesManagementUnit != SalesManagementUnit.None ||
                                                                  m.ProjectManagementUnit != ProjectManagementUnit.None);

            var model = new EditUserModel(user)
            {
                Roles = _roleService.GetAllRoles(),
                Supervisors = allSupervisors
            };

            if (model.CandidateID != null)
            {
                model.Candidate = _recruitmentService.GetCandidate((int)model.CandidateID);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(EditUserModel model)
        {
            //User user = _membershipService.GetUserByEmail(model.Email);
            //if (string.IsNullOrEmpty(model.Password) && model.Password != model.ConfirmPassword)
            //    ModelState.AddModelError("User.Password", Resource.PasswordMismatch);

            IEnumerable<int> userRoles = StringHelper.Ensure(Request.Form["SelectedRoles"])
                                                     .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(id => Convert.ToInt32(id));


            var allSupervisors = _membershipService.GetUsers(m => m.FinanceAdminUnit != FinanceAdminUnit.None ||
                                                                  m.SalesManagementUnit != SalesManagementUnit.None ||
                                                                  m.ProjectManagementUnit != ProjectManagementUnit.None);
            var exist = _membershipService.GetUserByEmail(model.Email);
            if (exist != null && exist.ID != model.ID)
                ModelState.AddModelError("Email", Resource.EmailExists);

            if (!ModelState.IsValid)
            {
                //var oldUser = _membershipService.GetUser(model.ID);
                model.SelectedRoles = _roleService.GetAllRoles().Where(m => userRoles.Contains(m.ID)).Select(m => m.ID).ToList();
                model.Roles = _roleService.GetAllRoles();
                model.Supervisors = allSupervisors;
                if (model.CandidateID != null)
                {
                    model.Candidate = _recruitmentService.GetCandidate((int)model.CandidateID);
                }
                return View(model);
            }

            var user = _membershipService.GetUser(model.ID);
            //save salary history
            if (user.BasicSalary > 0 && (user.BasicSalary != model.BasicSalary))
            {
                user.UserSalaryHistories.Add(new UserSalaryHistory
                {
                    BusinessDevelopmentUnit = user.BusinessDevelopmentUnit,
                    SalesManagementUnit = user.SalesManagementUnit,
                    SalesSupervision = user.SalesSupervision,
                    EmploymentEndDate = user.EmploymentEndDate,
                    EmploymentDate = user.EmploymentDate,
                    UserStatus = user.UserStatus,
                    SalaryCurrency = user.SalaryCurrency,
                    FirstEvaluationDate = user.FirstEvaluationDate,
                    BasicSalary = Convert.ToDecimal(user.BasicSalary),
                    FinanceAdminUnit = user.FinanceAdminUnit,
                    ProductionUnit = user.ProductionUnit,
                    OperationUnit = user.OperationUnit,
                    HumanResourceUnit = user.HumanResourceUnit,
                    MarketingManagementUnit = user.MarketingManagementUnit,
                    ProcurementManagementUnit = user.ProcurementManagementUnit,
                    ProjectManagementUnit = user.ProjectManagementUnit
                });
            }

            if (model.SignedContractFile != null)
            {
                string uploadPicture = UserPicture.UploadContract(model.SignedContractFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    model.SignedContract = uploadPicture;
                }
            }
            if (user.Status == EntityUserStatus.ApprovedEmployment && (!string.IsNullOrEmpty(user.Password) ||
                !string.IsNullOrEmpty(model.Password)))
                user.Status = EntityUserStatus.Normal;
            user.DisplayName = model.DisplayName;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.BusinessPhone = model.BusinessPhone;
            user.MobilePhone = model.MobilePhone;
            user.BusinessDevelopmentUnit = model.BusinessDevelopmentUnit;
            user.SalesManagementUnit = model.SalesManagementUnit;
            user.SalesSupervision = model.SalesSupervision;
            user.EmploymentEndDate = model.EmploymentEndDate;
            user.EmploymentDate = model.EmploymentDate;
            user.PersonalEmail = model.PersonalEmail;
            user.PassportID = model.PassportID;
            user.Nationality = model.Nationality;
            user.DateOfBirth = model.DateOfBirth;
            user.UserStatus = model.UserStatus;
            user.SalaryCurrency = model.SalaryCurrency;
            user.FirstEvaluationDate = model.FirstEvaluationDate;
            user.DirectSupervisorID = model.DirectSupervisorID;
            user.BasicSalary = model.BasicSalary;
            user.Extension = model.Extension;
            user.OfficeLocationID = model.OfficeLocationID;
            user.SignedContract = model.SignedContract;
            user.FinanceAdminUnit = model.FinanceAdminUnit;
            user.ProductionUnit = model.ProductionUnit;
            user.OperationUnit = model.OperationUnit;
            user.HumanResourceUnit = model.HumanResourceUnit;
            user.MarketingManagementUnit = model.MarketingManagementUnit;
            user.ProcurementManagementUnit = model.ProcurementManagementUnit;
            user.ProjectManagementUnit = model.ProjectManagementUnit;
            if (!string.IsNullOrEmpty(model.Password))
            {
                //user.Password = EncryptHelper.EncryptPassword(model.Password);
                user.Password = model.Password;
            }
            //user.LastAccess = model.LastAccess;
            var success = _membershipService.UpdateUser(user);

            _roleService.AssignRoles(user, userRoles);

            _loginTracker.ReloadUser(user.Email, user);
            if (success)
            {
                if (user.UserStatus != UserStatus.Live)
                {
                    LeadHelper.MakeMergeNoCallListToComResource(user.ID);
                }
                TempData["message"] = Resource.SaveSuccessful;
                return RedirectToAction("Index");
            }
            ViewBag.Success = true;
            model.Supervisors = allSupervisors;
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

            var salesUser = PermissionHelper.SalesmanId();
            if (salesUser > 0)
            {
                var currentUser = CurrentUser.Identity;
                IEnumerable<User> bookings = new List<User>();
                if (currentUser != null && currentUser.BusinessDevelopmentUnit != BusinessDevelopmentUnit.None)
                {
                    bookings =
                        _membershipService.GetUsers(m => m.DirectSupervisorID == currentUser.ID &&
                                                         m.Roles.Any(r => r.RoleLevel == RoleLevel.SalesLevel) &&
                                                         (m.DisplayName != null &&
                                                          m.DisplayName.ToLower().Contains(q.ToLower())) ||
                                                         (m.Email != null && m.Email.ToLower().Contains(q.ToLower())));
                }
                else if (currentUser != null && currentUser.SalesManagementUnit != SalesManagementUnit.None)
                {
                    bookings =
                        _membershipService.GetUsers(m => m.DirectSupervisorID == currentUser.ID &&
                                                         m.Roles.Any(r => r.RoleLevel == RoleLevel.SalesLevel) &&
                                                         (m.DisplayName != null &&
                                                          m.DisplayName.ToLower().Contains(q.ToLower())) ||
                                                         (m.Email != null && m.Email.ToLower().Contains(q.ToLower())));
                }
                return Json(bookings.Select(m => new { id = m.ID, text = m.DisplayName }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var bookings =
                    _membershipService.GetUsers(m => m.Roles.Any(r => r.RoleLevel == RoleLevel.SalesLevel) &&
                                                     (m.DisplayName != null &&
                                                      m.DisplayName.ToLower().Contains(q.ToLower())) ||
                                                     (m.Email != null && m.Email.ToLower().Contains(q.ToLower())));
                return Json(bookings.Select(m => new { id = m.ID, text = m.DisplayName }), JsonRequestBehavior.AllowGet);

            }
        }
        [AjaxOnly]
        public ActionResult GetPossibleHR(string q)
        {

            var salesUser = PermissionHelper.HRId();
            if (salesUser > 0)
            {
                var currentUser = CurrentUser.Identity;
                IEnumerable<User> users = new List<User>();
                if (currentUser != null && currentUser.BusinessDevelopmentUnit != BusinessDevelopmentUnit.None)
                {
                    users =
                        _membershipService.GetUsers(m => m.DirectSupervisorID == currentUser.ID &&
                                                         m.Roles.Any(r => r.Name.ToUpper().Contains("HR")) &&
                                                         (m.DisplayName != null &&
                                                          m.DisplayName.ToLower().Contains(q.ToLower())) ||
                                                         (m.Email != null && m.Email.ToLower().Contains(q.ToLower())));
                }
                else if (currentUser != null && currentUser.SalesManagementUnit != SalesManagementUnit.None)
                {
                    users =
                        _membershipService.GetUsers(m => m.DirectSupervisorID == currentUser.ID &&
                                                         m.Roles.Any(r => r.Name.ToUpper().Contains("HR")) &&
                                                         (m.DisplayName != null &&
                                                          m.DisplayName.ToLower().Contains(q.ToLower())) ||
                                                         (m.Email != null && m.Email.ToLower().Contains(q.ToLower())));
                }
                return Json(users.Select(m => new { id = m.ID, text = m.DisplayName }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var bookings =
                    _membershipService.GetUsers(m => m.Roles.Any(r => r.Name.ToUpper().Contains("HR")) &&
                                                     (m.DisplayName != null &&
                                                      m.DisplayName.ToLower().Contains(q.ToLower())) ||
                                                     (m.Email != null && m.Email.ToLower().Contains(q.ToLower())));
                return Json(bookings.Select(m => new { id = m.ID, text = m.DisplayName }), JsonRequestBehavior.AllowGet);

            }
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
                    (m.Status == EntityUserStatus.Normal ||
                     m.Status == EntityUserStatus.ApprovedEmployment) &&
                    (roleID == 0 || m.Roles.Select(r => m.ID).Contains(roleID)) &&
                    ((m.DisplayName.ToLower().Contains(searchValue)) ||
                     (m.UserStatusDisplay.ToLower().Contains(searchValue)) ||
                     (m.DirectSupervisorDisplay.ToLower().Contains(searchValue)) ||
                     (m.Email != null && m.Email.ToLower().Contains(searchValue)) ||
                     (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)) ||
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
                    (m.Status == EntityUserStatus.Normal ||
                    m.Status == EntityUserStatus.ApprovedEmployment) &&
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
                    m.FirstName,
                    m.LastName,
                    m.Email,
                    UserStatus = m.UserStatusDisplay,
                    m.PersonalEmail,
                    m.MobilePhone,
                    m.BusinessPhone,
                    DateOfBirth = m.DateOfBirthDisplay,
                    m.Extension,
                    DirectSupervisor = m.DirectSupervisorDisplay,
                    m.RolesHtml,
                    DisplayNameHtml = !string.IsNullOrEmpty(m.Picture) ? "<img src='/data/user_img/" + m.ID + "/" + m.Picture + "' class='img-rounded user-picture-small' style='max-width: 50px; max-height: 50px;' /> " + m.DisplayName : m.DisplayName,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }

}
