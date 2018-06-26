using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Hubs;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Utility;
using PQT.Web.Models;

namespace PQT.Web.Controllers
{
    public class EventController : Controller
    {

        //
        // GET: /SalesGroup/
        private readonly IEventService _repo;
        private readonly ICompanyRepository _comRepo;
        private readonly ILeadService _leadService;

        public EventController(IEventService repo, ICompanyRepository comRepo, ILeadService leadService)
        {
            _repo = repo;
            _comRepo = comRepo;
            _leadService = leadService;
        }

        [DisplayName(@"Event management")]
        public ActionResult Index()
        {
            var models = _repo.GetAllEvents();
            return View(models);
        }
        public ActionResult Detail(int id)
        {
            var model = new EventModel();
            model.PrepareEdit(id);
            return View(model);
        }

        public ActionResult Create()
        {
            var model = new EventModel();
            model.Prepare();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(EventModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Create())
                {
                    TempData["message"] = "Created successful";
                    return RedirectToAction("Index");
                }
            }
            model.Prepare();
            return View(model);
        }


        public ActionResult Edit(int id)
        {
            var model = new EventModel();
            model.PrepareEdit(id);
            if (model.Event == null)
            {
                TempData["error"] = "Data not found";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(EventModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Update())
                {
                    TempData["message"] = "Updated successful";
                    return RedirectToAction("Edit", new { id = model.Event.ID });
                }
            }
            model.PrepareEdit(model.Event.ID);
            return View(model);
        }
        public ActionResult Delete(int id)
        {
            if (_repo.DeleteEvent(id))
            {
                return Json(true);
            }
            return Json(false);
        }


        [AjaxOnly]
        public ActionResult AjaxGetCompanies()
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
            companies = _comRepo.GetAllCompanies(m =>
                (string.IsNullOrEmpty(companyName) || (!string.IsNullOrEmpty(m.CompanyName) && m.CompanyName.ToLower().Contains(companyName))) &&
                (string.IsNullOrEmpty(productService) || (!string.IsNullOrEmpty(m.ProductOrService) && m.ProductOrService.ToLower().Contains(productService))) &&
                (string.IsNullOrEmpty(countryName) || (!string.IsNullOrEmpty(m.CountryCode) && m.CountryCode.ToLower().Contains(countryName)) || 
                (!string.IsNullOrEmpty(m.CountryName) && m.CountryName.ToLower().Contains(countryName))) &&
                (string.IsNullOrEmpty(sector) || (!string.IsNullOrEmpty(m.Sector) && m.Sector.ToLower().Contains(sector))) &&
                (string.IsNullOrEmpty(industry) || (!string.IsNullOrEmpty(m.Industry) && m.Industry.ToLower().Contains(industry)))
               );



            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "Country":
                        companies = companies.OrderBy(s => s.CountryName).ThenBy(s => s.CompanyName);
                        break;
                    case "ProductOrService":
                        companies = companies.OrderBy(s => s.ProductOrService).ThenBy(s => s.CompanyName);
                        break;
                    case "Sector":
                        companies = companies.OrderBy(s => s.Sector).ThenBy(s => s.CompanyName);
                        break;
                    case "Industry":
                        companies = companies.OrderBy(s => s.Industry).ThenBy(s => s.CompanyName);
                        break;
                    default:
                        companies = companies.OrderBy(s => s.CompanyName);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "Country":
                        companies = companies.OrderByDescending(s => s.CountryName).ThenBy(s => s.CompanyName);
                        break;
                    case "ProductOrService":
                        companies = companies.OrderByDescending(s => s.ProductOrService).ThenBy(s => s.CompanyName);
                        break;
                    case "Sector":
                        companies = companies.OrderByDescending(s => s.Sector).ThenBy(s => s.CompanyName);
                        break;
                    case "Industry":
                        companies = companies.OrderByDescending(s => s.Industry).ThenBy(s => s.CompanyName);
                        break;
                    default:
                        companies = companies.OrderByDescending(s => s.CompanyName);
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
                    Country = m.CountryName,
                    m.CompanyName,
                    m.ProductOrService,
                    m.Sector,
                    m.Industry,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [DisplayName(@"End Event")]
        public ActionResult EndEvent(int id)
        {
            var ev = _repo.GetEvent(id);
            var leads = _leadService.GetAllLeads(m => m.EventID == id);
            var companyResources = _comRepo.GetAllCompanyResources(m => m.CompanyID != null && ev.Companies.Select(n => n.ID).Contains(Convert.ToInt32(m.CompanyID))).ToList();
            var count = 0;
            var totalCount = leads.Count();
            var userId = CurrentUser.Identity.ID;
            foreach (var lead in leads)
            {
                count++;
                var existResources =
                    companyResources.Where(
                        m => m.BusinessPhone == lead.BusinessPhone && m.MobilePhone == lead.MobilePhone);
                if (existResources.Any())
                {
                    foreach (var item in existResources)
                    {
                        item.CompanyID = lead.CompanyID;
                        item.CountryID = lead.Company.CountryID;
                        item.Country = lead.Company.CountryName;
                        item.FirstName = lead.FirstName;
                        item.LastName = lead.LastName;
                        item.Organisation = lead.CompanyName;
                        item.PersonalEmailAddress = lead.PersonalEmailAddress;
                        item.Role = lead.ClientName;
                        item.Salutation = lead.Salutation;
                        item.WorkEmailAddress = lead.WorkEmailAddress;
                        _comRepo.UpdateCompanyResource(item);
                    }
                }
                else
                {
                    var item = new CompanyResource()
                    {
                        BusinessPhone = lead.BusinessPhone,
                        CompanyID = lead.CompanyID,
                        CountryID = lead.Company.CountryID,
                        Country = lead.Company.CountryName,
                        FirstName = lead.FirstName,
                        LastName = lead.LastName,
                        MobilePhone = lead.MobilePhone,
                        Organisation = lead.CompanyName,
                        PersonalEmailAddress = lead.PersonalEmailAddress,
                        Role = lead.ClientName,
                        Salutation = lead.Salutation,
                        WorkEmailAddress = lead.WorkEmailAddress
                    };
                    _comRepo.CreateCompanyResource(item);
                }
                var json = new
                {
                    count,
                    totalCount
                };
                ProgressHub.SendMessage(userId, System.Web.Helpers.Json.Encode(json));
            }


            return Json(true);
        }


        [AjaxOnly]
        public ActionResult GetPossibleEvent(string q)
        {
            var bookings =
                _repo.GetAllEvents(m => (m.EventName != null && m.EventName.ToLower().Contains(q.ToLower())) ||
                (m.EventCode != null && m.EventCode.ToLower().Contains(q.ToLower())));
            return Json(bookings.Select(m => new { id = m.ID, text = m.EventCode + " - " + m.EventName }), JsonRequestBehavior.AllowGet);
        }
    }
}
