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
    public class NewEventController : Controller
    {
        private readonly ILeadNewService _repo;
        private readonly IMembershipService _memRepo;
        public NewEventController(ILeadNewService repo, IMembershipService memRepo)
        {
            _repo = repo;
            _memRepo = memRepo;
        }

        [DisplayName(@"New Event Management")]
        public ActionResult Index()
        {
            return View();
        }

        [DisplayName(@"Assigned New Event")]
        public ActionResult Assigned()
        {
            return View();
        }

        public ActionResult Detail(int id = 0)
        {
            var model = _repo.GetLeadNew(id);
            if (model == null)
            {
                TempData["error"] = "Call not found";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = new CallingModel();
            model.PrepareNewEvent(id);
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Edit(CallingModel model)
        {
            if (model.SaveEditNewEvent())
            {
                return Json(new { Code = 1});
            }
            return Json(new { Code = 0, Message = "Save failed" });
        }

        public ActionResult Assign(int id)
        {
            var model = new CallingModel();
            model.PrepareNewEvent(id);
            model.Sales = _memRepo.GetAllSalesmans();
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Assign(CallingModel model)
        {
            if (model.AssignSalesNewEvent())
            {
                return Json(new { Code = 1 });
            }
            return Json(new { Code = 0, Message = "Save failed" });
        }

        [HttpPost]
        public ActionResult Delete(LeadModel model)
        {
            return Json(model.DeleteLeadNew());
        }


        [DisplayName(@"Request Brochure")]
        public ActionResult RequestBrochure(int id)
        {
            var model = new LeadModel(id);
            return PartialView(model);
        }

        [DisplayName(@"Request Brochure")]
        [HttpPost]
        public ActionResult RequestBrochure(LeadModel model)
        {
            return Json(model.RequestBrochure());
        }

        [AjaxOnly]
        public ActionResult AjaxGetNewEvents()
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
            var saleId = CurrentUser.Identity.ID;
            if (CurrentUser.HasRole("Admin") || CurrentUser.HasRole("Manager"))
            {
                saleId = 0;
            }
            IEnumerable<LeadNew> leads = new HashSet<LeadNew>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                leads = _repo.GetAllLeadNews(m => m.AssignUserID == null &&
                                               (saleId == 0 || (m.UserID == saleId) ||
                                                (m.User != null && m.User.TransferUserID == saleId)) && (
                                                   m.DateCreatedDisplay.Contains(searchValue) ||
                                                   m.CompanyName.ToLower().Contains(searchValue) ||
                                                   m.CountryCode.ToLower().Contains(searchValue) ||
                                                   m.JobTitle.ToLower().Contains(searchValue) ||
                                                   m.DirectLine.ToLower().Contains(searchValue) ||
                                                   (m.Salutation != null && m.Salutation.ToLower().Contains(searchValue)) ||
                                                   (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                                                   (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                                                   (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                                                   (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                                                   (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                                                   (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                                                   (m.WorkEmail1 != null && m.WorkEmail1.ToLower().Contains(searchValue)) ||
                                                   (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                                                   (m.NewTopics != null && m.NewTopics.ToLower().Contains(searchValue)) ||
                                                   (m.NewLocations != null && m.NewLocations.ToLower().Contains(searchValue)) ||
                                                   (m.NewDateFromDisplay.ToLower().Contains(searchValue)) ||
                                                   (m.NewDateToDisplay.ToLower().Contains(searchValue)) ||
                                                   (m.NewTrainingTypeDisplay.ToLower().Contains(searchValue)) ||
                                                   (m.FirstFollowUpStatusDisplay.ToLower().Contains(searchValue)) ||
                                                   (m.FinalStatusDisplay.ToLower().Contains(searchValue))));
            }
            else
            {
                leads = _repo.GetAllLeadNews(m => m.AssignUserID == null && (saleId == 0 || (m.UserID == saleId) ||
                                                   (m.User != null && m.User.TransferUserID == saleId)));
            }
            // ReSharper disable once AssignNullToNotNullAttribute

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        leads = leads.OrderBy(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        leads = leads.OrderBy(s => s.Company.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        leads = leads.OrderBy(s => s.Company.CountryCode).ThenBy(s => s.ID);
                        break;
                    case "JobTitle":
                        leads = leads.OrderBy(s => s.JobTitle).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderBy(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "Sales":
                        leads = leads.OrderBy(s => s.Sales).ThenBy(s => s.ID);
                        break;
                    case "Salutation":
                        leads = leads.OrderBy(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        leads = leads.OrderBy(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        leads = leads.OrderBy(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone1":
                        leads = leads.OrderBy(s => s.MobilePhone1).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone2":
                        leads = leads.OrderBy(s => s.MobilePhone2).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone3":
                        leads = leads.OrderBy(s => s.MobilePhone3).ThenBy(s => s.ID);
                        break;
                    case "WorkEmail":
                        leads = leads.OrderBy(s => s.WorkEmail).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmail":
                        leads = leads.OrderBy(s => s.PersonalEmail).ThenBy(s => s.ID);
                        break;
                    case "EstimatedDelegateNumber":
                        leads = leads.OrderBy(s => s.EstimatedDelegateNumber).ThenBy(s => s.ID);
                        break;
                    case "TrainingBudgetPerHead":
                        leads = leads.OrderBy(s => s.TrainingBudgetPerHead).ThenBy(s => s.ID);
                        break;
                    case "GoodTrainingMonth":
                        leads = leads.OrderBy(s => s.GoodTrainingMonth).ThenBy(s => s.ID);
                        break;
                    case "FirstFollowUpStatus":
                        leads = leads.OrderBy(s => s.FirstFollowUpStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "FinalStatus":
                        leads = leads.OrderBy(s => s.FinalStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "NewTopics":
                        leads = leads.OrderBy(s => s.NewTopics).ThenBy(s => s.ID);
                        break;
                    case "NewLocations":
                        leads = leads.OrderBy(s => s.NewLocations).ThenBy(s => s.ID);
                        break;
                    case "NewDateFrom":
                        leads = leads.OrderBy(s => s.NewDateFrom).ThenBy(s => s.ID);
                        break;
                    case "NewTrainingTypeDisplay":
                        leads = leads.OrderBy(s => s.NewTrainingTypeDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        leads = leads.OrderByDescending(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        leads = leads.OrderByDescending(s => s.Company.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        leads = leads.OrderByDescending(s => s.Company.CountryCode).ThenBy(s => s.ID);
                        break;
                    case "JobTitle":
                        leads = leads.OrderByDescending(s => s.JobTitle).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderByDescending(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "Sales":
                        leads = leads.OrderByDescending(s => s.Sales).ThenBy(s => s.ID);
                        break;
                    case "Salutation":
                        leads = leads.OrderByDescending(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        leads = leads.OrderByDescending(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        leads = leads.OrderByDescending(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone1":
                        leads = leads.OrderByDescending(s => s.MobilePhone1).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone2":
                        leads = leads.OrderByDescending(s => s.MobilePhone2).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone3":
                        leads = leads.OrderByDescending(s => s.MobilePhone3).ThenBy(s => s.ID);
                        break;
                    case "WorkEmail":
                        leads = leads.OrderByDescending(s => s.WorkEmail).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmail":
                        leads = leads.OrderByDescending(s => s.PersonalEmail).ThenBy(s => s.ID);
                        break;
                    case "EstimatedDelegateNumber":
                        leads = leads.OrderByDescending(s => s.EstimatedDelegateNumber).ThenBy(s => s.ID);
                        break;
                    case "TrainingBudgetPerHead":
                        leads = leads.OrderByDescending(s => s.TrainingBudgetPerHead).ThenBy(s => s.ID);
                        break;
                    case "GoodTrainingMonth":
                        leads = leads.OrderByDescending(s => s.GoodTrainingMonth).ThenBy(s => s.ID);
                        break;
                    case "FirstFollowUpStatus":
                        leads = leads.OrderByDescending(s => s.FirstFollowUpStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "FinalStatus":
                        leads = leads.OrderByDescending(s => s.FinalStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "NewTopics":
                        leads = leads.OrderByDescending(s => s.NewTopics).ThenBy(s => s.ID);
                        break;
                    case "NewLocations":
                        leads = leads.OrderByDescending(s => s.NewLocations).ThenBy(s => s.ID);
                        break;
                    case "NewDates":
                        leads = leads.OrderByDescending(s => s.NewDateFrom).ThenBy(s => s.ID);
                        break;
                    case "NewTrainingTypeDisplay":
                        leads = leads.OrderByDescending(s => s.NewTrainingTypeDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderByDescending(s => s.ID);
                        break;
                }
            }

            #endregion sort

            recordsTotal = leads.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = leads.Skip(skip).Take(pageSize).ToList();
            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.EventID,
                    CreatedTime = m.DateCreatedDisplay,
                    Company = m.Company.CompanyName,
                    Country = m.Company.CountryCodeAndDialing,
                    m.JobTitle,
                    m.DirectLine,
                    m.Event.EventName,
                    m.Event.EventCode,
                    m.Sales,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.MobilePhone1,
                    m.MobilePhone2,
                    m.MobilePhone3,
                    m.PersonalEmail,
                    m.WorkEmail,
                    m.EstimatedDelegateNumber,
                    TrainingBudgetPerHead = m.TrainingBudgetPerHead != null ? Convert.ToDecimal(m.TrainingBudgetPerHead).ToString("N2") : "",
                    m.GoodTrainingMonth,
                    m.NewTopics,
                    m.NewLocations,
                    NewDates = m.NewDatesDisplay,
                    m.NewTrainingTypeDisplay,
                    FirstFollowUpStatus = m.FirstFollowUpStatusDisplay,
                    FinalStatus = m.FinalStatusDisplay,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult AjaxGetAssignedNewEvents()
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
            var saleId = CurrentUser.Identity.ID;
            if (CurrentUser.HasRole("Admin") || CurrentUser.HasRole("Manager"))
            {
                saleId = 0;
            }
            IEnumerable<LeadNew> leads = new HashSet<LeadNew>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                leads = _repo.GetAllLeadNews(m => m.AssignUserID != null &&
                                               (saleId == 0 ||
                                                (m.AssignUserID == saleId) ||
                                                (m.AssignUser.TransferUserID == saleId)) && (
                                                   m.DateCreatedDisplay.Contains(searchValue) ||
                                                   m.CompanyName.ToLower().Contains(searchValue) ||
                                                   m.CountryCode.ToLower().Contains(searchValue) ||
                                                   m.JobTitle.ToLower().Contains(searchValue) ||
                                                   m.DirectLine.ToLower().Contains(searchValue) ||
                                                   (m.Salutation != null && m.Salutation.ToLower().Contains(searchValue)) ||
                                                   (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                                                   (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                                                   (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                                                   (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                                                   (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                                                   (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                                                   (m.WorkEmail1 != null && m.WorkEmail1.ToLower().Contains(searchValue)) ||
                                                   (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                                                   (m.NewTopics != null && m.NewTopics.ToLower().Contains(searchValue)) ||
                                                   (m.NewLocations != null && m.NewLocations.ToLower().Contains(searchValue)) ||
                                                   (m.NewDateFromDisplay.ToLower().Contains(searchValue)) ||
                                                   (m.NewDateToDisplay.ToLower().Contains(searchValue)) ||
                                                   (m.NewTrainingTypeDisplay.ToLower().Contains(searchValue)) ||
                                                   (m.FirstFollowUpStatusDisplay.ToLower().Contains(searchValue)) ||
                                                   (m.FinalStatusDisplay.ToLower().Contains(searchValue))));
            }
            else
            {
                leads = _repo.GetAllLeadNews(m => m.AssignUserID != null && (saleId == 0 ||
                                                   (m.AssignUserID == saleId) ||
                                                   (m.AssignUser.TransferUserID == saleId)));
            }
            // ReSharper disable once AssignNullToNotNullAttribute

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        leads = leads.OrderBy(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        leads = leads.OrderBy(s => s.Company.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        leads = leads.OrderBy(s => s.Company.CountryCode).ThenBy(s => s.ID);
                        break;
                    case "JobTitle":
                        leads = leads.OrderBy(s => s.JobTitle).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderBy(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "Sales":
                        leads = leads.OrderBy(s => s.Sales).ThenBy(s => s.ID);
                        break;
                    case "Salutation":
                        leads = leads.OrderBy(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        leads = leads.OrderBy(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        leads = leads.OrderBy(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone1":
                        leads = leads.OrderBy(s => s.MobilePhone1).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone2":
                        leads = leads.OrderBy(s => s.MobilePhone2).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone3":
                        leads = leads.OrderBy(s => s.MobilePhone3).ThenBy(s => s.ID);
                        break;
                    case "WorkEmail":
                        leads = leads.OrderBy(s => s.WorkEmail).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmail":
                        leads = leads.OrderBy(s => s.PersonalEmail).ThenBy(s => s.ID);
                        break;
                    case "EstimatedDelegateNumber":
                        leads = leads.OrderBy(s => s.EstimatedDelegateNumber).ThenBy(s => s.ID);
                        break;
                    case "TrainingBudgetPerHead":
                        leads = leads.OrderBy(s => s.TrainingBudgetPerHead).ThenBy(s => s.ID);
                        break;
                    case "GoodTrainingMonth":
                        leads = leads.OrderBy(s => s.GoodTrainingMonth).ThenBy(s => s.ID);
                        break;
                    case "FirstFollowUpStatus":
                        leads = leads.OrderBy(s => s.FirstFollowUpStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "FinalStatus":
                        leads = leads.OrderBy(s => s.FinalStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "NewTopics":
                        leads = leads.OrderBy(s => s.NewTopics).ThenBy(s => s.ID);
                        break;
                    case "NewLocations":
                        leads = leads.OrderBy(s => s.NewLocations).ThenBy(s => s.ID);
                        break;
                    case "NewDateFrom":
                        leads = leads.OrderBy(s => s.NewDateFrom).ThenBy(s => s.ID);
                        break;
                    case "NewTrainingTypeDisplay":
                        leads = leads.OrderBy(s => s.NewTrainingTypeDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        leads = leads.OrderByDescending(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        leads = leads.OrderByDescending(s => s.Company.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        leads = leads.OrderByDescending(s => s.Company.CountryCode).ThenBy(s => s.ID);
                        break;
                    case "JobTitle":
                        leads = leads.OrderByDescending(s => s.JobTitle).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderByDescending(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "Sales":
                        leads = leads.OrderByDescending(s => s.Sales).ThenBy(s => s.ID);
                        break;
                    case "Salutation":
                        leads = leads.OrderByDescending(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        leads = leads.OrderByDescending(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        leads = leads.OrderByDescending(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone1":
                        leads = leads.OrderByDescending(s => s.MobilePhone1).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone2":
                        leads = leads.OrderByDescending(s => s.MobilePhone2).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone3":
                        leads = leads.OrderByDescending(s => s.MobilePhone3).ThenBy(s => s.ID);
                        break;
                    case "WorkEmail":
                        leads = leads.OrderByDescending(s => s.WorkEmail).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmail":
                        leads = leads.OrderByDescending(s => s.PersonalEmail).ThenBy(s => s.ID);
                        break;
                    case "EstimatedDelegateNumber":
                        leads = leads.OrderByDescending(s => s.EstimatedDelegateNumber).ThenBy(s => s.ID);
                        break;
                    case "TrainingBudgetPerHead":
                        leads = leads.OrderByDescending(s => s.TrainingBudgetPerHead).ThenBy(s => s.ID);
                        break;
                    case "GoodTrainingMonth":
                        leads = leads.OrderByDescending(s => s.GoodTrainingMonth).ThenBy(s => s.ID);
                        break;
                    case "FirstFollowUpStatus":
                        leads = leads.OrderByDescending(s => s.FirstFollowUpStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "FinalStatus":
                        leads = leads.OrderByDescending(s => s.FinalStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "NewTopics":
                        leads = leads.OrderByDescending(s => s.NewTopics).ThenBy(s => s.ID);
                        break;
                    case "NewLocations":
                        leads = leads.OrderByDescending(s => s.NewLocations).ThenBy(s => s.ID);
                        break;
                    case "NewDates":
                        leads = leads.OrderByDescending(s => s.NewDateFrom).ThenBy(s => s.ID);
                        break;
                    case "NewTrainingTypeDisplay":
                        leads = leads.OrderByDescending(s => s.NewTrainingTypeDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderByDescending(s => s.ID);
                        break;
                }
            }

            #endregion sort

            recordsTotal = leads.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = leads.Skip(skip).Take(pageSize).ToList();
            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.EventID,
                    CreatedTime = m.DateCreatedDisplay,
                    Company = m.Company.CompanyName,
                    Country = m.Company.CountryCodeAndDialing,
                    m.JobTitle,
                    m.DirectLine,
                    m.Event.EventName,
                    m.Event.EventCode,
                    m.Sales,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.MobilePhone1,
                    m.MobilePhone2,
                    m.MobilePhone3,
                    m.PersonalEmail,
                    m.WorkEmail,
                    m.EstimatedDelegateNumber,
                    TrainingBudgetPerHead = m.TrainingBudgetPerHead != null ? Convert.ToDecimal(m.TrainingBudgetPerHead).ToString("N2") : "",
                    m.GoodTrainingMonth,
                    m.NewTopics,
                    m.NewLocations,
                    NewDates = m.NewDatesDisplay,
                    m.NewTrainingTypeDisplay,
                    FirstFollowUpStatus = m.FirstFollowUpStatusDisplay,
                    FinalStatus = m.FinalStatusDisplay,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

    }
}
