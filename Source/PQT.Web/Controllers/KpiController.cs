﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Models;

namespace PQT.Web.Controllers
{
    public class KpiController : Controller
    {
        //
        // GET: /Kpi/
        private readonly ILeadService _leadService;
        private readonly IEventService _eventService;

        public KpiController(ILeadService leadService, IEventService eventService)
        {
            _leadService = leadService;
            _eventService = eventService;
        }

        [DisplayName(@"Enquire KPIs")]
        public ActionResult Index()
        {
            var model = new KPIViewModel();
            return View(model);
        }

        [DisplayName(@"Consolidate KPIs")]
        public ActionResult Consolidate()
        {
            var model = new KPIViewModel();
            return View(model);
        }

        [DisplayName(@"Import VoIp")]
        public ActionResult ImportVoIp(int id = 0)
        {
            var model = new LeadMarkKPIModel();
            model.EventID = id;
            model.Event = _eventService.GetEvent(id);
            if (model.Event == null)
            {
                return RedirectToAction("NCLForManager", "Lead");
            }
            return View(model);
        }
        [HttpPost]
        [AjaxOnly]
        public ActionResult UploadVoIp(LeadMarkKPIModel model)
        {
            if (model.SaveFile())
            {
                model.ImportVoIp();
                model.IsSuccess = true;
                if (model.ImportVoIps.Any(m => !string.IsNullOrEmpty(m.Error)))
                {
                    model.ImportError = true;
                }
            }
            else
            {
                model.Message = "Upload File Failed";
            }
            model.SessionName = Guid.NewGuid().ToString("N");
            Session["SessionVoIpImport" + model.SessionName] = model;
            return Json(new
            {
                model.EventID,
                model.SessionName,
                model.Message,
                model.ImportError,
                model.IsSuccess,
            }, JsonRequestBehavior.AllowGet);
        }
        [AjaxOnly]
        public ActionResult ImportReview(string session = "")
        {
            var model = (LeadMarkKPIModel)Session["SessionVoIpImport" + session];
            return PartialView(model);
        }

        [AjaxOnly]
        public ActionResult MakeKPIReview()
        {
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

            var session = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Session").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                session = Request.Form.GetValues("Session").FirstOrDefault();
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            if (!string.IsNullOrEmpty(session))
            {
                var model = (LeadMarkKPIModel)Session["SessionVoIpImport" + session];
                if (model != null)
                {
                    var leads = _leadService.GetAllLeads(m =>
                        (model.EventID == 0 || m.EventID == model.EventID) &&
                        !m.MarkKPI &&
                        (m.LeadStatusRecord != LeadStatus.Reject &&
                         m.LeadStatusRecord != LeadStatus.Initial)).OrderByDescending(m => m.CreatedTime).AsEnumerable();
                    recordsTotal = leads.Count();
                    if (pageSize > recordsTotal)
                    {
                        pageSize = recordsTotal;
                    }

                    leads = leads.Skip(skip).Take(pageSize).ToList();
                    leads = model.MarkKPI(leads);
                    var json = new
                    {
                        draw = draw,
                        recordsFiltered = recordsTotal,
                        recordsTotal = recordsTotal,
                        data = leads.Select(m => new
                        {
                            m.ID,
                            CreatedTime = m.CreatedTime.ToString("dd/MM/yyyy HH:mm"),
                            UserName = m.User.DisplayName,
                            m.CompanyName,
                            m.GeneralLine,
                            m.DirectLine,
                            m.ClientName,
                            m.Salutation,
                            m.FirstName,
                            m.LastName,
                            m.BusinessPhone,
                            m.MobilePhone,
                            m.PersonalEmailAddress,
                            m.WorkEmailAddress,
                            m.MarkKPI,
                            m.KPIRemarks,
                        })
                    };
                    return Json(json, JsonRequestBehavior.AllowGet);
                }
            }
            var data = new List<Lead>();
            var json1 = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    CreatedTime = m.CreatedTime.ToString("dd/MM/yyyy HH:mm"),
                    UserName = m.User.DisplayName,
                    m.CompanyName,
                    m.GeneralLine,
                    m.DirectLine,
                    m.ClientName,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.BusinessPhone,
                    m.MobilePhone,
                    m.PersonalEmailAddress,
                    m.WorkEmailAddress,
                    m.MarkKPI,
                    m.KPIRemarks,
                })
            };
            return Json(json1, JsonRequestBehavior.AllowGet);
        }
        [AjaxOnly]
        public ActionResult ConfirmKPI(string session)
        {
            var model = (LeadMarkKPIModel)Session["SessionVoIpImport" + session];
            if (model != null)
            {
                model.ConfirmKPI();
                model.Message = "Confirm successful";
            }
            return PartialView(model);
        }
        [AjaxOnly]
        public ActionResult AjaxGetKpis()
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
            var datefrom = default(DateTime);
            if (Request.Form.GetValues("DateFrom").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                var _dateFrom = Request.Form.GetValues("DateFrom").FirstOrDefault().Trim().ToLower();
                if (!string.IsNullOrEmpty(_dateFrom))
                    datefrom = Convert.ToDateTime(_dateFrom);
            }
            var dateto = default(DateTime);
            if (Request.Form.GetValues("DateTo").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                var _dateto = Request.Form.GetValues("DateTo").FirstOrDefault().Trim().ToLower();
                if (!string.IsNullOrEmpty(_dateto))
                    dateto = Convert.ToDateTime(_dateto);
            }


            var eventId = 0;
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("EventID").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (Request.Form.GetValues("EventID") != null && !string.IsNullOrEmpty(Request.Form.GetValues("EventID").FirstOrDefault()))
                {
                    eventId = Convert.ToInt32(Request.Form.GetValues("EventID").FirstOrDefault());
                }
            }

            var userId = 0;
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("UserID").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (Request.Form.GetValues("UserID") != null && !string.IsNullOrEmpty(Request.Form.GetValues("UserID").FirstOrDefault()))
                {
                    userId = Convert.ToInt32(Request.Form.GetValues("UserID").FirstOrDefault());
                }
            }
            var statusCode = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("StatusCode").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                statusCode = Request.Form.GetValues("StatusCode").FirstOrDefault();
            }



            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            IEnumerable<Lead> leads = new HashSet<Lead>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                leads = _leadService.GetAllLeads(m =>
                    (m.LeadStatusRecord != LeadStatus.Reject &&
                     m.LeadStatusRecord != LeadStatus.Initial) &&
                    (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                    (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                    (eventId == 0 || m.EventID == eventId) &&
                    (userId == 0 || m.UserID == userId) &&
                    (string.IsNullOrEmpty(statusCode) || (statusCode == KPIStatus.KPI && m.MarkKPI)
                     || (statusCode == KPIStatus.NoKPI && !m.MarkKPI && !string.IsNullOrEmpty(m.FileNameImportKPI))
                     || (statusCode == KPIStatus.NoCheck && string.IsNullOrEmpty(m.FileNameImportKPI)))
                    && (
                        m.EventName.Contains(searchValue) ||
                        m.SalesmanName.Contains(searchValue) ||
                        m.StatusUpdateTimeStr.Contains(searchValue) ||
                        m.StatusDisplay.Contains(searchValue) ||
                        m.CompanyName.Contains(searchValue) ||
                        m.CountryCode.Contains(searchValue) ||
                        m.ClientName.Contains(searchValue) ||
                        m.DirectLine.Contains(searchValue) ||
                        m.CallBackDateDisplay.Contains(searchValue) ||
                        m.Salutation.Contains(searchValue) ||
                        m.FirstName.Contains(searchValue) ||
                        m.LastName.Contains(searchValue) ||
                        m.BusinessPhone.Contains(searchValue) ||
                        m.MobilePhone.Contains(searchValue) ||
                        m.WorkEmailAddress.Contains(searchValue) ||
                        m.WorkEmailAddress1.Contains(searchValue) ||
                        m.PersonalEmailAddress.Contains(searchValue))
                );
            }
            else
            {
                leads = _leadService.GetAllLeads(m =>
                    (m.LeadStatusRecord != LeadStatus.Reject &&
                     m.LeadStatusRecord != LeadStatus.Initial) &&
                    (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                    (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                    (eventId == 0 || m.EventID == eventId) &&
                    (userId == 0 || m.UserID == userId) &&
                    (string.IsNullOrEmpty(statusCode) || (statusCode == KPIStatus.KPI && m.MarkKPI)
                     || (statusCode == KPIStatus.NoKPI && !m.MarkKPI && !string.IsNullOrEmpty(m.FileNameImportKPI))
                     || (statusCode == KPIStatus.NoCheck && string.IsNullOrEmpty(m.FileNameImportKPI)))
                );
            }
            // ReSharper disable once AssignNullToNotNullAttribute

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "EventName":
                        leads = leads.OrderBy(s => s.EventName).ThenBy(s => s.ID);
                        break;
                    case "CreatedTime":
                        leads = leads.OrderBy(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "CompanyName":
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
                    case "Salutation":
                        leads = leads.OrderBy(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        leads = leads.OrderBy(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        leads = leads.OrderBy(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "BusinessPhone":
                        leads = leads.OrderBy(s => s.BusinessPhone).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone":
                        leads = leads.OrderBy(s => s.MobilePhone).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress":
                        leads = leads.OrderBy(s => s.WorkEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmailAddress":
                        leads = leads.OrderBy(s => s.PersonalEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "UserName":
                        leads = leads.OrderBy(s => s.User.DisplayName).ThenBy(s => s.ID);
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
                    case "EventName":
                        leads = leads.OrderByDescending(s => s.EventName).ThenBy(s => s.ID);
                        break;
                    case "CreatedTime":
                        leads = leads.OrderByDescending(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "CompanyName":
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
                    case "Salutation":
                        leads = leads.OrderByDescending(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        leads = leads.OrderByDescending(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        leads = leads.OrderByDescending(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "BusinessPhone":
                        leads = leads.OrderByDescending(s => s.BusinessPhone).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone":
                        leads = leads.OrderByDescending(s => s.MobilePhone).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress":
                        leads = leads.OrderByDescending(s => s.WorkEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmailAddress":
                        leads = leads.OrderByDescending(s => s.PersonalEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "UserName":
                        leads = leads.OrderByDescending(s => s.User.DisplayName).ThenBy(s => s.ID);
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
                    EventName = m.Event.EventName,
                    CreatedTime = m.CreatedTime.ToString("dd/MM/yyyy HH:mm"),
                    UserName = m.User.DisplayName,
                    m.CompanyName,
                    m.GeneralLine,
                    m.DirectLine,
                    m.ClientName,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.BusinessPhone,
                    m.MobilePhone,
                    m.PersonalEmailAddress,
                    m.WorkEmailAddress,
                    m.MarkKPI,
                    m.KPIRemarks,
                    m.ClassKPIStatus,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult AjaxGetConsolidateKpis()
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
            var datefrom = default(DateTime);
            if (Request.Form.GetValues("DateFrom").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                var _dateFrom = Request.Form.GetValues("DateFrom").FirstOrDefault().Trim().ToLower();
                if (!string.IsNullOrEmpty(_dateFrom))
                    datefrom = Convert.ToDateTime(_dateFrom);
            }
            var dateto = default(DateTime);
            if (Request.Form.GetValues("DateTo").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                var _dateto = Request.Form.GetValues("DateTo").FirstOrDefault().Trim().ToLower();
                if (!string.IsNullOrEmpty(_dateto))
                    dateto = Convert.ToDateTime(_dateto);
            }


            var eventId = 0;
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("EventID").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (Request.Form.GetValues("EventID") != null && !string.IsNullOrEmpty(Request.Form.GetValues("EventID").FirstOrDefault()))
                {
                    eventId = Convert.ToInt32(Request.Form.GetValues("EventID").FirstOrDefault());
                }
            }

            var userId = 0;
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("UserID").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (Request.Form.GetValues("UserID") != null && !string.IsNullOrEmpty(Request.Form.GetValues("UserID").FirstOrDefault()))
                {
                    userId = Convert.ToInt32(Request.Form.GetValues("UserID").FirstOrDefault());
                }
            }



            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            IEnumerable<Lead> leads = new HashSet<Lead>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                leads = _leadService.GetAllLeads(m =>
                    (m.LeadStatusRecord != LeadStatus.Reject &&
                     m.LeadStatusRecord != LeadStatus.Initial) &&
                    (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                    (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                    (eventId == 0 || m.EventID == eventId) &&
                    (userId == 0 || m.UserID == userId) &&
                    ((m.User != null && m.User.Email.Contains(searchValue)) ||
                        m.SalesmanName.Contains(searchValue))
                );
            }
            else
            {
                leads = _leadService.GetAllLeads(m =>
                    (m.LeadStatusRecord != LeadStatus.Reject &&
                     m.LeadStatusRecord != LeadStatus.Initial) &&
                    (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                    (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                    (eventId == 0 || m.EventID == eventId) &&
                    (userId == 0 || m.UserID == userId)
                );
            }
            // ReSharper disable once AssignNullToNotNullAttribute
            var model = new ConsolidateKPIModel();
            model.Prepare(leads);

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "KPI":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.KPI).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "NoKPI":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.NoKPI).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "NoCheck":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.NoCheck).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "Email":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.User.Email).ThenBy(s => s.User.ID).ToList();
                        break;
                    default:
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.User.DisplayName).ToList();
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "KPI":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.KPI).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "NoKPI":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.NoKPI).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "NoCheck":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.NoCheck).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "Email":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.User.Email).ThenBy(s => s.User.ID).ToList();
                        break;
                    default:
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.User.DisplayName).ToList();
                        break;
                }
            }

            #endregion sort

            recordsTotal = model.ConsolidateKpis.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = model.ConsolidateKpis.Skip(skip).Take(pageSize).ToList();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    UserID = m.User.ID,
                    UserName = m.User.DisplayName,
                    UserEmail = m.User.Email,
                    m.KPI,
                    m.NoKPI,
                    m.NoCheck,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

    }
}
