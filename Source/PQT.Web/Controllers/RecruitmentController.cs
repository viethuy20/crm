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
            var rolesInterviewer = new List<string> { "manager", "hr", "admin" };
            var allSupervisors = _membershipService.GetAllInterviewers(rolesInterviewer.ToArray());
            model.Interviewers = allSupervisors;
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(RecruitmentModel model)
        {
            if (model.Candidate.OneFaceToFaceSummary.Status != RecruitmentStatus.Initial &&
                string.IsNullOrEmpty(model.Candidate.Information) && model.InformationFile == null)
                ModelState.AddModelError("InformationFile", Resource.TheFieldShouldNotBeEmpty);

            if (ModelState.IsValid)
            {
                var canExist =
                    _recruitmentService.GetExistCandidatesByMobile(
                        (int)model.Candidate.RecruitmentPositionID,
                        (int)model.Candidate.OfficeLocationID,
                        model.Candidate.Nationality,
                        model.Candidate.MobileNumber);
                if (canExist != null)
                {
                    TempData["error"] = "Number existing in another entry of same position";
                    return RedirectToAction("Create");
                }
                canExist =
                    _recruitmentService.GetExistCandidatesByEmail(
                        (int) model.Candidate.RecruitmentPositionID,
                        (int) model.Candidate.OfficeLocationID,
                        model.Candidate.Nationality,
                        model.Candidate.PersonalEmail);
                if (canExist != null)
                {
                    TempData["error"] = "Emails existing in another entry of same position";
                    return RedirectToAction("Create");
                }

                if (model.Create())
                {
                    TempData["message"] = "Create successful";
                    return RedirectToAction("Detail", new { id = model.Candidate.ID });
                }
            }

            var rolesInterviewer = new List<string> { "manager", "hr", "admin" };
            var allSupervisors = _membershipService.GetAllInterviewers(rolesInterviewer.ToArray());
            model.Interviewers = allSupervisors;
            TempData["error"] = "Save failed";
            return View(model);
        }
        public ActionResult Edit(int id, string backAction = "")
        {
            var model = new RecruitmentModel { BackAction = backAction };
            model.PrepareEdit(id);

            var rolesInterviewer = new List<string> { "manager", "hr", "admin" };
            var allSupervisors = _membershipService.GetAllInterviewers(rolesInterviewer.ToArray());
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
            if (model.Candidate.OneFaceToFaceSummary.Status != RecruitmentStatus.Initial &&
                string.IsNullOrEmpty(model.Candidate.Information) && model.InformationFile == null)
                ModelState.AddModelError("InformationFile", Resource.TheFieldShouldNotBeEmpty);
            if (ModelState.IsValid)
            {
                if (model.SaveEdit())
                {
                    TempData["message"] = "Save successful";
                    return RedirectToAction("Detail", new { id = model.Candidate.ID, backAction = model.BackAction });
                }
            }
            var rolesInterviewer = new List<string> { "manager", "hr", "admin" };
            var allSupervisors = _membershipService.GetAllInterviewers(rolesInterviewer.ToArray());
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
                model.Employee = _membershipService.GetUserIncludeAll((int)model.Candidate.EmployeeID);
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
                    CandidateID = model.Candidate.ID,
                    OfficeLocationID = model.Candidate.OfficeLocationID,
                    Nationality = model.Candidate.Nationality
                    //UserNo = _membershipService.GetTempUserNo()
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

            if (model.BirthCertificationFile != null)
            {
                string uploadPicture = UserPicture.UploadContract(model.BirthCertificationFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    model.BirthCertification = uploadPicture;
                }
            }
            if (model.FamilyCertificationFile != null)
            {
                string uploadPicture = UserPicture.UploadContract(model.FamilyCertificationFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    model.FamilyCertification = uploadPicture;
                }
            }
            if (model.FilledDeclarationFormFile != null)
            {
                string uploadPicture = UserPicture.UploadContract(model.FilledDeclarationFormFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    model.FilledDeclarationForm = uploadPicture;
                }
            }
            if (model.CertOfHighestEducationFile != null)
            {
                string uploadPicture = UserPicture.UploadContract(model.CertOfHighestEducationFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    model.CertOfHighestEducation = uploadPicture;
                }
            }
            if (model.IDCardFile != null)
            {
                string uploadPicture = UserPicture.UploadContract(model.IDCardFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    model.IDCard = uploadPicture;
                }
            }

            user.UserNo = model.UserNo;
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
            //user.SignedContract = model.SignedContract;
            user.BirthCertification = model.BirthCertification;
            user.FamilyCertification = model.FamilyCertification;
            user.FilledDeclarationForm = model.FilledDeclarationForm;
            user.CertOfHighestEducation = model.CertOfHighestEducation;
            user.IDCard = model.IDCard;
            user.OfferLetter = model.OfferLetter;
            user.TerminationLetter = model.TerminationLetter;
            var success = _membershipService.UpdateUserIncludeCollection(user);
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
            //bool isRecruitmentIntern = CurrentUser.HasRole("HR") || CurrentUser.HasRole("Recruitment Intern");
            //if (isRecruitmentIntern)
            //{
            //    saleId = CurrentUser.Identity.ID;
            //}
            IEnumerable<Candidate> data = new HashSet<Candidate>();
            Func<Candidate, bool> predicate = null;
            recordsTotal = _recruitmentService.GetCountCandidates(searchValue);
            // ReSharper disable once AssignNullToNotNullAttribute
            data = _recruitmentService.GetAllCandidates(searchValue, sortColumnDir, sortColumn, skip, pageSize);

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.CandidateNo,
                    CreatedTime = m.CreatedTime.ToString("dd/MM/yyyy"),
                    m.FirstName,
                    m.LastName,
                    m.MobileNumber,
                    m.PersonalEmail,
                    m.ApplicationSource,
                    m.OfficeLocationDisplay,
                    m.PsSummaryDateDisplay,
                    m.PsSummaryInterviewer,
                    m.PsSummaryStatusDisplay,
                    m.PsSummaryStatusReason,
                    m.OneFaceToFaceSummaryDateDisplay,
                    m.OneFaceToFaceSummaryInterviewer,
                    m.OneFaceToFaceSummaryStatusDisplay,
                    m.OneFaceToFaceSummaryStatusReason,
                    m.TwoFaceToFaceSummaryDateDisplay,
                    m.TwoFaceToFaceSummaryInterviewer,
                    m.TwoFaceToFaceSummaryStatusDisplay,
                    m.TwoFaceToFaceSummaryStatusReason,
                    m.StatusDisplay,
                    m.UserDisplay,
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
            var saleId = 0;
            //CurrentUser.Identity.ID;
            //bool isRecruitmentIntern = CurrentUser.HasRole("HR") || CurrentUser.HasRole("Recruitment Intern");
            //if (isRecruitmentIntern)
            //{
            //    saleId = CurrentUser.Identity.ID;
            //}
            IEnumerable<Candidate> data = new HashSet<Candidate>();
            recordsTotal = _recruitmentService.GetCountCandidatesInterviewToday(searchValue);
            data = _recruitmentService.GetAllCandidatesInterviewToday(searchValue, sortColumnDir, sortColumn, skip, pageSize);
            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.CandidateNo,
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
                    m.UserDisplay,
                    m.StatusCode,
                    m.ClassStatus,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

    }
}
