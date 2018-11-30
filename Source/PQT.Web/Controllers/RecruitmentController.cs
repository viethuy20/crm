using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Utility;
using PQT.Web.Models;
using Resources;

namespace PQT.Web.Controllers
{
    public class RecruitmentController : Controller
    {
        //
        // GET: /Recruitment/
        private readonly IRecruitmentService _recruitmentService;
        private readonly IMembershipService _membershipService;
        private readonly IRoleService _roleService;
        private readonly ILoginTracker _loginTracker;
        public RecruitmentController(IRecruitmentService recruitmentService, IMembershipService membershipService, IRoleService roleService, ILoginTracker loginTracker)
        {
            _recruitmentService = recruitmentService;
            _membershipService = membershipService;
            _roleService = roleService;
            _loginTracker = loginTracker;
        }
        [DisplayName("Recruitment management")]
        public ActionResult Index()
        {
            return View();
        }

        [DisplayName("Candidates list")]
        public ActionResult Candidates()
        {
            return View();
        }

        public ActionResult Create()
        {
            var model = new RecruitmentModel();
            model.Candidate = new Candidate { UserID = CurrentUser.Identity.ID };
            var rolesInterviewer = new List<string> { "manager", "hr" };
            var allSupervisors = _membershipService.GetUsers(m => m.Status.Value == EntityStatus.Deleted.Value && (m.FinanceAdminUnit != FinanceAdminUnit.None ||
                                                                  m.SalesManagementUnit != SalesManagementUnit.None ||
                                                                  m.ProjectManagementUnit != ProjectManagementUnit.None || m.Roles
                                                                      .Select(r => r.Name.ToUpper())
                                                                      .Intersect(rolesInterviewer.Select(r1 => r1.ToUpper()))
                                                                      .Any()));
            model.Interviewers = allSupervisors;
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(RecruitmentModel model)
        {
            if (ModelState.IsValid)
            {
                var callExists = _recruitmentService.GetAllCandidates(m => (
                    m.RecruitmentPositionID == model.Candidate.RecruitmentPositionID &&
                    (!string.IsNullOrEmpty(m.MobileNumber) && m.MobileNumber == model.Candidate.MobileNumber) ||
                    (!string.IsNullOrEmpty(m.PersonalEmail) && m.PersonalEmail == model.Candidate.PersonalEmail)));
                if (callExists.Any())
                {
                    TempData["error"] = "Candidate contact exists in called list";
                    return RedirectToAction("Create");
                }

                if (model.Create())
                {
                    TempData["message"] = "Create successful";
                    return RedirectToAction("Detail", new { id = model.Candidate.ID });
                }
            }

            var rolesInterviewer = new List<string> { "manager", "hr","admin" };
            var allSupervisors = _membershipService.GetUsers(m => m.Status.Value == EntityStatus.Deleted.Value && (
                                                                  m.HumanResourceUnit == HumanResourceUnit.Coordinator ||
                                                                  m.SalesManagementUnit != SalesManagementUnit.None ||
                                                                  m.FinanceAdminUnit == FinanceAdminUnit.Manager ||
                                                                  m.ProjectManagementUnit != ProjectManagementUnit.None || m.Roles
                                                                      .Select(r => r.Name.ToUpper())
                                                                      .Intersect(rolesInterviewer.Select(r1 => r1.ToUpper()))
                                                                      .Any()));
            model.Interviewers = allSupervisors;
            TempData["error"] = "Save failed";
            return View(model);
        }
        public ActionResult Edit(int id)
        {
            var model = new RecruitmentModel();
            model.PrepareEdit(id);

            var rolesInterviewer = new List<string> { "manager", "hr" };
            var allSupervisors = _membershipService.GetUsers(m => m.Status.Value == EntityStatus.Deleted.Value && (m.FinanceAdminUnit != FinanceAdminUnit.None ||
                                                                  m.SalesManagementUnit != SalesManagementUnit.None ||
                                                                  m.ProjectManagementUnit != ProjectManagementUnit.None || m.Roles
                                                                      .Select(r => r.Name.ToUpper())
                                                                      .Intersect(rolesInterviewer.Select(r1 => r1.ToUpper()))
                                                                      .Any()));
            model.Interviewers = allSupervisors;
            if (model.Candidate == null)
            {
                TempData["error"] = "Data not found";
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(RecruitmentModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SaveEdit())
                {
                    TempData["message"] = "Save successful";
                    return RedirectToAction("Detail", new { id = model.Candidate.ID });
                }
            }
            var rolesInterviewer = new List<string> { "manager", "hr" };
            var allSupervisors = _membershipService.GetUsers(m => m.Status.Value == EntityStatus.Deleted.Value && (
                                                                      m.HumanResourceUnit == HumanResourceUnit.Coordinator ||
                                                                      m.SalesManagementUnit != SalesManagementUnit.None ||
                                                                      m.FinanceAdminUnit == FinanceAdminUnit.Manager ||
                                                                      m.ProjectManagementUnit != ProjectManagementUnit.None || m.Roles
                                                                          .Select(r => r.Name.ToUpper())
                                                                          .Intersect(rolesInterviewer.Select(r1 => r1.ToUpper()))
                                                                          .Any()));
            model.Interviewers = allSupervisors;
            TempData["error"] = "Save failed";
            return View(model);
        }

        public ActionResult Detail(int id = 0, string backAction = "")
        {
            var model = new RecruitmentModel { BackAction = backAction };
            model.PrepareEdit(id);
            if (model.Candidate == null)
            {
                TempData["error"] = "Candidate not found";
                return RedirectToAction("Index");
            }
            if (model.Candidate.EmployeeID > 0)
                model.Employee = _membershipService.GetUser((int)model.Candidate.EmployeeID);
            return View(model);
        }

        [DisplayName(@"Delete Candidate")]
        [HttpPost]
        public ActionResult DeleteCandidate(int id)
        {
            var model = new RecruitmentModel { id = id };
            return Json(model.DeleteCandidate());
        }

        [DisplayName(@"Request Employment")]
        public ActionResult RequestAction(int id = 0)
        {
            var model = new RecruitmentModel();
            model.PrepareEdit(id);
            if (model.Candidate == null)
            {
                TempData["error"] = "Candidate not found";
                return RedirectToAction("Index");
            }
            if (model.Candidate.EmployeeID > 0)
            {
                model.Employee = _membershipService.GetUser((int)model.Candidate.EmployeeID);
            }
            else
                model.Employee = new User
                {
                    DisplayName = string.IsNullOrEmpty(model.Candidate.EnglishName) ? model.Candidate.FullName : model.Candidate.EnglishName,
                    FirstName = model.Candidate.FirstName,
                    LastName = model.Candidate.LastName,
                    MobilePhone = model.Candidate.MobileNumber,
                    PersonalEmail = model.Candidate.PersonalEmail,
                    CandidateID = model.Candidate.ID
                };
            var role = _roleService.GetRoleByName(model.Candidate.RecruitmentPosition.Department);
            if (role != null)
            {
                model.RoleID = role.ID;
            }
            return View(model);
        }
        [DisplayName(@"Request Employment")]
        [HttpPost]
        public ActionResult RequestAction(RecruitmentModel model)
        {
            if (ModelState.IsValid)
            {
                var message = model.RequestAction();
                if (message == "")
                {
                    TempData["message"] = "Save successful";
                    return RedirectToAction("Detail", new { id = model.id });
                }
                TempData["error"] = message;
            }
            return View(model);
        }


        [DisplayName(@"Approval Employment")]
        [HttpPost]
        public ActionResult ApprovalAction(RecruitmentModel model)
        {
            return Json(model.ApprovalRequest());
        }
        [DisplayName(@"Reject Employment")]
        public ActionResult RejectAction(int id)
        {
            var model = new RecruitmentModel();
            model.PrepareEdit(id);
            return PartialView(model);
        }

        [DisplayName(@"Reject Employment")]
        [HttpPost]
        public ActionResult RejectAction(RecruitmentModel model)
        {
            if (string.IsNullOrEmpty(model.Reason))
            {
                return Json(new
                {
                    Message = "`Reason` must not be empty",
                    IsSuccess = false
                });
            }
            return Json(model.RejectRequest());
        }

        [DisplayName("Employees management")]
        public ActionResult Employees()
        {
            return View(new List<User>());
        }

        [DisplayName("Detail Employee")]
        public ActionResult DetailEmployment(int id)
        {
            User user = _membershipService.GetUserIncludeAll(id);
            if (user == null)
            {
                TempData["error"] = "Data not found";
                return RedirectToAction("Employees");
            }
            return View(user);
        }

        [DisplayName("Edit Employee")]
        public ActionResult EditEmployment(int id)
        {
            User user = _membershipService.GetUserIncludeAll(id);
            if (user == null)
            {
                TempData["error"] = "Data not found";
                return RedirectToAction("Employees");
            }
            var model = new EditUserModel(user);

            return View(model);
        }

        [DisplayName("Edit Employee")]
        [HttpPost]
        public ActionResult EditEmployment(EditUserModel model)
        {
            if (!ModelState.IsValid)
            {
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
            user.DisplayName = model.DisplayName;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.BusinessPhone = model.BusinessPhone;
            user.MobilePhone = model.MobilePhone;
            user.PersonalEmail = model.PersonalEmail;
            user.PassportID = model.PassportID;
            user.DateOfBirth = model.DateOfBirth;
            user.Nationality = model.Nationality;
            user.OfficeLocationID = model.OfficeLocationID;
            user.EmploymentEndDate = model.EmploymentEndDate;
            user.EmploymentDate = model.EmploymentDate;
            user.FirstEvaluationDate = model.FirstEvaluationDate;
            user.BasicSalary = model.BasicSalary;
            user.SalaryCurrency = model.SalaryCurrency;
            user.SignedContract = model.SignedContract;
            var success = _membershipService.UpdateUser(user);
            _roleService.AssignRoles(user, user.Roles.Select(m => m.ID));
            _loginTracker.ReloadUser(user.Email, user);
            if (success)
            {
                TempData["message"] = Resource.SaveSuccessful;
                return RedirectToAction("Employees");
            }
            ViewBag.Success = true;
            TempData["error"] = Resource.SaveFailed;
            return RedirectToAction("EditEmployment", new { id = model.ID });
        }
        [AjaxOnly]
        public ActionResult AjaxGetAllCandidates()
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
            var searchValue = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("search[value]").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                searchValue = Request.Form.GetValues("search[value]").FirstOrDefault().Trim().ToLower();
            }


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var saleId = 0;//CurrentUser.Identity.ID;
            IEnumerable<Candidate> candidates = new HashSet<Candidate>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                candidates = _recruitmentService.GetAllCandidates(m =>
                                               (saleId == 0 || m.UserID == saleId ||
                                                (m.User != null && m.User.TransferUserID == saleId)) && (
                                                   (m.EnglishName != null && m.EnglishName.ToLower().Contains(searchValue)) ||
                                                   (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                                                   (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                                                   (m.MobileNumber != null && m.MobileNumber.ToLower().Contains(searchValue)) ||
                                                   (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                                                   (m.ApplicationSource != null && m.ApplicationSource.ToLower().Contains(searchValue)) ||
                                                   (m.OfficeLocationDisplay != null && m.OfficeLocationDisplay.ToLower().Contains(searchValue))));
            }
            else
            {
                candidates = _recruitmentService.GetAllCandidates(m => (saleId == 0 || m.UserID == saleId || (m.User != null && m.User.TransferUserID == saleId)));
            }
            // ReSharper disable once AssignNullToNotNullAttribute

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        candidates = candidates.OrderBy(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "EnglishName":
                        candidates = candidates.OrderBy(s => s.EnglishName).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        candidates = candidates.OrderBy(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        candidates = candidates.OrderBy(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "MobileNumber":
                        candidates = candidates.OrderBy(s => s.MobileNumber).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmail":
                        candidates = candidates.OrderBy(s => s.PersonalEmail).ThenBy(s => s.ID);
                        break;
                    case "ApplicationSource":
                        candidates = candidates.OrderBy(s => s.ApplicationSource).ThenBy(s => s.ID);
                        break;
                    case "OfficeLocationDisplay":
                        candidates = candidates.OrderBy(s => s.OfficeLocationDisplay).ThenBy(s => s.ID);
                        break;
                    case "PsSummaryStatusDisplay":
                        candidates = candidates.OrderBy(s => s.PsSummaryStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "PsSummaryStatusReason":
                        candidates = candidates.OrderBy(s => s.PsSummaryStatusReason).ThenBy(s => s.ID);
                        break;
                    case "OneFaceToFaceSummaryStatusDisplay":
                        candidates = candidates.OrderBy(s => s.OneFaceToFaceSummaryStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "OneFaceToFaceSummaryStatusReason":
                        candidates = candidates.OrderBy(s => s.OneFaceToFaceSummaryStatusReason).ThenBy(s => s.ID);
                        break;
                    case "TwoFaceToFaceSummaryStatusDisplay":
                        candidates = candidates.OrderBy(s => s.TwoFaceToFaceSummaryStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "TwoFaceToFaceSummaryStatusReason":
                        candidates = candidates.OrderBy(s => s.TwoFaceToFaceSummaryStatusReason).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        candidates = candidates.OrderBy(s => s.StatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        candidates = candidates.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        candidates = candidates.OrderByDescending(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "EnglishName":
                        candidates = candidates.OrderByDescending(s => s.EnglishName).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        candidates = candidates.OrderByDescending(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        candidates = candidates.OrderByDescending(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "MobileNumber":
                        candidates = candidates.OrderByDescending(s => s.MobileNumber).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmail":
                        candidates = candidates.OrderByDescending(s => s.PersonalEmail).ThenBy(s => s.ID);
                        break;
                    case "ApplicationSource":
                        candidates = candidates.OrderByDescending(s => s.ApplicationSource).ThenBy(s => s.ID);
                        break;
                    case "OfficeLocationDisplay":
                        candidates = candidates.OrderByDescending(s => s.OfficeLocationDisplay).ThenBy(s => s.ID);
                        break;
                    case "PsSummaryStatusDisplay":
                        candidates = candidates.OrderByDescending(s => s.PsSummaryStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "PsSummaryStatusReason":
                        candidates = candidates.OrderByDescending(s => s.PsSummaryStatusReason).ThenBy(s => s.ID);
                        break;
                    case "OneFaceToFaceSummaryStatusDisplay":
                        candidates = candidates.OrderByDescending(s => s.OneFaceToFaceSummaryStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "OneFaceToFaceSummaryStatusReason":
                        candidates = candidates.OrderByDescending(s => s.OneFaceToFaceSummaryStatusReason).ThenBy(s => s.ID);
                        break;
                    case "TwoFaceToFaceSummaryStatusDisplay":
                        candidates = candidates.OrderByDescending(s => s.TwoFaceToFaceSummaryStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "TwoFaceToFaceSummaryStatusReason":
                        candidates = candidates.OrderByDescending(s => s.TwoFaceToFaceSummaryStatusReason).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        candidates = candidates.OrderByDescending(s => s.StatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        candidates = candidates.OrderByDescending(s => s.ID);
                        break;
                }
            }

            #endregion sort

            recordsTotal = candidates.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = candidates.Skip(skip).Take(pageSize).ToList();
            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    CreatedTime = m.CreatedTime.ToString("dd/MM/yyyy"),
                    m.FirstName,
                    m.LastName,
                    m.MobileNumber,
                    m.PersonalEmail,
                    m.ApplicationSource,
                    m.OfficeLocationDisplay,
                    m.PsSummaryStatusDisplay,
                    m.PsSummaryStatusReason,
                    m.OneFaceToFaceSummaryStatusDisplay,
                    m.OneFaceToFaceSummaryStatusReason,
                    m.TwoFaceToFaceSummaryStatusDisplay,
                    m.TwoFaceToFaceSummaryStatusReason,
                    m.StatusDisplay,
                    m.StatusCode,
                    m.ClassStatus,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        [AjaxOnly]
        public ActionResult AjaxGetAllCandidatesInterviewToday()
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
            var searchValue = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("search[value]").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                searchValue = Request.Form.GetValues("search[value]").FirstOrDefault().Trim().ToLower();
            }


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var saleId = 0;//CurrentUser.Identity.ID;
            IEnumerable<Candidate> candidates = new HashSet<Candidate>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                candidates = _recruitmentService.GetAllCandidates(m =>
                ((m.PsSummary != null && m.PsSummary.DateSelected == DateTime.Today) ||
                (m.OneFaceToFaceSummary != null && m.OneFaceToFaceSummary.DateSelected == DateTime.Today) ||
                (m.TwoFaceToFaceSummary != null && m.TwoFaceToFaceSummary.DateSelected == DateTime.Today)
                ) &&
                                               (saleId == 0 || m.UserID == saleId ||
                                                (m.User != null && m.User.TransferUserID == saleId)) && (
                                                   (m.EnglishName != null && m.EnglishName.ToLower().Contains(searchValue)) ||
                                                   (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                                                   (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                                                   (m.MobileNumber != null && m.MobileNumber.ToLower().Contains(searchValue)) ||
                                                   (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                                                   (m.ApplicationSource != null && m.ApplicationSource.ToLower().Contains(searchValue)) ||
                                                   (m.OfficeLocationDisplay != null && m.OfficeLocationDisplay.ToLower().Contains(searchValue))));
            }
            else
            {
                candidates = _recruitmentService.GetAllCandidates(m =>
                    ((m.PsSummary != null && m.PsSummary.DateSelected == DateTime.Today) ||
                     (m.OneFaceToFaceSummary != null && m.OneFaceToFaceSummary.DateSelected == DateTime.Today) ||
                     (m.TwoFaceToFaceSummary != null && m.TwoFaceToFaceSummary.DateSelected == DateTime.Today)
                    ) && (saleId == 0 || m.UserID == saleId || (m.User != null && m.User.TransferUserID == saleId)));
            }
            // ReSharper disable once AssignNullToNotNullAttribute

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        candidates = candidates.OrderBy(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "EnglishName":
                        candidates = candidates.OrderBy(s => s.EnglishName).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        candidates = candidates.OrderBy(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        candidates = candidates.OrderBy(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "MobileNumber":
                        candidates = candidates.OrderBy(s => s.MobileNumber).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmail":
                        candidates = candidates.OrderBy(s => s.PersonalEmail).ThenBy(s => s.ID);
                        break;
                    case "ApplicationSource":
                        candidates = candidates.OrderBy(s => s.ApplicationSource).ThenBy(s => s.ID);
                        break;
                    case "OfficeLocationDisplay":
                        candidates = candidates.OrderBy(s => s.OfficeLocationDisplay).ThenBy(s => s.ID);
                        break;
                    case "PsSummaryStatusDisplay":
                        candidates = candidates.OrderBy(s => s.PsSummaryStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "PsSummaryStatusReason":
                        candidates = candidates.OrderBy(s => s.PsSummaryStatusReason).ThenBy(s => s.ID);
                        break;
                    case "OneFaceToFaceSummaryStatusDisplay":
                        candidates = candidates.OrderBy(s => s.OneFaceToFaceSummaryStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "OneFaceToFaceSummaryStatusReason":
                        candidates = candidates.OrderBy(s => s.OneFaceToFaceSummaryStatusReason).ThenBy(s => s.ID);
                        break;
                    case "TwoFaceToFaceSummaryStatusDisplay":
                        candidates = candidates.OrderBy(s => s.TwoFaceToFaceSummaryStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "TwoFaceToFaceSummaryStatusReason":
                        candidates = candidates.OrderBy(s => s.TwoFaceToFaceSummaryStatusReason).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        candidates = candidates.OrderBy(s => s.StatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        candidates = candidates.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        candidates = candidates.OrderByDescending(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "EnglishName":
                        candidates = candidates.OrderByDescending(s => s.EnglishName).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        candidates = candidates.OrderByDescending(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        candidates = candidates.OrderByDescending(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "MobileNumber":
                        candidates = candidates.OrderByDescending(s => s.MobileNumber).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmail":
                        candidates = candidates.OrderByDescending(s => s.PersonalEmail).ThenBy(s => s.ID);
                        break;
                    case "ApplicationSource":
                        candidates = candidates.OrderByDescending(s => s.ApplicationSource).ThenBy(s => s.ID);
                        break;
                    case "OfficeLocationDisplay":
                        candidates = candidates.OrderByDescending(s => s.OfficeLocationDisplay).ThenBy(s => s.ID);
                        break;
                    case "PsSummaryStatusDisplay":
                        candidates = candidates.OrderByDescending(s => s.PsSummaryStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "PsSummaryStatusReason":
                        candidates = candidates.OrderByDescending(s => s.PsSummaryStatusReason).ThenBy(s => s.ID);
                        break;
                    case "OneFaceToFaceSummaryStatusDisplay":
                        candidates = candidates.OrderByDescending(s => s.OneFaceToFaceSummaryStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "OneFaceToFaceSummaryStatusReason":
                        candidates = candidates.OrderByDescending(s => s.OneFaceToFaceSummaryStatusReason).ThenBy(s => s.ID);
                        break;
                    case "TwoFaceToFaceSummaryStatusDisplay":
                        candidates = candidates.OrderByDescending(s => s.TwoFaceToFaceSummaryStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "TwoFaceToFaceSummaryStatusReason":
                        candidates = candidates.OrderByDescending(s => s.TwoFaceToFaceSummaryStatusReason).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        candidates = candidates.OrderByDescending(s => s.StatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        candidates = candidates.OrderByDescending(s => s.ID);
                        break;
                }
            }

            #endregion sort

            recordsTotal = candidates.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = candidates.Skip(skip).Take(pageSize).ToList();
            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    CreatedTime = m.CreatedTime.ToString("dd/MM/yyyy"),
                    m.FirstName,
                    m.LastName,
                    m.MobileNumber,
                    m.PersonalEmail,
                    m.ApplicationSource,
                    m.OfficeLocationDisplay,
                    m.PsSummaryStatusDisplay,
                    m.PsSummaryStatusReason,
                    m.OneFaceToFaceSummaryStatusDisplay,
                    m.OneFaceToFaceSummaryStatusReason,
                    m.TwoFaceToFaceSummaryStatusDisplay,
                    m.TwoFaceToFaceSummaryStatusReason,
                    m.StatusDisplay,
                    m.StatusCode,
                    m.ClassStatus,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

    }
}
