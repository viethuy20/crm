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
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Models;

namespace PQT.Web.Controllers
{
    public class OperationController : Controller
    {
        private readonly IEventService _repo;
        private readonly IUnitRepository _unitRepository;
        private readonly IBookingService _bookingService;

        public OperationController(IEventService repo, IUnitRepository unitRepository, IBookingService bookingService)
        {
            _repo = repo;
            _unitRepository = unitRepository;
            _bookingService = bookingService;
        }

        //
        // GET: /Operation/

        [DisplayName(@"Event management")]
        public ActionResult Index()
        {
            return View(new List<Event>());
        }
        [DisplayName(@"Detail")]
        public ActionResult Detail(int id)
        {
            var model = new EventModel();
            model.PrepareEdit(id);
            return View(model);
        }

        [DisplayName("Edit")]
        public ActionResult Edit(int id)
        {
            var model = new EventModel();
            model.PrepareOperationEdit(id);
            if (model.Event == null)
            {
                TempData["error"] = "Data not found";
                return RedirectToAction("Index");
            }
            return View(model);
        }
        [DisplayName("Edit")]
        [HttpPost]
        public ActionResult Edit(EventModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.OperationUpdate())
                {
                    TempData["message"] = "Updated successful";
                    return RedirectToAction("Detail", new { id = model.ID });
                }
            }
            model.PrepareOperationEdit(model.ID, false);
            return View(model);
        }

        [DisplayName(@"Approval Hotel")]
        [HttpPost]
        public ActionResult ApprovalHotel(int id, string actType)
        {
            if (actType == "venue")
            {
                var hotel = _unitRepository.GetVenueInfo(id);
                if (hotel == null)
                    return Json(new { IsSuccess = false, Message = "Data not found" });
                if (hotel.Status != InfoStatus.Request)
                    return Json(new { IsSuccess = false, Message = "Data has " + hotel.Status.DisplayName });
                hotel.Status = InfoStatus.Approved;
                hotel.RejectMessage = "";
                if (!_unitRepository.UpdateVenueInfo(hotel))
                {
                    return Json(new { IsSuccess = false, Message = "Approve failed" });
                }
                _repo.UpdateVenueInfo(hotel);
                OpeEventNotificator.NotifyUser(NotifyAction.Approved, hotel.EntryId, "Hotel For Venue has been approved");
            }
            else
            {
                var hotel = _unitRepository.GetAccomodationInfo(id);
                if (hotel == null)
                    return Json(new { IsSuccess = false, Message = "Data not found" });
                if (hotel.Status != InfoStatus.Request)
                    return Json(new { IsSuccess = false, Message = "Data has " + hotel.Status.DisplayName });
                hotel.Status = InfoStatus.Approved;
                hotel.RejectMessage = "";
                if (!_unitRepository.UpdateAccomodationInfo(hotel))
                {
                    return Json(new { IsSuccess = false, Message = "Approve failed" });
                }
                _repo.UpdateAccomodationInfo(hotel);
                OpeEventNotificator.NotifyUser(NotifyAction.Approved, hotel.EntryId, "Hotel For Accomodation has been approved");
            }
            return Json(new { IsSuccess = true });
        }

        [DisplayName(@"Reject Hotel")]
        public ActionResult RejectHotel(int id, string actType)
        {
            ViewBag.ID = id;
            ViewBag.Type = actType;
            return PartialView(0);
        }

        [DisplayName(@"Reject Hotel")]
        [HttpPost]
        public ActionResult RejectHotel(int id, string reason, string actType)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return Json(new
                {
                    Message = "`Reason` must not be empty",
                    IsSuccess = false
                });
            }

            if (actType == "venue")
            {
                var hotel = _unitRepository.GetVenueInfo(id);
                if (hotel == null)
                    return Json(new { IsSuccess = false, Message = "Data not found" });
                if (hotel.Status != InfoStatus.Request)
                    return Json(new { IsSuccess = false, Message = "Data has " + hotel.Status.DisplayName });

                hotel.RejectMessage = reason;
                hotel.Status = InfoStatus.Rejected;
                if (!_unitRepository.UpdateVenueInfo(hotel))
                {
                    return Json(new { IsSuccess = false, Message = "Reject failed" });
                }
                _repo.UpdateVenueInfo(hotel);
                OpeEventNotificator.NotifyUser(NotifyAction.Rejected, hotel.EntryId, "Hotel For Venue has been rejected");
            }
            else
            {
                var hotel = _unitRepository.GetAccomodationInfo(id);
                if (hotel == null)
                    return Json(new { IsSuccess = false, Message = "Data not found" });
                if (hotel.Status != InfoStatus.Request)
                    return Json(new { IsSuccess = false, Message = "Data has " + hotel.Status.DisplayName });
                hotel.RejectMessage = reason;
                hotel.Status = InfoStatus.Rejected;
                if (!_unitRepository.UpdateAccomodationInfo(hotel))
                {
                    return Json(new { IsSuccess = false, Message = "Reject failed" });
                }
                _repo.UpdateAccomodationInfo(hotel);
                OpeEventNotificator.NotifyUser(NotifyAction.Rejected, hotel.EntryId, "Hotel For Accomodation has been rejected");
            }
            return Json(new { IsSuccess = true });
        }

        [AjaxOnly]
        public ActionResult AjaxGetEventAlls()
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

            var bookings = _bookingService.GetAllBookings(m => m.BookingStatusRecord == BookingStatus.Approved);

            int recordsTotal = _repo.GetCountEventsForOpe(searchValue);
            var data = _repo.GetAllEventsForOpe(searchValue, sortColumnDir, sortColumn, skip, pageSize);

            foreach (var item in data)
            {
                item.TotalDelegates = bookings.Where(m => m.EventID == item.ID).Sum(m => m.Delegates.Count);
            }

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.EventCode,
                    m.EventName,
                    m.EventStatusDisplay,
                    m.BackgroundColor,
                    m.Location,
                    m.HotelVenue,
                    m.TotalDelegates,
                    StartDate = m.StartDate.ToString("dd/MM/yyyy"),
                    EndDate = m.EndDate.ToString("dd/MM/yyyy"),
                    DateOfConfirmation = m.DateOfConfirmationStr,
                    ClosingDate = m.ClosingDateStr
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

    }
}
