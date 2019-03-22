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
            IEnumerable<LeadNew> leadNews = new HashSet<LeadNew>();
            IEnumerable<Lead> leads = new HashSet<Lead>();
            IEnumerable<Booking> bookings = new HashSet<Booking>();

            leads = _leadService.GetAllLeadsForKPI(eventId, userId, m =>
                datefrom <= m.CreatedTime.Date &&
                m.CreatedTime.Date <= dateto
            );
            leadNews = _leadNewService.GetAllLeadNewsForKPI(eventId, userId, m =>
                m.AssignUserID > 0 &&
                datefrom <= m.FirstAssignDate.Date &&
                m.FirstAssignDate.Date <= dateto
            );
            bookings = _bookingService.GetAllBookingsForKPI(eventId, userId, m =>
                m.BookingStatusRecord.Status.Value == BookingStatus.Approved.Value &&
                datefrom <= m.BookingDate.Date &&
                m.BookingDate.Date <= dateto
            );
            model.Prepare(leads, leadNews, bookings);
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
            if (userId > 0)
            {
                candidates = _recruitmentService.GetAllCandidates(m =>
                    m.CandidateStatusRecord.Status.Value != CandidateStatus.Deleted.Value &&
                    datefrom <= m.CreatedTime.Date &&
                    m.CreatedTime.Date <= dateto
                );
            }
            else
            {
                candidates = _recruitmentService.GetAllCandidates(m =>
                    m.CandidateStatusRecord.Status.Value != CandidateStatus.Deleted.Value &&
                    datefrom <= m.CreatedTime.Date &&
                    m.CreatedTime.Date <= dateto
                );
            }
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
