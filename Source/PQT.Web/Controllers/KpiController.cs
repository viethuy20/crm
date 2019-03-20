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
using PQT.Web.Infrastructure.Utility;
using PQT.Web.Models;

namespace PQT.Web.Controllers
{
    public class KpiController : Controller
    {
        //
        // GET: /Kpi/
        private readonly ILeadService _leadService;
        private readonly ILeadNewService _leadNewService;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;
        private readonly IRecruitmentService _recruitmentService;
        private readonly ILeaveService _leaveService;

        public KpiController(ILeadService leadService, IEventService eventService, ILeadNewService leadNewService, IBookingService bookingService, IRecruitmentService recruitmentService, ILeaveService leaveService)
        {
            _leadService = leadService;
            _eventService = eventService;
            _leadNewService = leadNewService;
            _bookingService = bookingService;
            _recruitmentService = recruitmentService;
            _leaveService = leaveService;
        }

        [DisplayName(@"Enquire KPIs")]
        public ActionResult Index()
        {
            var model = new KPIViewModel();
            return View(model);
        }

        [DisplayName(@"Update KPI")]
        public ActionResult Update(int id = 0)
        {
            var model = new Lead();
            if (id > 0)
            {
                model = _leadService.GetLead(id);
            }
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Update KPI")]
        public ActionResult Update(int ID, string KPIRemarks, bool MarkKPI = false)
        {
            var lead = _leadService.GetLead(ID);
            if (lead == null)
                return Json(new
                {
                    Code = 3,
                    Message = "Call not found"
                });
            if (ModelState.IsValid)
            {
                lead.MarkKPI = MarkKPI;
                lead.KPIRemarks = KPIRemarks;
                if (_leadService.UpdateLead(lead))
                {
                    return Json(new
                    {
                        Code = 1,
                        lead.ID,
                        MarkKPI,
                        KPIRemarks,
                        lead.ClassKPIStatus
                    });
                }
                return Json(new
                {
                    Code = 2
                });
            }
            return Json(new
            {
                Code = 3,
                Message = "Model State is invalid"
            });
        }
        [DisplayName(@"Consolidate KPIs")]
        public ActionResult Consolidate()
        {
            var model = new KPIViewModel();
            return View(model);
        }

        [DisplayName(@"HR KPIs")]
        public ActionResult HRConsolidate()
        {
            var model = new KPIViewModel();
            return View(model);
        }

        [DisplayName(@"Print Consolidate KPIs")]
        [HttpPost]
        public ActionResult PrintConsolidate(KPIViewModel model)
        {
            return RedirectToAction("PublishToPdfWithURL", "ExportToPDF",
                new
                {
                    path =
                    Url.Action("PrintConsolidateKpis", "Report") + "?eventId=" + model.EventID +
                    "&userId=" + model.UserID +
                    "&dfrom=" + (model.DateFrom != default(DateTime) ? model.DateFrom.ToString("dd/MM/yyyy") : "") +
                    "&dto=" + (model.DateTo != default(DateTime) ? model.DateTo.ToString("dd/MM/yyyy") : ""),
                    name = "ConsolidateKPIs"
                }
            );
        }
        [DisplayName(@"Print HR KPIs")]
        [HttpPost]
        public ActionResult PrintHRConsolidate(KPIViewModel model)
        {
            return RedirectToAction("PublishToPdfWithURL", "ExportToPDF",
                new
                {
                    path =
                    Url.Action("PrintHRKpis", "Report") + "?" +
                    "userId=" + model.UserID +
                    "&dfrom=" + (model.DateFrom != default(DateTime) ? model.DateFrom.ToString("dd/MM/yyyy") : "") +
                    "&dto=" + (model.DateTo != default(DateTime) ? model.DateTo.ToString("dd/MM/yyyy") : ""),
                    name = "HRKPIs"
                }
            );
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
                        (m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                         m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                         m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value)).OrderByDescending(m => m.CreatedTime).AsEnumerable();
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        leads = leads.Where(m =>
                                m.CompanyName.ToLower().Contains(searchValue) ||
                                m.DirectLine.ToLower().Contains(searchValue) ||
                                m.JobTitle.ToLower().Contains(searchValue) ||
                                m.FirstName.ToLower().Contains(searchValue) ||
                                m.LastName.ToLower().Contains(searchValue) ||
                                (m.MobilePhone1 != null && m.MobilePhone1.ToLower().Contains(searchValue)) ||
                                (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                                (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                                (m.KPIRemarks != null && m.KPIRemarks.ToLower().Contains(searchValue))
                            );
                    }
                    leads = model.MarkKPI(leads);
                    if (sortColumnDir == "asc")
                    {
                        switch (sortColumn)
                        {
                            case "CompanyName":
                                leads = leads.OrderBy(s => s.CompanyName).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "DirectLine":
                                leads = leads.OrderBy(s => s.DirectLine).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "JobTitle":
                                leads = leads.OrderBy(s => s.JobTitle).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "FirstName":
                                leads = leads.OrderBy(s => s.FirstName).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "LastName":
                                leads = leads.OrderBy(s => s.LastName).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "MobilePhone1":
                                leads = leads.OrderBy(s => s.MobilePhone1).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "PersonalEmail":
                                leads = leads.OrderBy(s => s.PersonalEmail).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "WorkEmail":
                                leads = leads.OrderBy(s => s.WorkEmail).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "KPIRemarks":
                                leads = leads.OrderBy(s => s.KPIRemarks).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            default:
                                leads = leads.OrderBy(s => s.CreatedTime).AsEnumerable();
                                break;
                        }
                    }
                    else
                    {
                        switch (sortColumn)
                        {
                            case "CompanyName":
                                leads = leads.OrderByDescending(s => s.CompanyName).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "DirectLine":
                                leads = leads.OrderByDescending(s => s.DirectLine).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "JobTitle":
                                leads = leads.OrderByDescending(s => s.JobTitle).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "FirstName":
                                leads = leads.OrderByDescending(s => s.FirstName).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "LastName":
                                leads = leads.OrderByDescending(s => s.LastName).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "MobilePhone1":
                                leads = leads.OrderByDescending(s => s.MobilePhone1).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "PersonalEmail":
                                leads = leads.OrderByDescending(s => s.PersonalEmail).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "WorkEmail":
                                leads = leads.OrderByDescending(s => s.WorkEmail).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            case "KPIRemarks":
                                leads = leads.OrderByDescending(s => s.KPIRemarks).ThenBy(s => s.CreatedTime).AsEnumerable();
                                break;
                            default:
                                leads = leads.OrderByDescending(s => s.CreatedTime).AsEnumerable();
                                break;
                        }
                    }

                    recordsTotal = leads.Count();
                    if (pageSize > recordsTotal)
                    {
                        pageSize = recordsTotal;
                    }

                    leads = leads.Skip(skip).Take(pageSize).ToList();
                    var json = new
                    {
                        draw = draw,
                        recordsFiltered = recordsTotal,
                        recordsTotal = recordsTotal,
                        data = leads.Select(m => new
                        {
                            m.ID,
                            CreatedTime = m.CreatedTime.ToString("dd/MM/yyyy HH:mm"),
                            UserName = m.User != null ? m.User.DisplayName : "",
                            m.CompanyName,
                            DirectLine = "(" + m.DialingCode + ")" + m.DirectLine,
                            m.JobTitle,
                            m.Salutation,
                            m.FirstName,
                            m.LastName,
                            m.MobilePhone1,
                            m.MobilePhone2,
                            m.MobilePhone3,
                            m.PersonalEmail,
                            m.WorkEmail,
                            m.EstimatedDelegateNumber,
                            m.TrainingBudgetPerHead,
                            m.GoodTrainingMonth,
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
                    UserName = m.User != null ? m.User.DisplayName : "",
                    m.CompanyName,
                    m.DirectLine,
                    m.JobTitle,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.MobilePhone1,
                    m.MobilePhone2,
                    m.MobilePhone3,
                    m.PersonalEmail,
                    m.WorkEmail,
                    m.EstimatedDelegateNumber,
                    m.TrainingBudgetPerHead,
                    m.GoodTrainingMonth,
                    m.MarkKPI,
                    m.KPIRemarks,
                })
            };
            return Json(json1, JsonRequestBehavior.AllowGet);
        }
        [AjaxOnly]
        [HttpPost]
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
            var salesUser = PermissionHelper.SalesmanId();
            if (!string.IsNullOrEmpty(searchValue))
            {
                if (salesUser > 0) //only salesman
                {
                    var currentUser = CurrentUser.Identity;
                    if (currentUser != null && currentUser.SalesManagementUnit != SalesManagementUnit.None)
                    {

                        leads = _leadService.GetAllLeads(m =>
                            (m.UserID == currentUser.ID ||
                            m.User.DirectSupervisorID == currentUser.ID) &&
                            (m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                             m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                             m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value) &&
                            (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                            (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                            (eventId == 0 || m.EventID == eventId) &&
                            (userId == 0 || m.UserID == userId ||
                             (m.User != null && m.User.TransferUserID == userId)) &&
                            (string.IsNullOrEmpty(statusCode) || (statusCode == KPIStatus.KPI && m.MarkKPI)
                             || (statusCode == KPIStatus.NoKPI && !m.MarkKPI &&
                                 !string.IsNullOrEmpty(m.FileNameImportKPI))
                             || (statusCode == KPIStatus.NoCheck && string.IsNullOrEmpty(m.FileNameImportKPI)))
                            && (
                                m.EventName.Contains(searchValue) ||
                                m.SalesmanName.Contains(searchValue) ||
                                m.StatusUpdateTimeStr.Contains(searchValue) ||
                                m.StatusDisplay.Contains(searchValue) ||
                                m.CompanyName.Contains(searchValue) ||
                                m.CountryCode.Contains(searchValue) ||
                                m.JobTitle.Contains(searchValue) ||
                                m.DirectLine.Contains(searchValue) ||
                                m.CallBackDateDisplay.Contains(searchValue) ||
                                (m.Salutation != null && m.Salutation.Contains(searchValue)) ||
                                (m.FirstName != null && m.FirstName.Contains(searchValue)) ||
                                (m.LastName != null && m.LastName.Contains(searchValue)) ||
                                (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                                (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                                (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                                (m.WorkEmail != null && m.WorkEmail.Contains(searchValue)) ||
                                (m.WorkEmail1 != null && m.WorkEmail1.Contains(searchValue)) ||
                                (m.PersonalEmail != null && m.PersonalEmail.Contains(searchValue)))
                        );
                    }
                    else if (currentUser != null && currentUser.BusinessDevelopmentUnit != BusinessDevelopmentUnit.None)
                    {
                        leads = _leadService.GetAllLeads(m =>
                            m.UserID == currentUser.ID &&
                            (m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                             m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                             m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value) &&
                            (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                            (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                            (eventId == 0 || m.EventID == eventId) &&
                            (userId == 0 || m.UserID == userId || (m.User != null && m.User.TransferUserID == userId)) &&
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
                                m.JobTitle.Contains(searchValue) ||
                                m.DirectLine.Contains(searchValue) ||
                                m.CallBackDateDisplay.Contains(searchValue) ||
                                (m.Salutation != null && m.Salutation.Contains(searchValue)) ||
                                (m.FirstName != null && m.FirstName.Contains(searchValue)) ||
                                (m.LastName != null && m.LastName.Contains(searchValue)) ||
                                (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                                (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                                (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                                (m.WorkEmail != null && m.WorkEmail.Contains(searchValue)) ||
                                (m.WorkEmail1 != null && m.WorkEmail1.Contains(searchValue)) ||
                                (m.PersonalEmail != null && m.PersonalEmail.Contains(searchValue)))
                        );
                    }
                }
                else //for manager
                {
                    leads = _leadService.GetAllLeads(m =>
                        (m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                         m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                         m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value) &&
                        (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                        (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                        (eventId == 0 || m.EventID == eventId) &&
                        (userId == 0 || m.UserID == userId || (m.User != null && m.User.TransferUserID == userId)) &&
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
                            m.JobTitle.Contains(searchValue) ||
                            m.DirectLine.Contains(searchValue) ||
                            m.CallBackDateDisplay.Contains(searchValue) ||
                            (m.Salutation != null && m.Salutation.Contains(searchValue)) ||
                            (m.FirstName != null && m.FirstName.Contains(searchValue)) ||
                            (m.LastName != null && m.LastName.Contains(searchValue)) ||
                            (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                            (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                            (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                            (m.WorkEmail != null && m.WorkEmail.Contains(searchValue)) ||
                            (m.WorkEmail1 != null && m.WorkEmail1.Contains(searchValue)) ||
                            (m.PersonalEmail != null && m.PersonalEmail.Contains(searchValue)))
                    );
                }
            }
            else
            {
                if (salesUser > 0)//only salesman
                {
                    var currentUser = CurrentUser.Identity;
                    if (currentUser != null && currentUser.SalesManagementUnit != SalesManagementUnit.None)
                    {
                        leads = _leadService.GetAllLeads(m =>
                            (m.UserID == currentUser.ID ||
                            m.User.DirectSupervisorID == currentUser.ID) &&
                            (m.LeadStatusRecord != LeadStatus.Reject &&
                             m.LeadStatusRecord != LeadStatus.Initial &&
                             m.LeadStatusRecord != LeadStatus.Deleted) &&
                            (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                            (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                            (eventId == 0 || m.EventID == eventId) &&
                            (userId == 0 || m.UserID == userId || (m.User != null && m.User.TransferUserID == userId)) &&
                            (string.IsNullOrEmpty(statusCode) || (statusCode == KPIStatus.KPI && m.MarkKPI)
                             || (statusCode == KPIStatus.NoKPI && !m.MarkKPI && !string.IsNullOrEmpty(m.FileNameImportKPI))
                             || (statusCode == KPIStatus.NoCheck && string.IsNullOrEmpty(m.FileNameImportKPI)))
                        );
                    }
                    else if (currentUser != null && currentUser.BusinessDevelopmentUnit != BusinessDevelopmentUnit.None)
                    {
                        leads = _leadService.GetAllLeads(m =>
                            m.UserID == currentUser.ID &&
                            (m.LeadStatusRecord != LeadStatus.Reject &&
                             m.LeadStatusRecord != LeadStatus.Initial &&
                             m.LeadStatusRecord != LeadStatus.Deleted) &&
                            (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                            (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                            (eventId == 0 || m.EventID == eventId) &&
                            (userId == 0 || m.UserID == userId ||
                             (m.User != null && m.User.TransferUserID == userId)) &&
                            (string.IsNullOrEmpty(statusCode) || (statusCode == KPIStatus.KPI && m.MarkKPI)
                             || (statusCode == KPIStatus.NoKPI && !m.MarkKPI &&
                                 !string.IsNullOrEmpty(m.FileNameImportKPI))
                             || (statusCode == KPIStatus.NoCheck && string.IsNullOrEmpty(m.FileNameImportKPI)))
                        );
                    }
                }
                else//for manager
                {
                    leads = _leadService.GetAllLeads(m =>
                        (m.LeadStatusRecord != LeadStatus.Reject &&
                         m.LeadStatusRecord != LeadStatus.Initial &&
                         m.LeadStatusRecord != LeadStatus.Deleted) &&
                        (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                        (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                        (eventId == 0 || m.EventID == eventId) &&
                        (userId == 0 || m.UserID == userId || (m.User != null && m.User.TransferUserID == userId)) &&
                        (string.IsNullOrEmpty(statusCode) || (statusCode == KPIStatus.KPI && m.MarkKPI)
                         || (statusCode == KPIStatus.NoKPI && !m.MarkKPI && !string.IsNullOrEmpty(m.FileNameImportKPI))
                         || (statusCode == KPIStatus.NoCheck && string.IsNullOrEmpty(m.FileNameImportKPI)))
                    );
                }
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
                    case "JobTitle":
                        leads = leads.OrderBy(s => s.JobTitle).ThenBy(s => s.ID);
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
                    case "JobTitle":
                        leads = leads.OrderByDescending(s => s.JobTitle).ThenBy(s => s.ID);
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
                    m.DirectLine,
                    m.JobTitle,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.MobilePhone1,
                    m.MobilePhone2,
                    m.MobilePhone3,
                    m.PersonalEmail,
                    m.WorkEmail,
                    m.EstimatedDelegateNumber,
                    m.TrainingBudgetPerHead,
                    m.GoodTrainingMonth,
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
            IEnumerable<LeadNew> leadNews = new HashSet<LeadNew>();
            IEnumerable<Booking> bookings = new HashSet<Booking>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                leads = _leadService.GetAllLeadsForKPI(eventId, userId, m =>
                     datefrom <= m.CreatedTime.Date &&
                     m.CreatedTime.Date <= dateto &&
                     m.SalesmanName.Contains(searchValue)
                );
                leadNews = _leadNewService.GetAllLeadNewsForKPI(eventId, userId, m =>
                     m.AssignUserID > 0 &&
                     datefrom <= m.FirstAssignDate.Date &&
                     m.FirstAssignDate.Date <= dateto &&
                      m.SalesmanName.Contains(searchValue)
                );
                bookings = _bookingService.GetAllBookingsForKPI(eventId, userId, m =>
                    m.BookingStatusRecord.Status.Value == BookingStatus.Approved.Value &&
                    datefrom <= m.BookingDate.Date &&
                   m.BookingDate.Date <= dateto &&
                     m.SalesmanName.Contains(searchValue)
                );
            }
            else
            {
                leads = _leadService.GetAllLeadsForKPI(eventId, userId, m =>
                    datefrom <= m.CreatedTime.Date &&
                    m.CreatedTime.Date <= dateto
                );
                leadNews = _leadNewService.GetAllLeadNewsForKPI(eventId, userId, m =>
                    m.AssignUserID > 0 &&
                    datefrom <= m.FirstAssignDate.Date &&
                    m.FirstAssignDate.Date <= dateto
                );
                bookings = _bookingService.GetAllBookingsForKPI(eventId, userId, m =>
                    m.BookingStatusRecord.Status.Value == BookingStatus.Approved.Value &&
                    datefrom <= m.BookingDate.Date &&
                    m.BookingDate.Date <= dateto
                );
            }
            // ReSharper disable once AssignNullToNotNullAttribute
            var model = new ConsolidateKPIModel { DateFrom = datefrom, DateTo = dateto };
            model.Prepare(leads, leadNews, bookings);

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "NewEventRequest":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.NewEventRequest).ThenBy(s => s.User.ID).ToList();
                        break;
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
                    case "WrittenRevenue":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.WrittenRevenue).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "ActualCallKpis":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.ActualCallKpis).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "ActualRequiredCallKpis":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.ActualRequiredCallKpis).ThenBy(s => s.User.ID).ToList();
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
                    case "NewEventRequest":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.NewEventRequest).ThenBy(s => s.User.ID).ToList();
                        break;
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
                    case "WrittenRevenue":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.WrittenRevenue).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "ActualCallKpis":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.ActualCallKpis).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "ActualRequiredCallKpis":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.ActualRequiredCallKpis).ThenBy(s => s.User.ID).ToList();
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
                    m.NewEventRequest,
                    m.KPI,
                    m.NoKPI,
                    m.ActualCallKpis,
                    m.ActualRequiredCallKpis,
                    WrittenRevenue = m.WrittenRevenue.ToString("N0"),
                    m.NoCheck,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult AjaxGetTopSalesConsolidateKpis()
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
            IEnumerable<Booking> bookings = new HashSet<Booking>();
            var dateMonth = DateTime.Today.AddMonths(-2);
            var month = new DateTime(dateMonth.Year, dateMonth.Month, 1);
            if (!string.IsNullOrEmpty(searchValue))
            {
                bookings = _bookingService.GetAllBookingsForKPI(0,0, m =>
                    m.BookingStatusRecord.Status.Value == BookingStatus.Approved.Value &&
                    m.CreatedTime >= month &&
                     m.SalesmanName.Contains(searchValue)
                );
            }
            else
            {
                bookings = _bookingService.GetAllBookingsForKPI(0, 0, m =>
                    m.BookingStatusRecord.Status.Value == BookingStatus.Approved.Value &&
                    m.CreatedTime >= month
                );
            }
            // ReSharper disable once AssignNullToNotNullAttribute
            var model = new ConsolidateKPIModel();
            model.Prepare(bookings);

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "Email":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.User.Email).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "WrittenRevenue1":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.WrittenRevenue1).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "WrittenRevenue2":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.WrittenRevenue2).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "WrittenRevenue3":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.WrittenRevenue3).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "TotalWrittenRevenue":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderBy(s => s.TotalWrittenRevenue).ThenBy(s => s.User.ID).ToList();
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
                    case "Email":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.User.Email).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "WrittenRevenue1":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.WrittenRevenue1).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "WrittenRevenue2":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.WrittenRevenue2).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "WrittenRevenue3":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.WrittenRevenue3).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "TotalWrittenRevenue":
                        model.ConsolidateKpis = model.ConsolidateKpis.OrderByDescending(s => s.TotalWrittenRevenue).ThenBy(s => s.User.ID).ToList();
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
                    WrittenRevenue1 = m.WrittenRevenue1.ToString("N0"),
                    WrittenRevenue2 = m.WrittenRevenue2.ToString("N0"),
                    WrittenRevenue3 = m.WrittenRevenue3.ToString("N0"),
                    TotalWrittenRevenue = m.TotalWrittenRevenue.ToString("N0"),
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult AjaxGetHRKpis()
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
            IEnumerable<Candidate> candidates = new HashSet<Candidate>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                candidates = _recruitmentService.GetAllCandidates(m =>
                    (m.CandidateStatusRecord != CandidateStatus.Deleted) &&
                    (datefrom == default(DateTime) || datefrom.Date <= m.CreatedTime.Date) &&
                    (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                    (userId == 0 || m.UserID == userId || (m.User != null && m.User.TransferUserID == userId)) &&
                    ((m.User != null && m.User.Email.Contains(searchValue)) ||
                     m.SalesmanName.Contains(searchValue))
                );
            }
            else
            {
                candidates = _recruitmentService.GetAllCandidates(m =>
                    (m.CandidateStatusRecord != CandidateStatus.Deleted) &&
                    (datefrom == default(DateTime) || datefrom.Date <= m.CreatedTime.Date) &&
                    (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                    (userId == 0 || m.UserID == userId ||
                    (m.User != null && m.User.TransferUserID == userId))
                );
            }
            // ReSharper disable once AssignNullToNotNullAttribute
            var model = new HRConsolidateKPIModel();
            model.Prepare(candidates);

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "EmployeeKPIs":
                        model.HrConsolidateKpis = model.HrConsolidateKpis.OrderBy(s => s.EmployeeKPIs).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "RecruitmentCallKPIs":
                        model.HrConsolidateKpis = model.HrConsolidateKpis.OrderBy(s => s.RecruitmentCallKPIs).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "Email":
                        model.HrConsolidateKpis = model.HrConsolidateKpis.OrderBy(s => s.User.Email).ThenBy(s => s.User.ID).ToList();
                        break;
                    default:
                        model.HrConsolidateKpis = model.HrConsolidateKpis.OrderBy(s => s.User.DisplayName).ToList();
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "EmployeeKPIs":
                        model.HrConsolidateKpis = model.HrConsolidateKpis.OrderByDescending(s => s.EmployeeKPIs).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "RecruitmentCallKPIs":
                        model.HrConsolidateKpis = model.HrConsolidateKpis.OrderByDescending(s => s.RecruitmentCallKPIs).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "Email":
                        model.HrConsolidateKpis = model.HrConsolidateKpis.OrderByDescending(s => s.User.Email).ThenBy(s => s.User.ID).ToList();
                        break;
                    default:
                        model.HrConsolidateKpis = model.HrConsolidateKpis.OrderByDescending(s => s.User.DisplayName).ToList();
                        break;
                }
            }

            #endregion sort

            recordsTotal = model.HrConsolidateKpis.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = model.HrConsolidateKpis.Skip(skip).Take(pageSize).ToList();

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
                    m.EmployeeKPIs,
                    m.RecruitmentCallKPIs
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(LeadModel model)
        {
            return Json(model.DeleteLead());
        }
    }
}
