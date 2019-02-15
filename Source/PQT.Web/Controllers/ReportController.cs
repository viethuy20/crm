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
            leads = _leadService.GetAllLeads(m =>
                (m.LeadStatusRecord != LeadStatus.Reject &&
                 m.LeadStatusRecord != LeadStatus.Initial &&
                 m.LeadStatusRecord != LeadStatus.Deleted) &&
                (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                (eventId == 0 || m.EventID == eventId) &&
                (userId == 0 || m.UserID == userId || (m.User != null && m.User.TransferUserID == userId))
            );
            leadNews = _leadNewService.GetAllLeadNews(m =>
                (m.AssignUserID > 0) &&
                (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                (eventId == 0 || m.EventID == eventId) &&
                (userId == 0 || m.UserID == userId || (m.User != null && m.User.TransferUserID == userId))
            );
            bookings = _bookingService.GetAllBookings(m =>
                m.BookingStatusRecord == BookingStatus.Approved &&
                (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                (eventId == 0 || m.EventID == eventId) &&
                (userId == 0 || m.SalesmanID == userId || (m.Salesman != null && m.Salesman.TransferUserID == userId))
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
            candidates = _recruitmentService.GetAllCandidates(m =>
                (m.CandidateStatusRecord != CandidateStatus.Deleted) &&
                (datefrom == default(DateTime) || m.CreatedTime.Date >= datefrom.Date) &&
                (dateto == default(DateTime) || m.CreatedTime.Date <= dateto.Date) &&
                (userId == 0 || m.UserID == userId || (m.User != null && m.User.TransferUserID == userId))
            );
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
