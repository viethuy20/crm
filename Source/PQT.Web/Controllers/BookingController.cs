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
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateOrEdit(int id, int leadId)
        {
            var model = new BookingModel();
            if (id > 0)
            {
                var booking = _bookingService.GetBooking(id);
                model.Prepare(booking.LeadID);
                model.Booking = booking;
                model.SessionIds = booking.EventSessions.Select(m => m.ID).ToList();
            }
            else
            {
                model.Prepare(leadId);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateOrEdit(BookingModel model)
        {
            var flag = model.Booking.ID > 0 ? model.Update() : (model.Create() != null);
            var lead = _leadService.GetLead(model.Booking.LeadID);
            if (flag)
            {
                var leadStatusRecord = new LeadStatusRecord(lead.ID, LeadStatus.RequestBook, CurrentUser.Identity.ID);
                lead.LeadStatusRecord = leadStatusRecord;
                _leadService.UpdateLead(lead);
                var notiUsers = _membershipService.GetUsersInRole(new string[] { "Manager", "Finance" });
                LeadNotificator.NotifyUser(notiUsers, lead);
                TempData["message"] = Resource.SaveSuccessful;
                return RedirectToAction("Index", "Lead", new { id = model.Booking.EventID });
            }
            TempData["error"] = Resource.SaveError;
            model.Event = _eventService.GetEvent(lead.EventID);
            model.Companies = _companyRepo.GetAllCompanies();
            return View(model);
        }
        
        [DisplayName(@"Approve booking")]
        public ActionResult ApproveBooking(int id)
        {
            var booking = _bookingService.GetBooking(id);
            if (_bookingService.UpdateBooking(booking, BookingStatus.Approved, CurrentUser.Identity.ID))
            {
                TempData["message"] = Resource.SaveSuccessful;
                return RedirectToAction("Index");
            }
            TempData["error"] = Resource.SaveError;
            return RedirectToAction("Index");
        }
        [DisplayName(@"Reject booking")]
        public ActionResult RejectBooking(int id)
        {
            var model = new BookingModel();
            model.BookingID = id;
            return PartialView(model);
        }
        [DisplayName(@"Reject booking")]
        [HttpPost]
        public ActionResult RejectBooking(BookingModel model)
        {
            var booking = _bookingService.GetBooking(model.BookingID);
            if (_bookingService.UpdateBooking(booking, BookingStatus.Rejected, CurrentUser.Identity.ID, model.Message))
            {
                TempData["message"] = "The booking was declined.";
                return RedirectToAction("Index");
            }
            TempData["error"] = Resource.SaveError;
            return RedirectToAction("Index");
        }

        public ActionResult Action(int id)
        {
            var model = new BookingModel();
            var booking = _bookingService.GetBooking(id);
            model.Prepare(booking.LeadID);
            model.Booking = booking;
            return PartialView(model);
        }

        [AjaxOnly]
        public ActionResult AjaxGetBookings()
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
                bookings = _bookingService.GetAllBookings(m => (
                                                   (m.CreatedTime.ToString("dd/MM/yyyy HH:mm:ss").Contains(searchValue) ||
                                                   m.Company != null && m.Company.CompanyName.Contains(searchValue)) ||
                                                   m.Address.Contains(searchValue) ||
                                                   m.Tel.Contains(searchValue) ||
                                                   m.Fax.Contains(searchValue) ||
                                                   m.AuthoriserName.Contains(searchValue) ||
                                                   m.SenderName.Contains(searchValue) ||
                                                   (m.Event != null && m.Event.EventName.Contains(searchValue)) ||
                                                   (m.Salesman != null && m.Salesman.DisplayName.Contains(searchValue)) ||
                                                   m.FeePerDelegate.ToString().Contains(searchValue) ||
                                                   m.DiscountPercent.ToString().Contains(searchValue) ||
                                                   m.RevenueAmount.ToString().Contains(searchValue) ||
                                                   m.TotalPaidRevenue.ToString().Contains(searchValue) ||
                                                   (m.BookingStatusRecord != null && m.BookingStatusRecord.Status.DisplayName.Contains(searchValue))));
            }
            else
            {
                bookings = _bookingService.GetAllBookings();
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
                        bookings = bookings.OrderBy(s => s.Company!= null ? s.Company.CompanyName : "").ThenBy(s => s.ID);
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
                    case "Salesman":
                        bookings = bookings.OrderBy(s => s.Salesman != null ? s.Salesman.DisplayName : "").ThenBy(s => s.ID);
                        break;
                    case "FeePerDelegate":
                        bookings = bookings.OrderBy(s => s.FeePerDelegate).ThenBy(s => s.ID);
                        break;
                    case "Discount":
                        bookings = bookings.OrderBy(s => s.DiscountPercent).ThenBy(s => s.ID);
                        break;
                    case "RevenueAmount":
                        bookings = bookings.OrderBy(s => s.RevenueAmount).ThenBy(s => s.ID);
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
                    case "Salesman":
                        bookings = bookings.OrderByDescending(s => s.Salesman != null ? s.Salesman.DisplayName : "").ThenBy(s => s.ID);
                        break;
                    case "FeePerDelegate":
                        bookings = bookings.OrderByDescending(s => s.FeePerDelegate).ThenBy(s => s.ID);
                        break;
                    case "Discount":
                        bookings = bookings.OrderByDescending(s => s.DiscountPercent).ThenBy(s => s.ID);
                        break;
                    case "RevenueAmount":
                        bookings = bookings.OrderByDescending(s => s.RevenueAmount).ThenBy(s => s.ID);
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
                    Salesman = m.Salesman != null ? m.Salesman.DisplayName : "",
                    m.FeePerDelegate,
                    Discount = m.DiscountPercent,
                    m.RevenueAmount,
                    m.TotalPaidRevenue,
                    Status = m.BookingStatusRecord != null ? m.BookingStatusRecord.Status.DisplayName : "",
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

    }
}
