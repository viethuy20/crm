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

        public ActionResult Detail(int id = 0, string back = "")
        {
            var model = _repo.GetLeadNew(id);
            if (model == null)
            {
                TempData["error"] = "Call not found";
                return RedirectToAction("Index");
            }
            ViewBag.GoBack = back;
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
                return Json(new { Code = 1 });
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

        [DisplayName(@"Delete Assigned")]
        [HttpPost]
        public ActionResult DeleteAssigned(LeadModel model)
        {
            return Json(model.DeleteAssigned());
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
            var saleId = CurrentUser.Identity.ID;
            if (CurrentUser.HasRole("Manager"))
            {
                saleId = 0;
            }
            int recordsTotal = _repo.GetCountLeadNews(saleId, searchValue);
            var data = _repo.GetAllLeadNews(saleId, searchValue, sortColumnDir, sortColumn, skip, pageSize);
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
                    m.FirstFollowUpStatusClass,
                    BrochureClass = !string.IsNullOrEmpty(m.RequestBrochure) ? "Brochure" : "",
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
            var saleId = CurrentUser.Identity.ID;
            if (CurrentUser.HasRole("Manager"))
            {
                saleId = 0;
            }
            int recordsTotal = _repo.GetCountLeadNewsForAssigned(saleId, searchValue);
            var data = _repo.GetAllLeadNewsForAssigned(saleId, searchValue, sortColumnDir, sortColumn, skip, pageSize);
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
                    AssignDate = m.AssignDateDisplay,
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
                    m.FirstFollowUpStatusClass,
                    BrochureClass = !string.IsNullOrEmpty(m.RequestBrochure) ? "Brochure" : "",
                    FirstFollowUpStatus = m.FirstFollowUpStatusDisplay,
                    FinalStatus = m.FinalStatusDisplay,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

    }
}
