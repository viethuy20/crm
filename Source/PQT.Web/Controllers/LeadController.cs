using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Models;

namespace PQT.Web.Controllers
{
    public class LeadController : Controller
    {
        //
        // GET: /Lead/
        private readonly ILeadService _repo;
        private readonly ICompanyRepository _companyRepo;

        public LeadController(ILeadService repo, ICompanyRepository companyRepo)
        {
            _repo = repo;
            _companyRepo = companyRepo;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">event id</param>
        /// <returns></returns>
        [DisplayName(@"Lead management")]
        public ActionResult Index(int id)
        {
            var model = new LeadModelView();
            model.Prepare(id);
            if (model.Event == null)
            {
                TempData["error"] = "Event not found";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        public ActionResult CallSheet(int eventId)
        {
            var model = new CallSheetModel(eventId);
            return PartialView(model);
        }
        [HttpPost]
        public ActionResult CallSheet(CallSheetModel model)
        {
            if (model.Lead.CompanyID == 0)
            {
                return Json(new { Code = 0, Message = "Please select company" });
            }

            if (model.TypeSubmit == "SaveCall")
            {
                if (model.Save())
                {
                    var callingModel = new CallingModel
                    {
                        Lead = model.Lead,
                        PhoneCall = {LeadID = model.Lead.ID}
                    };
                    return PartialView("CallingForm", callingModel);
                }
                return Json(new { Code = 0, Message = "Save failed" });
            }
            if (model.Save())
            {
                return Json(new { Code = 1, Model = model.Lead });
            }
            return Json(new { Code = 0, Message = "Save failed" });
        }

        public ActionResult CallingForm(int leadId)
        {
            var model = new CallingModel(leadId);
            return PartialView(model);
        }
        [HttpPost]
        public ActionResult CallingForm(CallingModel model)
        {
            if (model.Save())
            {
                return Json(new { Code = 1 });
            }
            return Json(new { Code = 0, Message = "Save failed" });
        }

        [AjaxOnly]
        public ActionResult AjaxGetLeads(int eventId)
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

            var saleId = PermissionHelper.SalesmanId();
            IEnumerable<Lead> leads = new HashSet<Lead>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                leads = _repo.GetAllLeads(m => m.EventID == eventId && (saleId == 0 || m.UserID == saleId || (m.User != null && m.User.TransferUserID == saleId)));
            }
            else
            {
                leads = _repo.GetAllLeads(m => m.EventID == eventId && (saleId == 0 || m.UserID == saleId || (m.User != null && m.User.TransferUserID == saleId)));
            }
            // ReSharper disable once AssignNullToNotNullAttribute


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
                    case "ClientName":
                        leads = leads.OrderBy(s => s.ClientName).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderBy(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "CallBackDate":
                        leads = leads.OrderBy(s => s.CallBackDate).ThenBy(s => s.ID);
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
                    case "ClientName":
                        leads = leads.OrderByDescending(s => s.ClientName).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderByDescending(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "CallBackDate":
                        leads = leads.OrderByDescending(s => s.CallBackDate).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderByDescending(s => s.ID);
                        break;
                }
            }


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
                    CreatedTime = m.CreatedTime.ToString("dd/MM/yyyy"),
                    Company = m.Company.CompanyName,
                    Country = m.Company.CountryCode,
                    m.GeneralLine,
                    m.ClientName,
                    m.DirectLine,
                    CallBackDate = m.CallBackDate == default(DateTime) ? "" : m.CallBackDate.ToString("dd/MM/yyyy"),
                    m.Event.EventName,
                    m.Event.EventCode
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        [AjaxOnly]
        public ActionResult AjaxGetNCList(int eventId)
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

            var saleId = PermissionHelper.SalesmanId();
            IEnumerable<Lead> leads = new HashSet<Lead>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                leads = _repo.GetAllLeads(m => m.EventID == eventId && (saleId == 0 || m.UserID == saleId || (m.User != null && m.User.TransferUserID == saleId)));
            }
            else
            {
                leads = _repo.GetAllLeads(m => m.EventID == eventId && (saleId == 0 || m.UserID == saleId || (m.User != null && m.User.TransferUserID == saleId)));
            }
            // ReSharper disable once AssignNullToNotNullAttribute


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
                    case "ClientName":
                        leads = leads.OrderBy(s => s.ClientName).ThenBy(s => s.ID);
                        break;
                    case "Salesman":
                        leads = leads.OrderBy(s => s.User.DisplayName).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        leads = leads.OrderBy(s => s.StatusDisplay).ThenBy(s => s.ID);
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
                    case "ClientName":
                        leads = leads.OrderByDescending(s => s.ClientName).ThenBy(s => s.ID);
                        break;
                    case "Salesman":
                        leads = leads.OrderByDescending(s => s.User.DisplayName).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        leads = leads.OrderByDescending(s => s.StatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderByDescending(s => s.ID);
                        break;
                }
            }


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
                    Salesman = m.User.DisplayName,
                    CreatedTime = m.DateCreatedDisplay,
                    Company = m.Company.CompanyName,
                    Country = m.Company.CountryCode,
                    m.ClientName,
                    StatusDisplay = m.StatusDisplay
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}
