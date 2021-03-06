using System;
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
        private readonly IRecruitmentService _recruitmentService;
        private readonly ILeadService _leadService;
        private readonly ILeadNewService _leadNewService;
        private readonly IEventService _eventService;
        private readonly IInvoiceService _invoiceService;
        private readonly IBookingService _bookingService;
        public ReportController(ILeadService leadService, IEventService eventService, ILeadNewService leadNewService, IInvoiceService invoiceService, IBookingService bookingService, IRecruitmentService recruitmentService)
        {
            _leadService = leadService;
            _eventService = eventService;
            _leadNewService = leadNewService;
            _invoiceService = invoiceService;
            _bookingService = bookingService;
            _recruitmentService = recruitmentService;
        }


        public ActionResult PrintConsolidateKpis(int eventId, int userId, string dfrom, string dto)
        {
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

            var model = new ConsolidateKPIModel { DateFrom = datefrom, DateTo = dateto };
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
            IEnumerable<LeadNew> leadNews = new HashSet<LeadNew>();
            IEnumerable<Lead> leads = new HashSet<Lead>();
            IEnumerable<Booking> bookings = new HashSet<Booking>();

            leads = _leadService.GetAllLeadsForKPI(eventId, userId, datefrom, dateto,null);
            leadNews = _leadNewService.GetAllLeadNewsForKPI(eventId, userId, datefrom, dateto, null);
            bookings = _bookingService.GetAllBookingsForKPI(eventId, userId, datefrom, dateto, null);
            model.Prepare(leads, leadNews);
            model.PrepareCalc(leads, leadNews, bookings);
            return View(model);
        }


        public ActionResult PrintHRKpis(int userId, string dfrom, string dto)
        {
            var model = new HRConsolidateKPIModel();
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
            IEnumerable<Candidate> candidates = new HashSet<Candidate>();
            candidates = _recruitmentService.GetAllCandidatesForKpis(null, userId, datefrom, dateto).AsEnumerable();
            model.Prepare(candidates);
            return View(model);
        }


        public ActionResult PrintInvoice(int id)
        {
            var invoice = _invoiceService.GetInvoice(id);
            return View(invoice);
        }

    }
}
