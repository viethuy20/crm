using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Models;

namespace PQT.Web.Controllers
{
    public class OperationController : Controller
    {
        private readonly IBookingService _bookingRepo;
        private readonly IEventService _eventRepo;

        public OperationController(IBookingService bookingRepo, IEventService eventRepo)
        {
            _bookingRepo = bookingRepo;
            _eventRepo = eventRepo;
        }
        //
        // GET: /Operation/

        [DisplayName(@"Event management")]
        public ActionResult Index()
        {
            return View(new List<Event>());
        }

        public ActionResult Detail(int id)
        {
            var model = new EventModel();
            model.PrepareEdit(id);
            return View(model);
        }


        [AjaxOnly]
        public ActionResult AjaxGetEventCompanies(int eventId)
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
            var companyName = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("CompanyName") != null && Request.Form.GetValues("CompanyName").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                companyName = Request.Form.GetValues("CompanyName").FirstOrDefault().Trim().ToLower();
            }
            var countryName = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("CountryName") != null && Request.Form.GetValues("CountryName").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                countryName = Request.Form.GetValues("CountryName").FirstOrDefault().Trim().ToLower();
            }
            var productService = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("ProductService") != null && Request.Form.GetValues("ProductService").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                productService = Request.Form.GetValues("ProductService").FirstOrDefault().Trim().ToLower();
            }
            var sector = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Sector") != null && Request.Form.GetValues("Sector").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                sector = Request.Form.GetValues("Sector").FirstOrDefault().Trim().ToLower();
            }
            var tier = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Tier") != null && Request.Form.GetValues("Tier").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                tier = Request.Form.GetValues("Tier").FirstOrDefault().Trim().ToLower();
            }
            var industry = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Industry") != null && Request.Form.GetValues("Industry").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                industry = Request.Form.GetValues("Industry").FirstOrDefault().Trim().ToLower();
            }


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            IEnumerable<Company> companies = new HashSet<Company>();
            companies = _bookingRepo.GetAllBookings(m => m.EventID == eventId && m.BookingStatusRecord == BookingStatus.Approved).Select(m => m.Company);
            companies = companies.Where(m =>
                m.EntityStatus == EntityStatus.Normal &&
                (string.IsNullOrEmpty(companyName) ||
                 (!string.IsNullOrEmpty(m.CompanyName) && m.CompanyName.ToLower().Contains(companyName))) &&
                (string.IsNullOrEmpty(productService) ||
                 (!string.IsNullOrEmpty(m.ProductOrService) &&
                  m.ProductOrService.ToLower().Contains(productService))) &&
                (string.IsNullOrEmpty(countryName) ||
                 (!string.IsNullOrEmpty(m.CountryCode) && m.CountryCode.ToLower().Contains(countryName)) ||
                 (!string.IsNullOrEmpty(m.CountryName) && m.CountryName.ToLower().Contains(countryName))) &&
                (string.IsNullOrEmpty(tier) || (m.Tier.ToString().Contains(tier))) &&
                (string.IsNullOrEmpty(sector) ||
                 (!string.IsNullOrEmpty(m.Sector) && m.Sector.ToLower().Contains(sector))) &&
                (string.IsNullOrEmpty(industry) ||
                 (!string.IsNullOrEmpty(m.Industry) && m.Industry.ToLower().Contains(industry)))
            );



            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "Country":
                        companies = companies.OrderBy(s => s.CountryCode).ThenBy(s => s.Tier);
                        break;
                    case "ProductOrService":
                        companies = companies.OrderBy(s => s.ProductOrService).ThenBy(s => s.Tier);
                        break;
                    case "Sector":
                        companies = companies.OrderBy(s => s.Sector).ThenBy(s => s.Tier);
                        break;
                    case "Industry":
                        companies = companies.OrderBy(s => s.Industry).ThenBy(s => s.Tier);
                        break;
                    case "CompanyName":
                        companies = companies.OrderBy(s => s.CompanyName).ThenBy(s => s.Tier);
                        break;
                    default:
                        companies = companies.OrderBy(s => s.Tier);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "Country":
                        companies = companies.OrderByDescending(s => s.CountryCode).ThenBy(s => s.Tier);
                        break;
                    case "ProductOrService":
                        companies = companies.OrderByDescending(s => s.ProductOrService).ThenBy(s => s.Tier);
                        break;
                    case "Sector":
                        companies = companies.OrderByDescending(s => s.Sector).ThenBy(s => s.Tier);
                        break;
                    case "Industry":
                        companies = companies.OrderByDescending(s => s.Industry).ThenBy(s => s.Tier);
                        break;
                    case "CompanyName":
                        companies = companies.OrderByDescending(s => s.CompanyName).ThenBy(s => s.Tier);
                        break;
                    default:
                        companies = companies.OrderByDescending(s => s.Tier);
                        break;
                }
            }


            recordsTotal = companies.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            IEnumerable<Company> data;
            if (pageSize < 1)
            {
                data = companies.Skip(skip).ToList();
            }
            else
            {
                data = companies.Skip(skip).Take(pageSize).ToList();
            }

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    Country = m.CountryCode,
                    m.CompanyName,
                    m.ProductOrService,
                    m.Sector,
                    m.Tier,
                    m.Industry,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

    }
}
