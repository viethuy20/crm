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

        public KpiController(ILeadService leadService, IEventService eventService, ILeadNewService leadNewService, IBookingService bookingService, IRecruitmentService recruitmentService)
        {
            _leadService = leadService;
            _eventService = eventService;
            _leadNewService = leadNewService;
            _bookingService = bookingService;
            _recruitmentService = recruitmentService;
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
                    recordsTotal = _leadService.GetCountLeadsForKpiReview(model.EventID,  searchValue);
                    var leads = _leadService.GetAllLeadsForKpiReview(model.EventID, searchValue, sortColumnDir, sortColumn, skip, pageSize);
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
            var salesUser = PermissionHelper.SalesmanId();
            var currentUser = CurrentUser.Identity;
            int recordsTotal = _leadService.GetCountEnquireLeadsForKpi(salesUser > 0, currentUser, eventId, userId, statusCode, datefrom, dateto, searchValue);
            var data = _leadService.GetAllEnquireLeadsForKpi(salesUser > 0, currentUser, eventId, userId, statusCode, datefrom, dateto, searchValue, sortColumnDir, sortColumn, skip, pageSize);
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
            leads = _leadService.GetAllLeadsForKPI(eventId, userId, datefrom, dateto, searchValue).AsEnumerable();
            leadNews = _leadNewService.GetAllLeadNewsForKPI(eventId, userId, datefrom, dateto, searchValue).AsEnumerable();
            bookings = _bookingService.GetAllBookingsForKPI(eventId, userId, datefrom, dateto, searchValue).AsEnumerable();
            // ReSharper disable once AssignNullToNotNullAttribute
            var model = new ConsolidateKPIModel { DateFrom = datefrom, DateTo = dateto };
            model.Prepare(leads, leadNews);

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
            model.ConsolidateKpis = model.ConsolidateKpis.Skip(skip).Take(pageSize).ToList();
            //Calc
            model.PrepareCalc(leads, leadNews, bookings);
            var data = model.ConsolidateKpis;
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
            bookings = _bookingService.GetAllBookingsForTopSalesKPI(month, searchValue).AsEnumerable();
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
            candidates = _recruitmentService.GetAllCandidatesForKpis(searchValue, userId, datefrom, dateto).AsEnumerable();
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
