using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using NS.Helpers;
using NS.Mvc.ActionResults;
using PQT.Domain;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Infrastructure.Utility;
using PQT.Web.Models;
using Resources;

namespace PQT.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ICompanyRepository _companyRepo;
        private readonly ILeadService _leadService;
        private readonly IEventService _eventService;
        private readonly IMembershipService _membershipService;

        public BookingController(IBookingService bookingService, ICompanyRepository companyRepo, ILeadService leadService
            , IEventService eventService, IMembershipService membershipService)
        {
            _bookingService = bookingService;
            _companyRepo = companyRepo;
            _leadService = leadService;
            _eventService = eventService;
            _membershipService = membershipService;
        }

        [DisplayName(@"Booking management")]
        public ActionResult Index(int id = 0)
        {
            var eventData = _eventService.GetEvent(id);
            //if (eventData == null)
            //{
            //TempData["error"] = "Event not found";
            //return RedirectToAction("Index", "Home");
            //}
            return View(eventData);
        }

        [DisplayName(@"Bookings approved")]
        public ActionResult BookingsApproved()
        {
            return View(new BookingViewModel());
        }

        public ActionResult Detail(int id = 0, int leadId = 0, int eventId = 0, int approvedList = 0)
        {
            var model = new BookingModel();
            Booking booking = null;
            if (id > 0)
            {
                booking = _bookingService.GetBooking(id);
            }
            else if (leadId > 0)
            {
                booking = _bookingService.GetBookingByLeadId(leadId);
            }
            if (booking == null)
            {
                TempData["error"] = "Booking not found";
                return RedirectToAction("Index", new { id = eventId });
            }
            model.Prepare(booking.LeadID);
            model.Booking = booking;
            return View(model);
        }

        [DisplayName(@"Detail By Lead")]
        public ActionResult DetailByLead(int id)
        {
            var booking = _bookingService.GetBookingByLeadId(id);
            if (booking == null)
            {
                TempData["error"] = "Booking not found";
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Detail", new { id = booking.ID });
        }
        public ActionResult Create(int id = 0)
        {
            if (id == 0)
            {
                TempData["error"] = "This call not found";
                return RedirectToAction("Index", "Lead");
            }
            var booking = _bookingService.GetBookingByLeadId(id);
            if (booking != null)
            {
                return RedirectToAction("Edit", new { id = booking.ID });
            }
            var model = new BookingModel();
            model.Prepare(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(BookingModel model)
        {
            var lead = _leadService.GetLead(model.Booking.LeadID);
            if (lead == null)
            {
                TempData["error"] = "This call not found";
                return RedirectToAction("Index", "Lead");
            }
            if (model.SessionIds == null || !model.SessionIds.Any())
            {
                ModelState.AddModelError("Event.EventSessions", @"Please choose event session");
            }
            if (ModelState.IsValid)
            {
                var flag = model.Booking.ID > 0 ? model.Update() : (model.Create() != null);
                if (flag)
                {
                    var leadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.RequestBook, CurrentUser.Identity.ID);
                    lead.LeadStatusRecord = leadStatusRecord;
                    _leadService.UpdateLead(lead);
                    var titleNotify = "Request for booking";
                    BookingNotificator.NotifyUser(NotifyAction.Request, model.Booking.ID, titleNotify, true);
                    BookingNotificator.NotifyUpdateBooking(model.Booking.ID);
                    TempData["message"] = Resource.SaveSuccessful;
                    return RedirectToAction("Index", "Lead", new { id = model.Booking.EventID });
                }
                TempData["error"] = Resource.SaveError;
            }
            model.Event = _eventService.GetEvent(lead.EventID);
            model.Booking.Company = lead.Company;
            //model.Companies = _companyRepo.GetAllCompanies();
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = new BookingModel();
            model.PrepareEdit(id);
            if (model.Booking == null)
            {
                return RedirectToAction("Index");
            }
            var userId = CurrentUser.Identity.ID;
            if (!(CurrentUser.HasRole("Finance") || CurrentUser.HasRole("Admin") || CurrentUser.HasRole("QC") || CurrentUser.HasRole("Manager")) &&
                model.Booking.SalesmanID != userId && model.Booking.Salesman.TransferUserID != userId)
            {
                TempData["error"] = "Don't have permission";
                return RedirectToAction("Index", "Lead", new { id = model.Booking.EventID });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(BookingModel model)
        {
            var lead = _leadService.GetLead(model.Booking.LeadID);
            if (lead == null)
            {
                TempData["error"] = "This call not found";
                return RedirectToAction("Index", "Lead");
            }
            if (model.SessionIds == null || !model.SessionIds.Any())
            {
                ModelState.AddModelError("Event.EventSessions", @"Please choose event session");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var flag = model.Booking.ID > 0 ? model.Update() : (model.Create() != null);
                    if (flag)
                    {
                        TempData["message"] = Resource.SaveSuccessful;
                        if (!CurrentUser.HasRoleLevel(RoleLevel.ManagerLevel))
                        {
                            var leadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.RequestBook, CurrentUser.Identity.ID);
                            lead.LeadStatusRecord = leadStatusRecord;
                            _leadService.UpdateLead(lead);
                            var titleNotify = "Request for booking";
                            BookingNotificator.NotifyUser(NotifyAction.Request, model.Booking.ID, titleNotify, true);
                            BookingNotificator.NotifyUpdateBooking(model.Booking.ID);
                            return RedirectToAction("Index", "Lead", new { id = model.Booking.EventID });
                        }
                        //no need nofify when manager edit and redirect to booking management
                        return RedirectToAction("Index");
                    }
                    TempData["error"] = Resource.SaveError;
                }
                catch (Exception e)
                {
                    TempData["error"] = e.Message;
                }
            }
            model.Event = _eventService.GetEvent(lead.EventID);
            model.Booking.Company = lead.Company;
            return View(model);
        }

        [DisplayName(@"Approve booking")]
        public ActionResult ApproveBooking(int id)
        {
            var model = new BookingModel();
            try
            {
                if (model.Approve(id))
                {
                    TempData["message"] = Resource.SaveSuccessful;
                    if (CurrentUser.HasRole("Finance"))
                    {
                        return RedirectToAction("Index", new { id = model.Booking?.EventID ?? 0 });
                    }
                    else
                    {
                        return RedirectToAction("NCLForManager", "Lead", new { id = model.Booking?.EventID ?? 0 });
                    }
                }
                TempData["error"] = Resource.SaveError;
            }
            catch (Exception e)
            {
                TempData["error"] = e.Message;
            }
            return RedirectToAction("Index", new { id = model.Booking?.EventID ?? 0 });
        }
        [DisplayName(@"Reject booking")]
        public ActionResult RejectBooking(int id)
        {
            var model = new BookingModel();
            model.BookingID = id;
            model.Booking = _bookingService.GetBooking(id);
            model.EventID = model.Booking.EventID;
            return PartialView(model);
        }
        [DisplayName(@"Reject booking")]
        [HttpPost]
        public ActionResult RejectBooking(BookingModel model)
        {
            if (string.IsNullOrEmpty(model.Message))
            {
                TempData["error"] = "`Reason` must not be empty";
                return RedirectToAction("Index", new { id = model.EventID });
            }
            try
            {
                if (model.Reject())
                {
                    TempData["message"] = "The booking was declined.";
                    if (CurrentUser.HasRole("Finance"))
                    {
                        return RedirectToAction("Index", new { id = model.Booking?.EventID ?? 0 });
                    }
                    else
                    {
                        return RedirectToAction("NCLForManager", "Lead", new { id = model.Booking?.EventID ?? 0 });
                    }
                }
                TempData["error"] = Resource.SaveError;
            }
            catch (Exception e)
            {
                TempData["error"] = e.Message;
            }
            return RedirectToAction("Index", new { id = model.Booking?.EventID ?? 0 });
        }

        public ActionResult Action(int id)
        {
            var model = new BookingModel();
            var booking = _bookingService.GetBooking(id);
            model.Prepare(booking.LeadID);
            model.Booking = booking;
            return PartialView(model);
        }

        [DisplayName(@"Edit Delegate")]
        public ActionResult EditDelegate(int id)
        {
            var delega = _bookingService.GetDelegate(id);
            return PartialView(delega);
        }

        [DisplayName(@"Edit Delegate")]
        [HttpPost]
        public ActionResult EditDelegate(Domain.Entities.Delegate model)
        {
            var delega = _bookingService.GetDelegate(model.ID);
            if (delega == null)
                return Json(new { IsSuccess = false, Message = "Delegate not found" });
            delega.OverallFeedbacks = model.OverallFeedbacks;
            delega.OpTopicsInterested = model.OpTopicsInterested;
            delega.OpLocationsInterested = model.OpLocationsInterested;
            delega.OpGoodTrainingMonth = model.OpGoodTrainingMonth;
            delega.AttendanceStatus = model.AttendanceStatus;
            if (_bookingService.UpdateDelegate(delega))
            {
                return Json(new
                {
                    IsSuccess = true,
                    Data = new
                    {
                        delega.ID,
                        delega.OverallFeedbacks,
                        delega.OpTopicsInterested,
                        delega.OpLocationsInterested,
                        delega.OpGoodTrainingMonth,
                        delega.AttendanceStatusDisplay,
                    }
                });
            }
            return Json(new { IsSuccess = false, Message = "Update failed" });
        }

        [DisplayName(@"Edit Company")]
        public ActionResult EditCompany(int id)
        {
            var delega = _bookingService.GetAllBookings(m => m.CompanyID == id).FirstOrDefault();
            return PartialView(delega);
        }

        [DisplayName(@"Edit Company")]
        [HttpPost]
        public ActionResult EditCompany(int CompanyID, PaymentStatus PaymentStatus, AttendanceStatus AttendanceStatus)
        {
            var coms = _bookingService.GetAllBookings(m => m.CompanyID == CompanyID);
            if (!coms.Any())
                return Json(new { IsSuccess = false, Message = "Company not found" });
            foreach (var booking in coms)
            {
                booking.PaymentStatus = PaymentStatus;
                booking.AttendanceStatus = AttendanceStatus;
                _bookingService.UpdateBooking(booking);
            }
            return Json(new
            {
                IsSuccess = true,
                Data = new
                {
                    ID = CompanyID,
                    PaymentStatus = PaymentStatus.DisplayName,
                    AttendanceStatus = AttendanceStatus.DisplayName,
                }
            });
        }
        [AjaxOnly]
        public ActionResult AjaxGetBookings(int eventId)
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
            IEnumerable<Booking> bookings = new HashSet<Booking>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                bookings = _bookingService.GetAllBookings(
                    m => (m.BookingStatusRecord.Status == BookingStatus.Approved ||
                          m.BookingStatusRecord.Status == BookingStatus.Initial) &&
                         (saleId == 0 || m.SalesmanID == saleId) &&
                         (eventId == 0 || m.EventID == eventId) && (
                             m.CreatedTime.ToString("dd/MM/yyyy HH:mm:ss")
                                 .Contains(searchValue) ||
                             (m.Company != null && m.Company.CompanyName.ToLower()
                                  .Contains(searchValue)) ||
                             (m.Address != null && m.Address.ToLower()
                                  .Contains(searchValue)) ||
                             (m.Tel != null &&
                              m.Tel.ToLower().Contains(searchValue)) ||
                             (m.Fax != null &&
                              m.Fax.ToLower().Contains(searchValue)) ||
                             (m.AuthoriserName != null &&
                              m.AuthoriserName.ToLower().Contains(searchValue)) ||
                             (m.SenderName != null && m.SenderName.ToLower()
                                  .Contains(searchValue)) ||
                             (m.Event != null && m.Event.EventName.ToLower()
                                  .Contains(searchValue)) ||
                             (m.Event != null && m.Event.EventCode.ToLower()
                                  .Contains(searchValue)) ||
                             (m.Salesman != null && m.Salesman.DisplayName
                                  .ToLower().Contains(searchValue)) ||
                             m.FeePerDelegate.ToString().Contains(searchValue) ||
                             m.DiscountPercent.ToString().Contains(searchValue) ||
                             m.TotalWrittenRevenue.ToString()
                                 .Contains(searchValue) ||
                             m.TotalPaidRevenue.ToString()
                                 .Contains(searchValue) ||
                             (m.BookingStatusRecord != null &&
                              m.BookingStatusRecord.Status.DisplayName.ToLower()
                                  .Contains(searchValue))));
            }
            else
            {
                bookings = _bookingService.GetAllBookings(m =>
                    (m.BookingStatusRecord.Status == BookingStatus.Approved ||
                     m.BookingStatusRecord.Status == BookingStatus.Initial) &&
                    (saleId == 0 || m.SalesmanID == saleId) &&
                    (eventId == 0 || m.EventID == eventId));
            }
            // ReSharper disable once AssignNullToNotNullAttribute

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        bookings = bookings.OrderBy(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        bookings = bookings.OrderBy(s => s.Company != null ? s.Company.CompanyName : "").ThenBy(s => s.ID);
                        break;
                    case "Address":
                        bookings = bookings.OrderBy(s => s.Address).ThenBy(s => s.ID);
                        break;
                    case "Tel":
                        bookings = bookings.OrderBy(s => s.Tel).ThenBy(s => s.ID);
                        break;
                    case "Fax":
                        bookings = bookings.OrderBy(s => s.Fax).ThenBy(s => s.ID);
                        break;
                    case "Authoriser":
                        bookings = bookings.OrderBy(s => s.AuthoriserName).ThenBy(s => s.ID);
                        break;
                    case "Sender":
                        bookings = bookings.OrderBy(s => s.SenderName).ThenBy(s => s.ID);
                        break;
                    case "Event":
                        bookings = bookings.OrderBy(s => s.Event.EventName).ThenBy(s => s.ID);
                        break;
                    case "EventCode":
                        bookings = bookings.OrderBy(s => s.Event.EventCode).ThenBy(s => s.ID);
                        break;
                    case "Salesman":
                        bookings = bookings.OrderBy(s => s.Salesman != null ? s.Salesman.DisplayName : "").ThenBy(s => s.ID);
                        break;
                    case "FeePerDelegate":
                        bookings = bookings.OrderBy(s => s.FeePerDelegate).ThenBy(s => s.ID);
                        break;
                    case "Discount":
                        bookings = bookings.OrderBy(s => s.DiscountPercent).ThenBy(s => s.ID);
                        break;
                    case "TotalWrittenRevenue":
                        bookings = bookings.OrderBy(s => s.TotalWrittenRevenue).ThenBy(s => s.ID);
                        break;
                    case "TotalPaidRevenue":
                        bookings = bookings.OrderBy(s => s.TotalPaidRevenue).ThenBy(s => s.ID);
                        break;
                    case "Status":
                        bookings = bookings.OrderBy(s => s.BookingStatusRecord != null ? s.BookingStatusRecord.Status : "").ThenBy(s => s.ID);
                        break;
                    default:
                        bookings = bookings.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        bookings = bookings.OrderByDescending(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        bookings = bookings.OrderByDescending(s => s.Company != null ? s.Company.CompanyName : "").ThenBy(s => s.ID);
                        break;
                    case "Address":
                        bookings = bookings.OrderByDescending(s => s.Address).ThenBy(s => s.ID);
                        break;
                    case "Tel":
                        bookings = bookings.OrderByDescending(s => s.Tel).ThenBy(s => s.ID);
                        break;
                    case "Fax":
                        bookings = bookings.OrderByDescending(s => s.Fax).ThenBy(s => s.ID);
                        break;
                    case "Authoriser":
                        bookings = bookings.OrderByDescending(s => s.AuthoriserName).ThenBy(s => s.ID);
                        break;
                    case "Sender":
                        bookings = bookings.OrderByDescending(s => s.SenderName).ThenBy(s => s.ID);
                        break;
                    case "Event":
                        bookings = bookings.OrderByDescending(s => s.Event.EventName).ThenBy(s => s.ID);
                        break;
                    case "EventCode":
                        bookings = bookings.OrderByDescending(s => s.Event.EventCode).ThenBy(s => s.ID);
                        break;
                    case "Salesman":
                        bookings = bookings.OrderByDescending(s => s.Salesman != null ? s.Salesman.DisplayName : "").ThenBy(s => s.ID);
                        break;
                    case "FeePerDelegate":
                        bookings = bookings.OrderByDescending(s => s.FeePerDelegate).ThenBy(s => s.ID);
                        break;
                    case "Discount":
                        bookings = bookings.OrderByDescending(s => s.DiscountPercent).ThenBy(s => s.ID);
                        break;
                    case "TotalWrittenRevenue":
                        bookings = bookings.OrderByDescending(s => s.TotalWrittenRevenue).ThenBy(s => s.ID);
                        break;
                    case "TotalPaidRevenue":
                        bookings = bookings.OrderByDescending(s => s.TotalPaidRevenue).ThenBy(s => s.ID);
                        break;
                    case "Status":
                        bookings = bookings.OrderByDescending(s => s.BookingStatusRecord != null ? s.BookingStatusRecord.Status : "").ThenBy(s => s.ID);
                        break;
                    default:
                        bookings = bookings.OrderByDescending(s => s.ID);
                        break;
                }
            }

            #endregion sort

            recordsTotal = bookings.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = bookings.Skip(skip).Take(pageSize).ToList();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    CreatedTime = m.CreatedTime.ToString("dd/MM/yyyy HH:mm:ss"),
                    Company = m.Company != null ? m.Company.CompanyName : "",
                    m.Address,
                    m.Tel,
                    m.Fax,
                    Authoriser = m.AuthoriserName,
                    Sender = m.SenderName,
                    Event = m.Event != null ? m.Event.EventName : "",
                    EventCode = m.Event != null ? m.Event.EventCode : "",
                    Salesman = m.Salesman != null ? m.Salesman.DisplayName : "",
                    m.FeePerDelegate,
                    Discount = m.DiscountPercent,
                    TotalWrittenRevenue = m.TotalWrittenRevenue.ToString("N2"),
                    TotalPaidRevenue = m.TotalPaidRevenue.ToString("N2"),
                    Status = m.BookingStatusRecord != null ? m.BookingStatusRecord.Status.DisplayName : "",
                    ClassStatus = m.ClassStatus
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult AjaxGetBookingsApproved()
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


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var saleId = PermissionHelper.SalesmanId();
            IEnumerable<Booking> bookings = new HashSet<Booking>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                bookings = _bookingService.GetAllBookings(m => m.BookingStatusRecord == BookingStatus.Approved &&
                                                               (saleId == 0 || m.SalesmanID == saleId) &&
                                                               (datefrom == default(DateTime) ||
                                                                m.CreatedTime.Date >= datefrom.Date) &&
                                                               (dateto == default(DateTime) ||
                                                                m.CreatedTime.Date <= dateto.Date) &&
                                                               (eventId == 0 || m.EventID == eventId) && (
                                                                   (m.CreatedTime.ToString("dd/MM/yyyy")
                                                                        .Contains(searchValue) ||
                                                                    m.Company != null && m.Company.CompanyName.ToLower()
                                                                        .Contains(searchValue)) ||
                                                                   (m.Event != null && m.Event.EventName.ToLower()
                                                                        .Contains(searchValue)) ||
                                                                   (m.Event != null && m.Event.EventCode.ToLower()
                                                                        .Contains(searchValue)) ||
                                                                   m.TotalWrittenRevenue.ToString()
                                                                       .Contains(searchValue) ||
                                                                   m.TotalPaidRevenue.ToString()
                                                                       .Contains(searchValue) ||
                                                                   (m.InvoiceNo != null && m.InvoiceNo
                                                                        .ToLower().Contains(searchValue)) ||
                                                                   (m.PaymentStatus != null && m.PaymentStatusDisplay
                                                                        .ToLower().Contains(searchValue))));
            }
            else
            {
                bookings = _bookingService.GetAllBookings(m => m.BookingStatusRecord == BookingStatus.Approved &&
                                                               (saleId == 0 || m.SalesmanID == saleId) &&
                                                               (datefrom == default(DateTime) ||
                                                                m.CreatedTime.Date >= datefrom.Date) &&
                                                               (dateto == default(DateTime) ||
                                                                m.CreatedTime.Date <= dateto.Date) &&
                                                               (eventId == 0 || m.EventID == eventId));
            }
            // ReSharper disable once AssignNullToNotNullAttribute

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        bookings = bookings.OrderBy(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        bookings = bookings.OrderBy(s => s.Company != null ? s.Company.CountryName : "").ThenBy(s => s.ID);
                        break;
                    case "CompanyName":
                        bookings = bookings.OrderBy(s => s.Company != null ? s.Company.CompanyName : "").ThenBy(s => s.ID);
                        break;
                    case "EventName":
                        bookings = bookings.OrderBy(s => s.Event != null ? s.Event.EventName : "").ThenBy(s => s.ID);
                        break;
                    case "EventCode":
                        bookings = bookings.OrderBy(s => s.Event != null ? s.Event.EventCode : "").ThenBy(s => s.ID);
                        break;
                    case "TotalWrittenRevenue":
                        bookings = bookings.OrderBy(s => s.TotalWrittenRevenue).ThenBy(s => s.ID);
                        break;
                    case "TotalPaidRevenue":
                        bookings = bookings.OrderBy(s => s.TotalPaidRevenue).ThenBy(s => s.ID);
                        break;
                    case "PaymentStatus":
                        bookings = bookings.OrderBy(s => s.PaymentStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "InvoiceNo":
                        bookings = bookings.OrderBy(s => s.InvoiceNo).ThenBy(s => s.ID);
                        break;
                    default:
                        bookings = bookings.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        bookings = bookings.OrderByDescending(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        bookings = bookings.OrderByDescending(s => s.Company != null ? s.Company.CountryName : "").ThenBy(s => s.ID);
                        break;
                    case "CompanyName":
                        bookings = bookings.OrderByDescending(s => s.Company != null ? s.Company.CompanyName : "").ThenBy(s => s.ID);
                        break;
                    case "EventName":
                        bookings = bookings.OrderByDescending(s => s.Event != null ? s.Event.EventName : "").ThenBy(s => s.ID);
                        break;
                    case "EventCode":
                        bookings = bookings.OrderByDescending(s => s.Event != null ? s.Event.EventCode : "").ThenBy(s => s.ID);
                        break;
                    case "TotalWrittenRevenue":
                        bookings = bookings.OrderByDescending(s => s.TotalWrittenRevenue).ThenBy(s => s.ID);
                        break;
                    case "TotalPaidRevenue":
                        bookings = bookings.OrderByDescending(s => s.TotalPaidRevenue).ThenBy(s => s.ID);
                        break;
                    case "PaymentStatus":
                        bookings = bookings.OrderByDescending(s => s.PaymentStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "InvoiceNo":
                        bookings = bookings.OrderByDescending(s => s.InvoiceNo).ThenBy(s => s.ID);
                        break;
                    default:
                        bookings = bookings.OrderByDescending(s => s.ID);
                        break;
                }
            }

            #endregion sort

            recordsTotal = bookings.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = bookings.Skip(skip).Take(pageSize).ToList();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    CreatedTime = m.CreatedTime.ToString("dd/MM/yyyy"),
                    Country = m.Company != null ? m.Company.CountryName : "",
                    CompanyName = m.Company != null ? m.Company.CompanyName : "",
                    EventName = m.Event != null ? m.Event.EventName : "",
                    EventCode = m.Event != null ? m.Event.EventCode : "",
                    TotalWrittenRevenue = m.TotalWrittenRevenue.ToString("N0"),
                    TotalPaidRevenue = m.TotalPaidRevenue.ToString("N0"),
                    PaymentStatus = m.PaymentStatusDisplay,
                    m.InvoiceNo
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        [AjaxOnly]
        public ActionResult AjaxGetMyCallList()
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

            var companyId = 0;
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("CompanyId") != null && !string.IsNullOrEmpty(Request.Form.GetValues("CompanyId").FirstOrDefault()))
            {
                // ReSharper disable once PossibleNullReferenceException
                companyId = Convert.ToInt32(Request.Form.GetValues("CompanyId").FirstOrDefault().Trim().ToLower());
            }
            var name = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Name") != null && !string.IsNullOrEmpty(Request.Form.GetValues("Name").FirstOrDefault()))
            {
                // ReSharper disable once PossibleNullReferenceException
                name = Request.Form.GetValues("Name").FirstOrDefault().Trim().ToLower();
            }
            var designation = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Designation") != null && Request.Form.GetValues("Designation").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                designation = Request.Form.GetValues("Designation").FirstOrDefault().Trim().ToLower();
            }
            var email = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Email") != null && Request.Form.GetValues("Email").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                email = Request.Form.GetValues("Email").FirstOrDefault().Trim().ToLower();
            }

            var phone = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Mobile") != null && Request.Form.GetValues("Mobile").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                phone = Request.Form.GetValues("Mobile").FirstOrDefault().Trim().ToLower();
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int page = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var currentUser = CurrentUser.Identity;
            IEnumerable<Lead> leads = new HashSet<Lead>();
            if (CurrentUser.HasRole("Manager"))
            {
                leads = Mapper.Map<IEnumerable<Lead>>(_companyRepo.GetAllCompanyResources(companyId, name, designation, email, phone));
            }
            else
            {
                Func<Lead, bool> predicate = m =>
                    m.UserID == currentUser.ID && m.CompanyID == companyId &&
                    (string.IsNullOrEmpty(name) ||
                     (!string.IsNullOrEmpty(m.Name) && m.Name.ToLower().Contains(name))) &&
                    (string.IsNullOrEmpty(designation) ||
                     (!string.IsNullOrEmpty(m.JobTitle) && m.JobTitle.ToLower().Contains(designation))) &&
                    (string.IsNullOrEmpty(email) ||
                     (!string.IsNullOrEmpty(m.WorkEmail) && m.WorkEmail.ToLower().Contains(email)) ||
                     (!string.IsNullOrEmpty(m.WorkEmail1) && m.WorkEmail1.ToLower().Contains(email)) ||
                     (!string.IsNullOrEmpty(m.PersonalEmail) && m.PersonalEmail.ToLower().Contains(email))) &&
                    (string.IsNullOrEmpty(phone) ||
                     (!string.IsNullOrEmpty(m.MobilePhone1) && m.MobilePhone1.ToLower().Contains(phone)) ||
                     (!string.IsNullOrEmpty(m.MobilePhone2) && m.MobilePhone2.ToLower().Contains(phone)) ||
                     (!string.IsNullOrEmpty(m.MobilePhone3) && m.MobilePhone3.ToLower().Contains(phone)) ||
                     (!string.IsNullOrEmpty(m.DirectLine) && m.DirectLine.ToLower().Contains(phone)));
                leads = _leadService.GetAllLeads(predicate);
            }
            leads = leads.DistinctBy(m => m.Name);
            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "Salutation":
                        leads = leads.OrderBy(s => s.Salutation).ThenBy(s => s.ID).ToList();
                        break;
                    case "FirstName":
                        leads = leads.OrderBy(s => s.FirstName).ThenBy(s => s.ID).ToList();
                        break;
                    case "LastName":
                        leads = leads.OrderBy(s => s.LastName).ThenBy(s => s.ID).ToList();
                        break;
                    case "JobTitle":
                        leads = leads.OrderBy(s => s.JobTitle).ThenBy(s => s.ID).ToList();
                        break;
                    case "DirectLine":
                        leads = leads.OrderBy(s => s.DirectLine).ThenBy(s => s.ID).ToList();
                        break;
                    case "MobilePhone1":
                        leads = leads.OrderBy(s => s.MobilePhone1).ThenBy(s => s.ID).ToList();
                        break;
                    case "MobilePhone2":
                        leads = leads.OrderBy(s => s.MobilePhone2).ThenBy(s => s.ID).ToList();
                        break;
                    case "MobilePhone3":
                        leads = leads.OrderBy(s => s.MobilePhone3).ThenBy(s => s.ID).ToList();
                        break;
                    case "WorkEmail":
                        leads = leads.OrderBy(s => s.WorkEmail).ThenBy(s => s.ID).ToList();
                        break;
                    case "PersonalEmail":
                        leads = leads.OrderBy(s => s.PersonalEmail).ThenBy(s => s.ID).ToList();
                        break;
                    default:
                        leads = leads.OrderBy(s => s.ID).ToList();
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "Salutation":
                        leads = leads.OrderByDescending(s => s.Salutation).ThenBy(s => s.ID).ToList();
                        break;
                    case "FirstName":
                        leads = leads.OrderByDescending(s => s.FirstName).ThenBy(s => s.ID).ToList();
                        break;
                    case "LastName":
                        leads = leads.OrderByDescending(s => s.LastName).ThenBy(s => s.ID).ToList();
                        break;
                    case "JobTitle":
                        leads = leads.OrderByDescending(s => s.JobTitle).ThenBy(s => s.ID).ToList();
                        break;
                    case "DirectLine":
                        leads = leads.OrderByDescending(s => s.DirectLine).ThenBy(s => s.ID).ToList();
                        break;
                    case "MobilePhone1":
                        leads = leads.OrderByDescending(s => s.MobilePhone1).ThenBy(s => s.ID).ToList();
                        break;
                    case "MobilePhone2":
                        leads = leads.OrderByDescending(s => s.MobilePhone2).ThenBy(s => s.ID).ToList();
                        break;
                    case "MobilePhone3":
                        leads = leads.OrderByDescending(s => s.MobilePhone3).ThenBy(s => s.ID).ToList();
                        break;
                    case "WorkEmail":
                        leads = leads.OrderByDescending(s => s.WorkEmail).ThenBy(s => s.ID).ToList();
                        break;
                    case "PersonalEmail":
                        leads = leads.OrderByDescending(s => s.PersonalEmail).ThenBy(s => s.ID).ToList();
                        break;
                    default:
                        leads = leads.OrderByDescending(s => s.ID).ToList();
                        break;
                }
            }

            #endregion sort

            recordsTotal = leads.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = leads.Skip(page).Take(pageSize).ToList();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.Salutation,
                    m.FullName,
                    m.FirstName,
                    m.LastName,
                    m.JobTitle,
                    m.WorkEmail,
                    m.PersonalEmail,
                    m.MobilePhone1,
                    m.MobilePhone2,
                    m.MobilePhone3,
                    m.DirectLine,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        [AjaxOnly]
        public ActionResult AjaxGetTotalBooked()
        {
            var saleId = CurrentUser.Identity.ID;
            var leads = _bookingService.GetAllBookings(m =>
                m.BookingStatusRecord == BookingStatus.Approved &&
                (saleId == 0 || m.SalesmanID == saleId
                //||
                // (m.Salesman != null && m.Salesman.TransferUserID == saleId)
                ));

            return Json(new
            {
                TotalBooked = leads.Count()
            }, JsonRequestBehavior.AllowGet);
        }




        [AjaxOnly]
        public ActionResult AjaxGetDelegates(int eventId)
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


            var country = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Country") != null && !string.IsNullOrEmpty(Request.Form.GetValues("Country").FirstOrDefault()))
            {
                // ReSharper disable once PossibleNullReferenceException
                country = Request.Form.GetValues("Country").FirstOrDefault().Trim().ToLower();
            }
            var company = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Company") != null && !string.IsNullOrEmpty(Request.Form.GetValues("Company").FirstOrDefault()))
            {
                // ReSharper disable once PossibleNullReferenceException
                company = Request.Form.GetValues("Company").FirstOrDefault().Trim().ToLower();
            }
            var name = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Name") != null && !string.IsNullOrEmpty(Request.Form.GetValues("Name").FirstOrDefault()))
            {
                // ReSharper disable once PossibleNullReferenceException
                name = Request.Form.GetValues("Name").FirstOrDefault().Trim().ToLower();
            }
            var mobile = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Mobile") != null && Request.Form.GetValues("Mobile").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                mobile = Request.Form.GetValues("Mobile").FirstOrDefault().Trim().ToLower();
            }
            var email = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Email") != null && Request.Form.GetValues("Email").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                email = Request.Form.GetValues("Email").FirstOrDefault().Trim().ToLower();
            }
            var role = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Role") != null && Request.Form.GetValues("Role").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                role = Request.Form.GetValues("Role").FirstOrDefault().Trim().ToLower();
            }
            var session = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Session") != null && Request.Form.GetValues("Session").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                session = Request.Form.GetValues("Session").FirstOrDefault().Trim().ToLower();
            }
            var status = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Status") != null && Request.Form.GetValues("Status").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                status = Request.Form.GetValues("Status").FirstOrDefault().Trim().ToLower();
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int page = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            IEnumerable<PQT.Domain.Entities.Delegate> leads = new HashSet<PQT.Domain.Entities.Delegate>();
            var bookings = _bookingService.GetAllBookings(m => m.EventID == eventId && m.BookingStatusRecord == BookingStatus.Approved &&
                                                               (string.IsNullOrEmpty(country) ||
                                                                (m.Company != null && m.Company.CountryName.ToLower().Contains(country))) &&
                                                               (string.IsNullOrEmpty(company) ||
                                                                (m.Company != null && m.Company.CompanyName.ToLower().Contains(company))));
            Func<PQT.Domain.Entities.Delegate, bool> predicate = m =>
                (string.IsNullOrEmpty(name) ||
                 (!string.IsNullOrEmpty(m.FullName) && m.FullName.ToLower().Contains(name))) &&
                (string.IsNullOrEmpty(email) ||
                 (!string.IsNullOrEmpty(m.WorkEmail) && m.WorkEmail.ToLower().Contains(email)) ||
                 (!string.IsNullOrEmpty(m.PersonalEmail) && m.PersonalEmail.ToLower().Contains(email))) &&
                (string.IsNullOrEmpty(role) ||
                 (!string.IsNullOrEmpty(m.JobTitle) && m.JobTitle.ToLower().Contains(role))) &&
                (string.IsNullOrEmpty(session) ||
                 (!string.IsNullOrEmpty(m.Session) && m.Session.ToLower().Contains(session))) &&
                (string.IsNullOrEmpty(status) ||
                 (!string.IsNullOrEmpty(m.AttendanceStatusDisplay) && m.AttendanceStatusDisplay.ToLower().Contains(status))) &&
                (string.IsNullOrEmpty(mobile) ||
                 (!string.IsNullOrEmpty(m.DirectLine) && m.DirectLine.ToLower().Contains(mobile)) ||
                 (!string.IsNullOrEmpty(m.MobilePhone1) && m.MobilePhone1.ToLower().Contains(mobile)) ||
                 (!string.IsNullOrEmpty(m.MobilePhone2) && m.MobilePhone2.ToLower().Contains(mobile)) ||
                 (!string.IsNullOrEmpty(m.MobilePhone3) && m.MobilePhone3.ToLower().Contains(mobile)));
            leads = bookings.SelectMany(m => m.Delegates.Select(d =>
                    d.PassInfo(m.Company.CountryName, m.CompanyName, m.Salesman.DisplayName, m.EventSessions)))
                .Where(predicate);

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        leads = leads.OrderBy(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "Salesman":
                        leads = leads.OrderBy(s => s.Salesman).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        leads = leads.OrderBy(s => s.Country).ThenBy(s => s.ID);
                        break;
                    case "Salutation":
                        leads = leads.OrderBy(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        leads = leads.OrderBy(s => s.Company).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderBy(s => s.DirectLine).ThenBy(s => s.ID);
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
                    case "JobTitle":
                        leads = leads.OrderBy(s => s.JobTitle).ThenBy(s => s.ID);
                        break;
                    case "Session":
                        leads = leads.OrderBy(s => s.Session).ThenBy(s => s.ID);
                        break;
                    case "AttendanceStatus":
                        leads = leads.OrderBy(s => s.AttendanceStatus).ThenBy(s => s.ID);
                        break;
                    case "OverallFeedbacks":
                        leads = leads.OrderBy(s => s.OverallFeedbacks).ThenBy(s => s.ID);
                        break;
                    case "OpTopicsInterested":
                        leads = leads.OrderBy(s => s.OpTopicsInterested).ThenBy(s => s.ID);
                        break;
                    case "OpLocationsInterested":
                        leads = leads.OrderBy(s => s.OpLocationsInterested).ThenBy(s => s.ID);
                        break;
                    case "OpGoodTrainingMonth":
                        leads = leads.OrderBy(s => s.OpGoodTrainingMonth).ThenBy(s => s.ID);
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
                    case "Salesman":
                        leads = leads.OrderByDescending(s => s.Salesman).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        leads = leads.OrderByDescending(s => s.Country).ThenBy(s => s.ID);
                        break;
                    case "Salutation":
                        leads = leads.OrderByDescending(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        leads = leads.OrderByDescending(s => s.Company).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderByDescending(s => s.DirectLine).ThenBy(s => s.ID);
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
                    case "JobTitle":
                        leads = leads.OrderByDescending(s => s.JobTitle).ThenBy(s => s.ID);
                        break;
                    case "Session":
                        leads = leads.OrderByDescending(s => s.Session).ThenBy(s => s.ID);
                        break;
                    case "AttendanceStatus":
                        leads = leads.OrderByDescending(s => s.AttendanceStatus).ThenBy(s => s.ID);
                        break;
                    case "OverallFeedbacks":
                        leads = leads.OrderByDescending(s => s.OverallFeedbacks).ThenBy(s => s.ID);
                        break;
                    case "OpTopicsInterested":
                        leads = leads.OrderByDescending(s => s.OpTopicsInterested).ThenBy(s => s.ID);
                        break;
                    case "OpLocationsInterested":
                        leads = leads.OrderByDescending(s => s.OpLocationsInterested).ThenBy(s => s.ID);
                        break;
                    case "OpGoodTrainingMonth":
                        leads = leads.OrderByDescending(s => s.OpGoodTrainingMonth).ThenBy(s => s.ID);
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
            var data = leads.Skip(page).Take(pageSize).ToList();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    CreatedTime = m.CreatedTimeStr,
                    m.Salesman,
                    m.Country,
                    m.Company,
                    m.DirectLine,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.MobilePhone1,
                    m.MobilePhone2,
                    m.MobilePhone3,
                    m.WorkEmail,
                    m.PersonalEmail,
                    m.JobTitle,
                    m.Session,
                    AttendanceStatus = m.AttendanceStatusDisplay,
                    m.OverallFeedbacks,
                    m.OpTopicsInterested,
                    m.OpLocationsInterested,
                    m.OpGoodTrainingMonth,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        [AjaxOnly]
        public ActionResult AjaxGetCompaniesApproved(int eventId)
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
            var companyName = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("CompanyName") != null && Request.Form.GetValues("CompanyName").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                companyName = Request.Form.GetValues("CompanyName").FirstOrDefault().Trim().ToLower();
            }
            var countryName = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("CountryName") != null && Request.Form.GetValues("CountryName").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                countryName = Request.Form.GetValues("CountryName").FirstOrDefault().Trim().ToLower();
            }
            var attendanceStatus = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("AttendanceStatus") != null && Request.Form.GetValues("AttendanceStatus").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                attendanceStatus = Request.Form.GetValues("AttendanceStatus").FirstOrDefault().Trim().ToLower();
            }
            var paymentStatus = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("PaymentStatus") != null && Request.Form.GetValues("PaymentStatus").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                paymentStatus = Request.Form.GetValues("PaymentStatus").FirstOrDefault().Trim().ToLower();
            }


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            IEnumerable<Booking> bookings = _bookingService.GetAllBookings(m =>
                m.EventID == eventId && m.BookingStatusRecord == BookingStatus.Approved &&
                (string.IsNullOrEmpty(countryName) ||
                 (m.Company != null && m.Company.CountryName.ToLower().Contains(countryName))) &&
                (string.IsNullOrEmpty(companyName) ||
                 (m.Company != null && m.Company.CompanyName.ToLower().Contains(companyName))) &&
                (string.IsNullOrEmpty(attendanceStatus) ||
                 (m.AttendanceStatusDisplay != null &&
                  m.AttendanceStatusDisplay.ToLower().Contains(attendanceStatus))) &&
                (string.IsNullOrEmpty(paymentStatus) ||
                 (m.PaymentStatusDisplay != null && m.PaymentStatusDisplay.ToLower().Contains(paymentStatus))));

            IEnumerable<Booking> companies = bookings.DistinctBy(m => m.CompanyID).ToList();
            foreach (var company in companies)
            {
                var deleNo = bookings.Where(m => m.CompanyID == company.CompanyID).Sum(m => m.DelegateNumber);
                company.DelegateNumber = deleNo;
            }

            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "Country":
                        companies = companies.OrderBy(s => s.Company.CountryName).ThenBy(s => s.ID);
                        break;
                    case "DelegateNumber":
                        companies = companies.OrderBy(s => s.DelegateNumber).ThenBy(s => s.ID);
                        break;
                    case "AttendanceStatus":
                        companies = companies.OrderBy(s => s.AttendanceStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "PaymentStatus":
                        companies = companies.OrderBy(s => s.AttendanceStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "CompanyName":
                        companies = companies.OrderBy(s => s.CompanyName).ThenBy(s => s.ID);
                        break;
                    default:
                        companies = companies.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "Country":
                        companies = companies.OrderByDescending(s => s.Company.CountryName).ThenBy(s => s.ID);
                        break;
                    case "DelegateNumber":
                        companies = companies.OrderByDescending(s => s.DelegateNumber).ThenBy(s => s.ID);
                        break;
                    case "AttendanceStatus":
                        companies = companies.OrderByDescending(s => s.AttendanceStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "PaymentStatus":
                        companies = companies.OrderByDescending(s => s.PaymentStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "CompanyName":
                        companies = companies.OrderByDescending(s => s.CompanyName).ThenBy(s => s.ID);
                        break;
                    default:
                        companies = companies.OrderByDescending(s => s.ID);
                        break;
                }
            }


            recordsTotal = companies.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            IEnumerable<Booking> data;
            if (pageSize < 1)
            {
                data = companies.Skip(skip).ToList();
            }
            else
            {
                data = companies.Skip(skip).Take(pageSize).ToList();
            }

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    ID = m.CompanyID,
                    Country = m.Company.CountryName,
                    m.CompanyName,
                    m.DelegateNumber,
                    AttendanceStatus = m.AttendanceStatusDisplay,
                    PaymentStatus = m.PaymentStatusDisplay,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

    }
}
