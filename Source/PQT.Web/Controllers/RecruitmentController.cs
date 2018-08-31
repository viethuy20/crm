using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Utility;
using PQT.Web.Models;

namespace PQT.Web.Controllers
{
    public class RecruitmentController : Controller
    {
        //
        // GET: /Recruitment/
        private readonly IRecruitmentService _recruitmentService;
        private readonly IMembershipService _membershipService;

        public RecruitmentController(IRecruitmentService recruitmentService, IMembershipService membershipService)
        {
            _recruitmentService = recruitmentService;
            _membershipService = membershipService;
        }
        [DisplayName("Recruitment management")]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Edit(int id)
        {
            var model = new RecruitmentModel();
            model.PrepareEdit(id);
            var allSupervisors = _membershipService.GetUsers(m => m.FinanceAdminUnit != FinanceAdminUnit.None ||
                                                                  m.SalesManagementUnit != SalesManagementUnit.None ||
                                                                  m.ProjectManagementUnit != ProjectManagementUnit.None);
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
                    TempData["message"] = "Create successful";
                    return RedirectToAction("Detail", new { id = model.Candidate.ID });
                }
            }
            var allSupervisors = _membershipService.GetUsers(m => m.FinanceAdminUnit != FinanceAdminUnit.None ||
                                                                  m.SalesManagementUnit != SalesManagementUnit.None ||
                                                                  m.ProjectManagementUnit != ProjectManagementUnit.None);
            model.Interviewers = allSupervisors;
            TempData["error"] = "Save failed";
            return View(model);
        }

        public ActionResult Create()
        {
            var model = new RecruitmentModel();
            model.Candidate = new Candidate { UserID = CurrentUser.Identity.ID };
            var allSupervisors = _membershipService.GetUsers(m => m.FinanceAdminUnit != FinanceAdminUnit.None ||
                                                                  m.SalesManagementUnit != SalesManagementUnit.None ||
                                                                  m.ProjectManagementUnit != ProjectManagementUnit.None);
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
            var allSupervisors = _membershipService.GetUsers(m => m.FinanceAdminUnit != FinanceAdminUnit.None ||
                                                                  m.SalesManagementUnit != SalesManagementUnit.None ||
                                                                  m.ProjectManagementUnit != ProjectManagementUnit.None);
            model.Interviewers = allSupervisors;
            TempData["error"] = "Save failed";
            return View(model);
        }
        public ActionResult Detail(int id = 0)
        {
            var model = new RecruitmentModel();
            model.PrepareEdit(id);
            if (model.Candidate == null)
            {
                TempData["error"] = "Candidate not found";
                return RedirectToAction("Index");
            }
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
        [HttpPost]
        public ActionResult RequestAction(RecruitmentModel model)
        {
            return Json(model.RequestAction());
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

    }
}
