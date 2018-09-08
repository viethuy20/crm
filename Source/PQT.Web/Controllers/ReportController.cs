﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Models;
using PQT.Web.Infrastructure.Filters;

namespace PQT.Web.Controllers
{
    [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
    public class ReportController : Controller
    {
        private readonly IUnitRepository _unitRepo;
        private readonly ILeadService _leadService;
        private readonly IEventService _eventService;
        public ReportController(IUnitRepository unitRepo, ILeadService leadService, IEventService eventService)
        {
            _unitRepo = unitRepo;
            _leadService = leadService;
            _eventService = eventService;
        }


        public ActionResult PrintConsolidateKpis(int eventId, int userId, string dfrom, string dto)
        {
            var model = new ConsolidateKPIModel();
            var datefrom = default(DateTime);
            if (!string.IsNullOrEmpty(dfrom))
            {
                datefrom = Convert.ToDateTime(dfrom);
            }
            var dateto = default(DateTime);
            if (!string.IsNullOrEmpty(dto))
            {
                dateto = Convert.ToDateTime(dto);
            }
            if (!string.IsNullOrEmpty(dfrom) && !string.IsNullOrEmpty(dto))
            {
                model.Date = dfrom + " - " + dto;
            }
            else if (!string.IsNullOrEmpty(dfrom) && string.IsNullOrEmpty(dto))
            {
                model.Date = dfrom + " - " + DateTime.Today.ToString("dd/MM/yyyy");
            }
            else if (string.IsNullOrEmpty(dfrom) && !string.IsNullOrEmpty(dto))
            {
                model.Date = "All - " + dto;
            }
            else
            {
                model.Date = "All";
            }
            if (eventId > 0)
            {
                var eventData = _eventService.GetEvent(eventId);
                if (eventData != null)
                    model.EventName = eventData.EventName + " (" + eventData.EventCode + ")";
            }
            IEnumerable<Lead> leads = new HashSet<Lead>();
            leads = _leadService.GetAllLeads(m =>
                (m.LeadStatusRecord != LeadStatus.Reject &&
                 m.LeadStatusRecord != LeadStatus.Initial &&
                 m.LeadStatusRecord != LeadStatus.Deleted) &&
                (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                (eventId == 0 || m.EventID == eventId) &&
                (userId == 0 || m.UserID == userId || (m.User != null && m.User.TransferUserID == userId))
            );
            model.Prepare(leads);
            return View(model);
        }
    }
}
