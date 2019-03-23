using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Infrastructure.Utility;
using PQT.Web.Models;
using Resources;

namespace PQT.Web.Controllers
{
    public class LeaveController : Controller
    {
        private readonly ILeaveService _leaveService;

        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        //
        // GET: /Invoice/
        [DisplayName(@"List management")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Detail(int id = 0)
        {
            var model = new LeaveModel();
            if (id > 0)
            {
                model.Leave = _leaveService.GetLeave(id);
            }
            if (model.Leave == null)
            {
                TempData["error"] = "Leave not found";
                return RedirectToAction("Index");
            }
            return View(model);
        }
        public ActionResult Create()
        {
            var model = new LeaveModel();
            model.Prepare();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(LeaveModel model)
        {
            //var invoice = _leaveService.GetInvoiceByBooking(model.Invoice.BookingID);
            //if (invoice != null)
            //{
            //    TempData["error"] = "Booking has been invoiced";
            //    return RedirectToAction("Index");
            //}
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Create())
                    {
                        TempData["message"] = Resource.SaveSuccessful;
                        return RedirectToAction("Index");
                    }
                    TempData["error"] = Resource.SaveError;
                }
                catch (Exception e)
                {
                    if (e.InnerException != null)
                    {
                        TempData["error"] = e.Message + " >>>>> " + e.InnerException.Message;
                    }
                    else
                    {
                        TempData["error"] = e.Message;
                    }
                }
            }
            model.Prepare();
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = new LeaveModel();
            model.Prepare(id);
            if (model.Leave == null)
            {
                TempData["error"] = "Leave not found";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(LeaveModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var flag = model.Update();
                    if (flag)
                    {
                        TempData["message"] = Resource.SaveSuccessful;
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
            model.Prepare(model.Leave.ID);
            return View(model);
        }

        //[DisplayName(@"Approve Leave")]
        //public ActionResult ApproveLeave(int id)
        //{
        //    var model = new LeaveModel { LeaveID = id };
        //    try
        //    {
        //        if (model.Approve())
        //        {
        //            TempData["message"] = Resource.SaveSuccessful;
        //            return RedirectToAction("Detail", new { id = id });
        //        }
        //        TempData["error"] = Resource.SaveError;
        //    }
        //    catch (Exception e)
        //    {
        //        TempData["error"] = e.Message;
        //    }
        //    return RedirectToAction("Index");
        //}
        //[DisplayName(@"Reject Leave")]
        //public ActionResult RejectLeave(int id)
        //{
        //    var model = new LeaveModel();
        //    model.LeaveID = id;
        //    model.Leave = _leaveService.GetLeave(id);
        //    if (model.Leave == null)
        //    {
        //        TempData["error"] = "Leave not found";
        //        return RedirectToAction("Index");
        //    }
        //    return PartialView(model);
        //}
        //[DisplayName(@"Reject Leave")]
        //[HttpPost]
        //public ActionResult RejectLeave(LeaveModel model)
        //{
        //    if (string.IsNullOrEmpty(model.Message))
        //    {
        //        TempData["error"] = "`Reason` must not be empty";
        //        return RedirectToAction("Detail", new { id = model.LeaveID });
        //    }
        //    try
        //    {
        //        if (model.Reject())
        //        {
        //            TempData["message"] = Resource.SaveSuccessful;
        //            return RedirectToAction("Detail", new { id = model.LeaveID });
        //        }
        //        TempData["error"] = Resource.SaveError;
        //    }
        //    catch (Exception e)
        //    {
        //        TempData["error"] = e.Message;
        //    }
        //    return RedirectToAction("Index");
        //}
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var model = new LeaveModel { LeaveID = id };
            if (model.Delete())
            {
                return Json(true);
            }
            return Json(false);
        }
        [DisplayName(@"Monthly Report")]
        public ActionResult MonthlyReport()
        {
            var model = new LeaveMonthlyReport();
            return View(model);
        }
        [AjaxOnly]
        public ActionResult AjaxGetAlls()
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
            var currentUser = CurrentUser.Identity;
            var currentUserId = CurrentUser.HasRole("Manager") ? 0 : currentUser.ID;
            var isSupervisor = currentUser.FinanceAdminUnit != FinanceAdminUnit.None ||
                                 currentUser.SalesManagementUnit != SalesManagementUnit.None ||
                                 currentUser.HumanResourceUnit == HumanResourceUnit.Coordinator ||
                                 currentUser.ProjectManagementUnit != ProjectManagementUnit.None;
            IEnumerable<Leave> data = new HashSet<Leave>();
            Func<Leave, bool> predicate = null;
            if (!string.IsNullOrEmpty(searchValue))
            {
                if (currentUserId > 0)
                {
                    if (isSupervisor)
                    {
                        predicate = m =>
                            (m.UserID == currentUserId || m.User.DirectSupervisorID == currentUserId) &&
                            ((m.AprroveUserDisplay.ToLower().Contains(searchValue)) ||
                             (m.Summary != null && m.Summary.ToLower().Contains(searchValue)) ||
                             (m.LeaveType.DisplayName.ToLower().Contains(searchValue)) ||
                             (m.TypeOfLeave.DisplayName.ToLower().Contains(searchValue)) ||
                             (m.TypeOfLatenes.DisplayName.ToLower().Contains(searchValue)) ||
                             m.LeaveDateDisplay.ToLower().Contains(searchValue));
                    }
                    else
                    {
                        predicate = m =>
                            (m.UserID == currentUserId) &&
                            ((m.AprroveUserDisplay.ToLower().Contains(searchValue)) ||
                             (m.Summary != null && m.Summary.ToLower().Contains(searchValue)) ||
                             (m.LeaveType.DisplayName.ToLower().Contains(searchValue)) ||
                             (m.TypeOfLeave.DisplayName.ToLower().Contains(searchValue)) ||
                             (m.TypeOfLatenes.DisplayName.ToLower().Contains(searchValue)) ||
                             m.LeaveDateDisplay.ToLower().Contains(searchValue));
                    }
                }
                else
                {
                    predicate = m =>
                        (m.AprroveUserDisplay.ToLower().Contains(searchValue)) ||
                        (m.Summary != null && m.Summary.ToLower().Contains(searchValue)) ||
                        (m.LeaveType.DisplayName.ToLower().Contains(searchValue)) ||
                        (m.TypeOfLeave.DisplayName.ToLower().Contains(searchValue)) ||
                        (m.TypeOfLatenes.DisplayName.ToLower().Contains(searchValue)) ||
                        m.LeaveDateDisplay.ToLower().Contains(searchValue);
                }
            }
            else
            {
                if (currentUserId > 0)
                {
                    if (isSupervisor)
                    {
                        predicate = m => (m.UserID == currentUserId || m.User.DirectSupervisorID == currentUserId);
                    }
                    else
                    {
                        predicate = m => (m.UserID == currentUserId);
                    }
                }
            }
            recordsTotal = _leaveService.GetCountLeaves(predicate);

            switch (sortColumn)
            {
                case "UserDisplay":
                    data = _leaveService.GetAllLeaves(predicate, sortColumnDir, s => s.UserDisplay, skip, pageSize);
                    break;
                case "LeaveDateDisplay":
                    data = _leaveService.GetAllLeaves(predicate, sortColumnDir, s => s.LeaveDate, skip, pageSize);
                    break;
                case "AprroveUserDisplay":
                    data = _leaveService.GetAllLeaves(predicate, sortColumnDir, s => s.AprroveUserDisplay, skip, pageSize);
                    break;
                case "LeaveType":
                    data = _leaveService.GetAllLeaves(predicate, sortColumnDir, s => s.LeaveType.DisplayName, skip, pageSize);
                    break;
                case "ReasonLeave":
                    data = _leaveService.GetAllLeaves(predicate, sortColumnDir, s => s.ReasonLeave, skip, pageSize);
                    break;
                default:
                    data = _leaveService.GetAllLeaves(predicate, sortColumnDir, s => s.ID, skip, pageSize); ;
                    break;
            }

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.CreatedUserID,
                    m.UserDisplay,
                    m.LeaveDateDisplay,
                    m.AprroveUserDisplay,
                    m.Summary,
                    LeaveType = m.LeaveType.DisplayName,
                    m.ReasonLeave,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        [AjaxOnly]
        public ActionResult AjaxGetMonthlyReport()
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
            var today = DateTime.Today;
            var monthReport = new DateTime(today.Year, today.Month, 1);
            if (Request.Form.GetValues("Month").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                var _dateFrom = Request.Form.GetValues("Month").FirstOrDefault().Trim().ToLower();
                if (!string.IsNullOrEmpty(_dateFrom))
                    monthReport = DateTime.ParseExact(_dateFrom, "MM/yyyy", CultureInfo.InvariantCulture); ;
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var currentUser = CurrentUser.Identity;
            var currentUserId = CurrentUser.HasRole("Manager") ? 0 : currentUser.ID;
            var isSupervisor = currentUser.FinanceAdminUnit != FinanceAdminUnit.None ||
                               currentUser.SalesManagementUnit != SalesManagementUnit.None ||
                               currentUser.HumanResourceUnit == HumanResourceUnit.Coordinator ||
                               currentUser.ProjectManagementUnit != ProjectManagementUnit.None;
            IEnumerable<Leave> leaves = new HashSet<Leave>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                if (currentUserId > 0)
                {
                    if (isSupervisor)
                    {
                        leaves = _leaveService.GetAllLeaves(m =>
                            (m.UserID == currentUserId || m.User.DirectSupervisorID == currentUserId) &&
                            m.LeaveDate.Month == monthReport.Month &&
                            (m.User.DisplayName.ToLower().Contains(searchValue)));
                    }
                    else
                    {
                        leaves = _leaveService.GetAllLeaves(m =>
                            (m.UserID == currentUserId) &&
                            m.LeaveDate.Month == monthReport.Month &&
                            (m.User.DisplayName.ToLower().Contains(searchValue)));
                    }
                }
                else
                {
                    leaves = _leaveService.GetAllLeaves(m =>
                        m.LeaveDate.Month == monthReport.Month &&
                        (m.User.DisplayName.ToLower().Contains(searchValue)));
                }
            }
            else
            {
                if (currentUserId > 0)
                {
                    if (isSupervisor)
                    {
                        leaves = _leaveService.GetAllLeaves(m =>
                            (m.UserID == currentUserId || m.User.DirectSupervisorID == currentUserId) &&
                            m.LeaveDate.Month == monthReport.Month);
                    }
                    else
                    {
                        leaves = _leaveService.GetAllLeaves(m =>
                            (m.UserID == currentUserId) &&
                            m.LeaveDate.Month == monthReport.Month);
                    }
                }
                else
                {
                    leaves = _leaveService.GetAllLeaves(m =>
                        m.LeaveDate.Month == monthReport.Month
                    );
                }
            }
            // ReSharper disable once AssignNullToNotNullAttribute
            var model = new LeaveMonthlyReport();
            model.Prepare(leaves);

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "Leaves":
                        model.UserMonthlyReports = model.UserMonthlyReports.OrderBy(s => s.Leaves).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "Lateness":
                        model.UserMonthlyReports = model.UserMonthlyReports.OrderBy(s => s.Lateness).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "Resignation":
                        model.UserMonthlyReports = model.UserMonthlyReports.OrderBy(s => s.Resignation).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "Total":
                        model.UserMonthlyReports = model.UserMonthlyReports.OrderBy(s => s.Total).ThenBy(s => s.User.ID).ToList();
                        break;
                    default:
                        model.UserMonthlyReports = model.UserMonthlyReports.OrderBy(s => s.User.DisplayName).ToList();
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "Leaves":
                        model.UserMonthlyReports = model.UserMonthlyReports.OrderByDescending(s => s.Leaves).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "Lateness":
                        model.UserMonthlyReports = model.UserMonthlyReports.OrderByDescending(s => s.Lateness).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "Resignation":
                        model.UserMonthlyReports = model.UserMonthlyReports.OrderByDescending(s => s.Resignation).ThenBy(s => s.User.ID).ToList();
                        break;
                    case "Total":
                        model.UserMonthlyReports = model.UserMonthlyReports.OrderByDescending(s => s.Total).ThenBy(s => s.User.ID).ToList();
                        break;
                    default:
                        model.UserMonthlyReports = model.UserMonthlyReports.OrderByDescending(s => s.User.DisplayName).ToList();
                        break;
                }
            }

            #endregion sort

            recordsTotal = model.UserMonthlyReports.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = model.UserMonthlyReports.Skip(skip).Take(pageSize).ToList();

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
                    m.Leaves,
                    m.Lateness,
                    m.Resignation,
                    m.Total,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

    }
}
