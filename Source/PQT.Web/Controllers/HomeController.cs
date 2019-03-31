﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using AutoMapper;
using PQT.Domain;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Models;
using iTextSharp.text.pdf.qrcode;
using PQT.Domain.Helpers;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using MultiLanguage;
using PQT.Domain.Abstract;
using PQT.Domain.Concrete;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Utility;
using Newtonsoft.Json;
using Ninject.Activation;
using NS.Mail;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using ServiceStack;

namespace PQT.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserNotificationService _notificationService;
        private readonly IEventService _eventService;
        private readonly IMembershipService _membershipService;

        public HomeController(IUserNotificationService notificationService, IEventService eventService, IMembershipService membershipService)
        {
            _notificationService = notificationService;
            _eventService = eventService;
            _membershipService = membershipService;
        }
        public ActionResult Index()
        {
            var model = new HomeModel();
            if (CurrentUser.HasRole("Finance"))
                return RedirectToAction("Index", "Event");
            if (CurrentUser.HasRole("Operation"))
                return RedirectToAction("Index", "Operation");
            if (CurrentUser.HasRole("HR"))
                return RedirectToAction("Index", "Recruitment");
            if (CurrentUser.HasRole("Admin") || CurrentUser.HasRole("QC") || CurrentUser.HasRole("Manager"))
                model.Events = _eventService.GetAllEventsForDashboard(0, "");
            else
            {
                var userId = CurrentUser.Identity.ID;
                var events = _eventService.GetAllEventsForDashboard(userId, "");
                model.Events = events.Where(m => (m.UserID == userId ||
                                                  m.SalesGroups.SelectMany(g => g.Users.Select(u => u.ID))
                                                      .Contains(userId) ||
                                                  m.SalesGroups
                                                      .SelectMany(g =>
                                                          g.Users.Where(u => u.TransferUserID > 0)
                                                              .Select(u => u.TransferUserID))
                                                      .Contains(userId) ||
                                                  m.ManagerUsers.Select(u => u.ID).Contains(userId) ||
                                                  m.ManagerUsers.Where(u => u.TransferUserID > 0)
                                                      .Select(u => u.TransferUserID).Contains(userId)));
                model.CrossSellEvents = events.Where(m => !model.Events.Select(e => e.ID).Contains(m.ID) &&
                                                          m.DateOfOpen <= DateTime.Today &&
                                                          DateTime.Today <= m.ClosingDate);
            }
            ViewBag.HomeSearch = model.HomeSearch;
            return View(model);
        }
        [HttpPost]
        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        public ActionResult Index(HomeModel model)
        {
            if (CurrentUser.HasRole("Finance"))
                return RedirectToAction("Index", "Event");
            if (CurrentUser.HasRole("Operation"))
                return RedirectToAction("Index", "Operation");
            if (CurrentUser.HasRole("HR"))
                return RedirectToAction("Index", "Recruitment");
            if (CurrentUser.HasRole("Admin") || CurrentUser.HasRole("QC") || CurrentUser.HasRole("Manager"))
                model.Events = _eventService.GetAllEventsForDashboard(0, model.HomeSearch.ToLower());
            else
            {
                var userId = CurrentUser.Identity.ID;
                var events = _eventService.GetAllEventsForDashboard(userId, model.HomeSearch.ToLower());
                model.Events = events.Where(m => (m.UserID == userId ||
                                                  m.SalesGroups.SelectMany(g => g.Users.Select(u => u.ID))
                                                      .Contains(userId) ||
                                                  m.SalesGroups
                                                      .SelectMany(g =>
                                                          g.Users.Where(u => u.TransferUserID > 0)
                                                              .Select(u => u.TransferUserID))
                                                      .Contains(userId) ||
                                                  m.ManagerUsers.Select(u => u.ID).Contains(userId) ||
                                                  m.ManagerUsers.Where(u => u.TransferUserID > 0)
                                                      .Select(u => u.TransferUserID).Contains(userId)));
                model.CrossSellEvents = events.Where(m => !model.Events.Select(e => e.ID).Contains(m.ID) &&
                                                          m.DateOfOpen <= DateTime.Today &&
                                                          DateTime.Today <= m.ClosingDate);
            }
            ViewBag.HomeSearch = model.HomeSearch;
            return View(model);
        }
        //[ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        //public ActionResult GetNotifyForEvent(int eventId, int page = 1)
        //{
        //    var notifications =
        //        _notificationService.GetAllUserNotificationsByEvent(CurrentUser.Identity.ID, eventId,
        //            Settings.System.NotificationNumber());
        //    return Json(notifications, JsonRequestBehavior.AllowGet);
        //}

        [AjaxOnly]
        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        public ActionResult GetNotifyForEvents(string eventIds)
        {
            if (string.IsNullOrEmpty(eventIds))
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
            var ids = eventIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(m => Convert.ToInt32(m));
            var notifications = new List<UserNotification>();
            foreach (var id in ids)
            {
                notifications.AddRange(_notificationService.GetAllUserNotificationsByEvent(CurrentUser.Identity.ID, id, 50));
            }
            return Json(notifications, JsonRequestBehavior.AllowGet);
        }
        [AjaxOnly]
        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        public ActionResult GetNotifyForNewEvent()
        {
            var notifications =
                _notificationService.GetAllUserNotificationsByNewEvent(CurrentUser.Identity.ID, Settings.System.NotificationNumber());
            return Json(notifications, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        public ActionResult PanelNotification(string type = "")
        {
            var notify = new List<UserNotification>();
            if (CurrentUser.Identity != null)
            {
                notify = _notificationService.GetAllUserNotifications(CurrentUser.Identity.ID, type, Settings.System.NotificationNumber()).ToList();
            }
            return PartialView(notify);
        }

        [AjaxOnly]
        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        public ActionResult RemoveNotifyCounter(string type = "")
        {
            if (CurrentUser.Identity != null)
            {
                if (type == NotifyType.Booking)
                {
                    if (CurrentUser.Identity.BookingNotifyNumber > 0)
                    {
                        CurrentUser.Identity.BookingNotifyNumber = 0;
                        _membershipService.UpdateUser(CurrentUser.Identity);
                    }
                }
                else if (type == NotifyType.Invoice)
                {
                    if (CurrentUser.Identity.InvoiceNotifyNumber > 0)
                    {
                        CurrentUser.Identity.InvoiceNotifyNumber = 0;
                        _membershipService.UpdateUser(CurrentUser.Identity);
                    }
                }
                else if (type == NotifyType.Recruitment)
                {
                    if (CurrentUser.Identity.RecruitmentNotifyNumber > 0)
                    {
                        CurrentUser.Identity.RecruitmentNotifyNumber = 0;
                        _membershipService.UpdateUser(CurrentUser.Identity);
                    }
                }
                else if (type == NotifyType.OpeEvent)
                {
                    if (CurrentUser.Identity.OpeEventNotifyNumber > 0)
                    {
                        CurrentUser.Identity.OpeEventNotifyNumber = 0;
                        _membershipService.UpdateUser(CurrentUser.Identity);
                    }
                }
                else if (type == NotifyType.NewEvent)
                {
                    if (CurrentUser.Identity.NewEventNotifyNumber > 0)
                    {
                        CurrentUser.Identity.NewEventNotifyNumber = 0;
                        _membershipService.UpdateUser(CurrentUser.Identity);
                    }
                }
                else if (type == NotifyType.MasterFiles)
                {
                    if (CurrentUser.Identity.MasterFilesNotifyNumber > 0)
                    {
                        CurrentUser.Identity.MasterFilesNotifyNumber = 0;
                        _membershipService.UpdateUser(CurrentUser.Identity);
                    }
                }
                else if (type == NotifyType.ReportCall)
                {
                    if (CurrentUser.Identity.ReportCallNotifyNumber > 0)
                    {
                        CurrentUser.Identity.ReportCallNotifyNumber = 0;
                        _membershipService.UpdateUser(CurrentUser.Identity);
                    }
                }
                else if (type == NotifyType.Leave)
                {
                    if (CurrentUser.Identity.LeaveNotifyNumber > 0)
                    {
                        CurrentUser.Identity.LeaveNotifyNumber = 0;
                        _membershipService.UpdateUser(CurrentUser.Identity);
                    }
                }
                else
                {
                    if (CurrentUser.Identity.NotifyNumber > 0)
                    {
                        CurrentUser.Identity.NotifyNumber = 0;
                        _membershipService.UpdateUser(CurrentUser.Identity);
                    }
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        public ActionResult UpdateSeenNotify(int entryId, NotifyType type)
        {
            if (CurrentUser.Identity != null)
            {
                var countSeen = _notificationService.SeenUserNotification(CurrentUser.Identity.ID, entryId, type);
                if (countSeen > 0)
                {
                    if (type == NotifyType.Booking)
                    {
                        CurrentUser.Identity.BookingNotifyNumber = CurrentUser.Identity.BookingNotifyNumber - countSeen;
                        if (CurrentUser.Identity.BookingNotifyNumber < 0)
                        {
                            CurrentUser.Identity.BookingNotifyNumber = 0;
                        }
                    }
                    else if (type == NotifyType.Invoice)
                    {
                        CurrentUser.Identity.InvoiceNotifyNumber = CurrentUser.Identity.InvoiceNotifyNumber - countSeen;
                        if (CurrentUser.Identity.InvoiceNotifyNumber < 0)
                        {
                            CurrentUser.Identity.InvoiceNotifyNumber = 0;
                        }
                    }
                    else if (type == NotifyType.Recruitment)
                    {
                        CurrentUser.Identity.RecruitmentNotifyNumber = CurrentUser.Identity.RecruitmentNotifyNumber - countSeen;
                        if (CurrentUser.Identity.RecruitmentNotifyNumber < 0)
                        {
                            CurrentUser.Identity.RecruitmentNotifyNumber = 0;
                        }
                    }
                    else if (type == NotifyType.OpeEvent)
                    {
                        CurrentUser.Identity.OpeEventNotifyNumber = CurrentUser.Identity.OpeEventNotifyNumber - countSeen;
                        if (CurrentUser.Identity.OpeEventNotifyNumber < 0)
                        {
                            CurrentUser.Identity.OpeEventNotifyNumber = 0;
                        }
                    }
                    else if (type == NotifyType.NewEvent)
                    {
                        CurrentUser.Identity.NewEventNotifyNumber = CurrentUser.Identity.NewEventNotifyNumber - countSeen;
                        if (CurrentUser.Identity.NewEventNotifyNumber < 0)
                        {
                            CurrentUser.Identity.NewEventNotifyNumber = 0;
                        }
                    }
                    else if (type == NotifyType.MasterFiles)
                    {
                        CurrentUser.Identity.MasterFilesNotifyNumber = CurrentUser.Identity.MasterFilesNotifyNumber - countSeen;
                        if (CurrentUser.Identity.MasterFilesNotifyNumber < 0)
                        {
                            CurrentUser.Identity.MasterFilesNotifyNumber = 0;
                        }
                    }
                    else if (type == NotifyType.ReportCall)
                    {
                        CurrentUser.Identity.ReportCallNotifyNumber = CurrentUser.Identity.ReportCallNotifyNumber - countSeen;
                        if (CurrentUser.Identity.ReportCallNotifyNumber < 0)
                        {
                            CurrentUser.Identity.ReportCallNotifyNumber = 0;
                        }
                    }
                    else if (type == NotifyType.Leave)
                    {
                        CurrentUser.Identity.LeaveNotifyNumber = CurrentUser.Identity.LeaveNotifyNumber - countSeen;
                        if (CurrentUser.Identity.LeaveNotifyNumber < 0)
                        {
                            CurrentUser.Identity.LeaveNotifyNumber = 0;
                        }
                    }
                    else
                    {
                        CurrentUser.Identity.NotifyNumber = CurrentUser.Identity.NotifyNumber - countSeen;
                        if (CurrentUser.Identity.NotifyNumber < 0)
                        {
                            CurrentUser.Identity.NotifyNumber = 0;
                        }
                    }
                    _membershipService.UpdateUser(CurrentUser.Identity);
                }
                return Json(countSeen, JsonRequestBehavior.AllowGet);
            }
            return Json(0, JsonRequestBehavior.AllowGet);
        }
        [AjaxOnly]
        [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
        public ActionResult SeenNotify(int notifyId)
        {
            _notificationService.SeenUserNotification(notifyId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [AjaxOnly]
        public ActionResult TestMail()
        {
            try
            {

                var record = new Audit
                {
                    ID = 1,
                    Username = "Test Mail",
                    Email = "Test Mail",
                    IPAddress = "Test Mail",
                    UrlAccessed = "Test Mail",
                    TimeAccessed = DateTime.Now,
                    SessionId = "Test Mail",
                    Message = "Test Mail",
                    Data = "Test Mail",
                    Type = (int)AuditType.Exception,
                    ActionId = 0
                };
                string subject = "Test Mail";
                var message = new RazorMailMessage("Logs/Exception", record, new { DomainRoot = Infrastructure.Helpers.UrlHelper.Root }).Render();
                var receiveEmail = ConfigurationManager.AppSettings["LogsEmail"];
                if (receiveEmail != null && !string.IsNullOrEmpty(receiveEmail))
                {
                    var emails = receiveEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    EmailHelper.SendEmail(emails, subject, message);
                }
                return Json("successful", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }
        [AjaxOnly]
        public ActionResult MakeExpiredLead()
        {
            LeadHelper.MakeExpiredLead();
            return Json("successful", JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult UpdateActiveKey(string key)
        {
            try
            {
                PermissionHelper.AddOrUpdateAppSettings("LicenceKey", key);
                return Json("successful", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
