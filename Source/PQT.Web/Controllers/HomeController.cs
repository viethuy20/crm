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
        private readonly IMembershipService _memRepository;
        private readonly IEventService _eventService;

        public HomeController(IMembershipService memRepository, IEventService eventService)
        {
            _memRepository = memRepository;
            _eventService = eventService;
        }

        public ActionResult Index()
        {
            var model = new HomeModel();
            if (CurrentUser.HasRole("Finance") || CurrentUser.HasRole("Admin") || CurrentUser.HasRole("QC")|| CurrentUser.HasRole("Manager"))
                model.Events = _eventService.GetAllEvents();
            else
            {
                var userId = CurrentUser.Identity.ID;
                model.Events = _eventService.GetAllEvents().Where(m =>
                    m.UserID == userId ||
                    m.SalesGroups.SelectMany(g => g.Users.Select(u => u.ID)).Contains(userId) ||
                        m.ManagerUsers.Select(u => u.ID).Contains(userId) || (m.EndDate < DateTime.Today && DateTime.Today <= m.ClosingDate));
            }
            return View(model);
        }

        public ActionResult GetNotifyForEvent(int eventId, int page = 1)
        {
            var notifications =
                _memRepository.GetAllUserNotificationsByEvent(CurrentUser.Identity.ID, eventId,
                    Settings.System.NotificationNumber());
            return Json(notifications, JsonRequestBehavior.AllowGet);
        }


        [AjaxOnly]
        public ActionResult PanelNotification()
        {
            var notify = new List<UserNotification>();
            if (CurrentUser.Identity != null)
            {
                notify = _memRepository.GetAllUserNotifications(CurrentUser.Identity.ID, Settings.System.NotificationNumber()).ToList();
            }
            return PartialView(notify);
        }

        [AjaxOnly]
        public ActionResult RemoveNotifyCounter()
        {
            if (CurrentUser.Identity != null)
            {
                CurrentUser.Identity.NotifyNumber = 0;
                _memRepository.UpdateUser(CurrentUser.Identity);
            }
            return Json(true);
        }

        [AjaxOnly]
        public ActionResult UpdateSeenNotify(int entryId, NotifyType type)
        {
            if (CurrentUser.Identity != null)
            {
                var countSeen = _memRepository.SeenUserNotification(CurrentUser.Identity.ID, entryId, type);
                if (countSeen > 0)
                {
                    CurrentUser.Identity.NotifyNumber = CurrentUser.Identity.NotifyNumber - countSeen;
                    if (CurrentUser.Identity.NotifyNumber < 0)
                    {
                        CurrentUser.Identity.NotifyNumber = 0;
                    }
                    _memRepository.UpdateUser(CurrentUser.Identity);
                }
                return Json(countSeen);
            }
            return Json(0);
        }
        [AjaxOnly]
        public ActionResult SeenNotify(int notifyId)
        {
            _memRepository.SeenUserNotification(notifyId);
            return Json(true);
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
    }
}
