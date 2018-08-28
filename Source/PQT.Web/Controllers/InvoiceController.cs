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
using PQT.Web.Infrastructure.Utility;
using PQT.Web.Models;
using Resources;

namespace PQT.Web.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IBookingService bookingService, IInvoiceService invoiceService)
        {
            _bookingService = bookingService;
            _invoiceService = invoiceService;
        }

        //
        // GET: /Invoice/
        [DisplayName(@"Booking management")]
        public ActionResult Index(int id = 0)
        {
            return View();
        }

        public ActionResult Detail(int id = 0)
        {
            var model = new InvoiceModel();
            if (id > 0)
            {
                model.Invoice = _invoiceService.GetInvoice(id);
            }
            if (model.Invoice == null)
            {
                TempData["error"] = "Invoice not found";
                return RedirectToAction("Index");
            }
            return View(model);
        }
        public ActionResult Create(int id = 0)
        {
            var model = new InvoiceModel();
            model.Prepare(id);
            if (model.Booking == null)
            {
                TempData["error"] = "Booking not found";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(InvoiceModel model)
        {
            var invoice = _invoiceService.GetInvoiceByBooking(model.Invoice.BookingID);
            if (invoice != null)
            {
                TempData["error"] = "Booking has been invoiced";
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var flag = model.Create() != null;
                    if (flag)
                    {
                        TempData["message"] = Resource.SaveSuccessful;
                        return RedirectToAction("Index");
                    }
                    TempData["error"] = Resource.SaveError;
                }
                catch (Exception e)
                {
                    TempData["error"] = e.Message;
                }
            }
            model.Booking = _bookingService.GetBooking(model.Invoice.BookingID);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = new InvoiceModel();
            model.PrepareEdit(id);
            if (model.Invoice == null)
            {
                TempData["error"] = "Invoice not found";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(InvoiceModel model)
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
            model.Booking = _bookingService.GetBooking(model.Invoice.BookingID);
            return View(model);
        }
        [AjaxOnly]
        public ActionResult AjaxGetInvoices()
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
            //var saleId = PermissionHelper.SalesmanId();
            IEnumerable<Invoice> invoices = new HashSet<Invoice>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                invoices = _invoiceService.GetAllInvoices();
            }
            else
            {
                invoices = _invoiceService.GetAllInvoices();
            }
            // ReSharper disable once AssignNullToNotNullAttribute

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "InvoiceNo":
                        invoices = invoices.OrderBy(s => s.InvoiceNo).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        invoices = invoices.OrderBy(s => s.Booking.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "Amount":
                        invoices = invoices.OrderBy(s => s.TotalAmount).ThenBy(s => s.ID);
                        break;
                    case "InvoiceDate":
                        invoices = invoices.OrderBy(s => s.InvoiceDate).ThenBy(s => s.ID);
                        break;
                    case "EventCode":
                        invoices = invoices.OrderBy(s => s.Booking.Event.EventCode).ThenBy(s => s.ID);
                        break;
                    default:
                        invoices = invoices.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "InvoiceNo":
                        invoices = invoices.OrderByDescending(s => s.InvoiceNo).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        invoices = invoices.OrderByDescending(s => s.Booking.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "Amount":
                        invoices = invoices.OrderByDescending(s => s.TotalAmount).ThenBy(s => s.ID);
                        break;
                    case "InvoiceDate":
                        invoices = invoices.OrderByDescending(s => s.InvoiceDate).ThenBy(s => s.ID);
                        break;
                    case "EventCode":
                        invoices = invoices.OrderByDescending(s => s.Booking.Event.EventCode).ThenBy(s => s.ID);
                        break;
                    default:
                        invoices = invoices.OrderByDescending(s => s.ID);
                        break;
                }
            }

            #endregion sort

            recordsTotal = invoices.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = invoices.Skip(skip).Take(pageSize).ToList();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.InvoiceNo,
                    Company = m.Booking.CompanyName,
                    Amount = m.TotalAmount.ToString("N2"),
                    InvoiceDate = m.InvoiceDateStr,
                    EventCode = m.Booking.Event.EventCode,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }


    }
}
