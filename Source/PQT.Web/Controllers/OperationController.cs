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

        public OperationController(IEventService repo, IUnitRepository unitRepository)
        {
            _repo = repo;
            _unitRepository = unitRepository;
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
                    return Json(new { IsSuccess = false, Message = "data not found" });
                if (hotel.Status != InfoStatus.Request)
                    return Json(new { IsSuccess = false, Message = "data has " + hotel.Status.DisplayName });
                hotel.Status = InfoStatus.Approved;
                hotel.RejectMessage = "";
                if (!_unitRepository.UpdateVenueInfo(hotel))
                {
                    return Json(new { IsSuccess = false, Message = "Approve failed" });
                }
                _repo.UpdateVenueInfo(hotel);
                OpeEventNotificator.NotifyUser(NotifyAction.Approved, hotel.EntryId, "Approved Venue Info");
            }
            else
            {
                var hotel = _unitRepository.GetAccomodationInfo(id);
                if (hotel == null)
                    return Json(new { IsSuccess = false, Message = "data not found" });
                if (hotel.Status != InfoStatus.Request)
                    return Json(new { IsSuccess = false, Message = "data has " + hotel.Status.DisplayName });
                hotel.Status = InfoStatus.Approved;
                hotel.RejectMessage = "";
                if (!_unitRepository.UpdateAccomodationInfo(hotel))
                {
                    return Json(new { IsSuccess = false, Message = "Approve failed" });
                }
                _repo.UpdateAccomodationInfo(hotel);
                OpeEventNotificator.NotifyUser(NotifyAction.Approved, hotel.EntryId, "Approved Accomodation Info");
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
                    return Json(new { IsSuccess = false, Message = "data not found" });
                if (hotel.Status != InfoStatus.Request)
                    return Json(new { IsSuccess = false, Message = "data has " + hotel.Status.DisplayName });

                hotel.RejectMessage = reason;
                hotel.Status = InfoStatus.Rejected;
                if (!_unitRepository.UpdateVenueInfo(hotel))
                {
                    return Json(new { IsSuccess = false, Message = "Reject failed" });
                }
                _repo.UpdateVenueInfo(hotel);
                OpeEventNotificator.NotifyUser(NotifyAction.Rejected, hotel.EntryId, "Rejected Venue Info");
            }
            else
            {
                var hotel = _unitRepository.GetAccomodationInfo(id);
                if (hotel == null)
                    return Json(new { IsSuccess = false, Message = "data not found" });
                if (hotel.Status != InfoStatus.Request)
                    return Json(new { IsSuccess = false, Message = "data has " + hotel.Status.DisplayName });
                hotel.RejectMessage = reason;
                hotel.Status = InfoStatus.Rejected;
                if (!_unitRepository.UpdateAccomodationInfo(hotel))
                {
                    return Json(new { IsSuccess = false, Message = "Reject failed" });
                }
                _repo.UpdateAccomodationInfo(hotel);
                OpeEventNotificator.NotifyUser(NotifyAction.Rejected, hotel.EntryId, "Rejected Accomodation Info");
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
            int recordsTotal = 0;

            IEnumerable<Event> events = new HashSet<Event>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                if (!string.IsNullOrEmpty(searchValue))
                    events = _repo.GetAllEvents(m =>
                        m.EventCode.ToLower().Contains(searchValue) ||
                        m.EventName.ToLower().Contains(searchValue) ||
                        m.StartDate.ToString("dd/MM/yyyy").Contains(searchValue) ||
                        m.EndDate.ToString("dd/MM/yyyy").Contains(searchValue) ||
                        m.DateOfConfirmationStr.Contains(searchValue) ||
                        m.ClosingDateStr.ToLower().Contains(searchValue) ||
                        m.EventStatusDisplay.ToLower().Contains(searchValue) ||
                        (m.Location != null && m.Location.ToLower().Contains(searchValue)) ||
                        (m.HotelVenue != null && m.HotelVenue.ToLower().Contains(searchValue))
                       );
            }
            else
            {
                events = _repo.GetAllEvents();
            }

            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "EventCode":
                        events = events.OrderBy(s => s.EventCode).ThenBy(s => s.ID);
                        break;
                    case "EventName":
                        events = events.OrderBy(s => s.EventName).ThenBy(s => s.ID);
                        break;
                    case "EventStatusDisplay":
                        events = events.OrderBy(s => s.EventStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "StartDate":
                        events = events.OrderBy(s => s.StartDate).ThenBy(s => s.ID);
                        break;
                    case "EndDate":
                        events = events.OrderBy(s => s.EndDate).ThenBy(s => s.ID);
                        break;
                    case "DateOfConfirmation":
                        events = events.OrderBy(s => s.DateOfConfirmation).ThenBy(s => s.ID);
                        break;
                    case "ClosingDate":
                        events = events.OrderBy(s => s.ClosingDate).ThenBy(s => s.ID);
                        break;
                    case "Location":
                        events = events.OrderBy(s => s.Location).ThenBy(s => s.ID);
                        break;
                    case "HotelVenue":
                        events = events.OrderBy(s => s.HotelVenue).ThenBy(s => s.ID);
                        break;
                    default:
                        events = events.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "EventCode":
                        events = events.OrderByDescending(s => s.EventCode).ThenBy(s => s.ID);
                        break;
                    case "EventName":
                        events = events.OrderByDescending(s => s.EventName).ThenBy(s => s.ID);
                        break;
                    case "EventStatusDisplay":
                        events = events.OrderByDescending(s => s.EventStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "StartDate":
                        events = events.OrderByDescending(s => s.StartDate).ThenBy(s => s.ID);
                        break;
                    case "EndDate":
                        events = events.OrderByDescending(s => s.EndDate).ThenBy(s => s.ID);
                        break;
                    case "DateOfConfirmation":
                        events = events.OrderByDescending(s => s.DateOfConfirmation).ThenBy(s => s.ID);
                        break;
                    case "ClosingDate":
                        events = events.OrderByDescending(s => s.ClosingDate).ThenBy(s => s.ID);
                        break;
                    case "Location":
                        events = events.OrderByDescending(s => s.Location).ThenBy(s => s.ID);
                        break;
                    case "HotelVenue":
                        events = events.OrderByDescending(s => s.HotelVenue).ThenBy(s => s.ID);
                        break;
                    default:
                        events = events.OrderByDescending(s => s.ID);
                        break;
                }
            }


            recordsTotal = events.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = events.Skip(skip).Take(pageSize).ToList();

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
