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
                    return RedirectToAction("AssignCompany", new { id = model.Event.ID });
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

        [DisplayName(@"Assign Company")]
        public ActionResult AssignCompany(int id)
        {
            var model = new EventModel();
            model.PrepareAssign(id);
            if (model.Event == null)
            {
                TempData["error"] = "Data not found";
                return RedirectToAction("Index");
            }
            return View(model);
        }
        [DisplayName(@"Assign Company")]
        [HttpPost]
        public ActionResult AssignCompany(EventModel model)
        {
            if (model.AssignCompany())
            {
                TempData["message"] = "Assign successful";
                return RedirectToAction("AssignCompany", new { id = model.ID });
            }
            model.PrepareAssign(model.ID);
            if (model.Event == null)
            {
                TempData["error"] = "Data not found";
                return RedirectToAction("Index");
            }
            return View(model);
        }
        [AjaxOnly]
        public ActionResult AjaxGetAssignCompanies(int type = 0)
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

            var saleIds = new List<int>();
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("SaleIds") != null && !string.IsNullOrEmpty(Request.Form.GetValues("SaleIds").FirstOrDefault()))
            {
                // ReSharper disable once PossibleNullReferenceException
                saleIds = Request.Form.GetValues("SaleIds").FirstOrDefault().ToString()
                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(m => Convert.ToInt32(m)).ToList();
            }


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            IEnumerable<Company> companies = new HashSet<Company>();
            Func<Company, bool> predicate = m =>
                (m.Tier == type) &&
                (type != 1 || !m.ManagerUsers.Any() || m.ManagerUsers.Any(s => saleIds.Contains(s.ID))) &&
                (string.IsNullOrEmpty(companyName) ||
                 (!string.IsNullOrEmpty(m.CompanyName) && m.CompanyName.ToLower().Contains(companyName))) &&
                (string.IsNullOrEmpty(productService) || (!string.IsNullOrEmpty(m.ProductOrService) &&
                                                          m.ProductOrService.ToLower().Contains(productService))) &&
                (string.IsNullOrEmpty(countryName) ||
                 (m.Country != null && m.Country.Code.ToLower().Contains(countryName)) ||
                 (m.Country != null && m.Country.Name.ToLower().Contains(countryName))) &&
                (string.IsNullOrEmpty(sector) ||
                 (!string.IsNullOrEmpty(m.Sector) && m.Sector.ToLower().Contains(sector))) &&
                (string.IsNullOrEmpty(industry) ||
                 (!string.IsNullOrEmpty(m.Industry) && m.Industry.ToLower().Contains(industry)));
            companies = _comRepo.GetAllCompanies(predicate).ToList();

            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CountryName":
                        companies = companies.OrderBy(s => s.Country.Name).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    case "ProductOrService":
                        companies = companies.OrderBy(s => s.ProductOrService).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    case "Sector":
                        companies = companies.OrderBy(s => s.Sector).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    case "Industry":
                        companies = companies.OrderBy(s => s.Industry).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    case "Tier":
                        companies = companies.OrderBy(s => s.Tier).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    case "BusinessUnit":
                        companies = companies.OrderBy(s => s.BusinessUnit).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    case "FinancialYear":
                        companies = companies.OrderBy(s => s.FinancialYear).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    default:
                        companies = companies.OrderBy(s => s.CompanyName).AsEnumerable();
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CountryName":
                        companies = companies.OrderByDescending(s => s.Country.Name).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    case "ProductOrService":
                        companies = companies.OrderByDescending(s => s.ProductOrService).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    case "Sector":
                        companies = companies.OrderByDescending(s => s.Sector).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    case "Industry":
                        companies = companies.OrderByDescending(s => s.Industry).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    case "Tier":
                        companies = companies.OrderByDescending(s => s.Tier).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    case "BusinessUnit":
                        companies = companies.OrderByDescending(s => s.BusinessUnit).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    case "FinancialYear":
                        companies = companies.OrderByDescending(s => s.FinancialYear).ThenBy(s => s.CompanyName).AsEnumerable();
                        break;
                    default:
                        companies = companies.OrderByDescending(s => s.CompanyName).AsEnumerable();
                        break;
                }
            }

            recordsTotal = companies.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = companies.Skip(skip).Take(pageSize).ToList();

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
                    m.Tier,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [DisplayName(@"End Event")]
        public ActionResult EndEvent(int id)
        {
            var eve = _repo.GetEvent(id);
            return PartialView(eve);
        }
        [HttpPost]
        [DisplayName(@"End Event")]
        public ActionResult EndEvent(int ID, EventStatus EventStatus)
        {
            var ev = _repo.GetEvent(ID);
            if (ev == null)
            {
                return Json(false);
            }
            ev.EventStatus = EventStatus;
            if (_repo.UpdateEvent(ev))
            {
                if (EventStatus != EventStatus.Completed)
                {
                    return Json(true);
                }
                var leads = _leadService.GetAllLeads(m => m.EventID == ID);
                var companyResources = _comRepo.GetAllCompanyResources(m => m.CompanyID != null &&
                ev.EventCompanies.Select(n => n.CompanyID).Contains(Convert.ToInt32(m.CompanyID))).ToList();
                var count = 0;
                var totalCount = leads.Count();
                var userId = CurrentUser.Identity.ID;
                foreach (var lead in leads)
                {
                    count++;
                    var existResources =
                        companyResources.Where(
                            m => m.WorkEmail == lead.WorkEmail &&
                                 m.MobilePhone1 == lead.MobilePhone1 &&
                                 m.MobilePhone2 == lead.MobilePhone2 &&
                                 m.MobilePhone3 == lead.MobilePhone3);
                    var eventCompany = _repo.GetEventCompany(lead.EventID, lead.CompanyID);
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
                            item.PersonalEmail = lead.PersonalEmail;
                            item.Role = lead.JobTitle;
                            item.Salutation = lead.Salutation;
                            item.WorkEmail = lead.WorkEmail;
                            item.MobilePhone1 = lead.MobilePhone1;
                            item.MobilePhone2 = lead.MobilePhone2;
                            item.MobilePhone3 = lead.MobilePhone3;
                            if (eventCompany != null)
                            {
                                item.BusinessUnit = eventCompany.BusinessUnit;
                                item.BudgetMonth = eventCompany.BudgetMonth;
                                item.Remarks = eventCompany.Remarks;
                            }
                            _comRepo.UpdateCompanyResource(item);
                        }
                    }
                    else
                    {
                        var item = new CompanyResource()
                        {
                            CompanyID = lead.CompanyID,
                            CountryID = lead.Company.CountryID,
                            Country = lead.Company.CountryName,
                            FirstName = lead.FirstName,
                            LastName = lead.LastName,
                            MobilePhone1 = lead.MobilePhone1,
                            MobilePhone2 = lead.MobilePhone2,
                            MobilePhone3 = lead.MobilePhone3,
                            Organisation = lead.CompanyName,
                            PersonalEmail = lead.PersonalEmail,
                            Role = lead.JobTitle,
                            Salutation = lead.Salutation,
                            WorkEmail = lead.WorkEmail
                        };
                        if (eventCompany != null)
                        {
                            item.BusinessUnit = eventCompany.BusinessUnit;
                            item.BudgetMonth = eventCompany.BudgetMonth;
                            item.Remarks = eventCompany.Remarks;
                        }
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
            return Json(false);
        }


        [AjaxOnly]
        public ActionResult GetPossibleEvent(string q)
        {
            var bookings =
                _repo.GetAllEvents(m => (m.EventName != null && m.EventName.ToLower().Contains(q.ToLower())) ||
                (m.EventCode != null && m.EventCode.ToLower().Contains(q.ToLower())));
            return Json(bookings.Select(m => new { id = m.ID, text = m.EventCode + " - " + m.EventName }), JsonRequestBehavior.AllowGet);
        }


        [AjaxOnly]
        public ActionResult AjaxGetEventAlls()
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

            IEnumerable<Event> events = new HashSet<Event>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                if (!string.IsNullOrEmpty(searchValue))
                    events = _repo.GetAllEvents(m =>
                        m.EventCode.ToLower().Contains(searchValue) ||
                        m.EventName.ToLower().Contains(searchValue) ||
                        m.StartDate.ToString("dd/MM/yyyy").Contains(searchValue) ||
                        m.EndDate.ToString("dd/MM/yyyy").Contains(searchValue) ||
                        m.DateOfConfirmationStr.Contains(searchValue) ||
                        m.ClosingDateStr.ToLower().Contains(searchValue) ||
                        m.EventStatusDisplay.ToLower().Contains(searchValue) ||
                        (m.Location != null && m.Location.ToLower().Contains(searchValue))
                       );
            }
            else
            {
                events = _repo.GetAllEvents();
            }

            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "EventCode":
                        events = events.OrderBy(s => s.EventCode).ThenBy(s => s.ID);
                        break;
                    case "EventName":
                        events = events.OrderBy(s => s.EventName).ThenBy(s => s.ID);
                        break;
                    case "EventStatusDisplay":
                        events = events.OrderBy(s => s.EventStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "StartDate":
                        events = events.OrderBy(s => s.StartDate).ThenBy(s => s.ID);
                        break;
                    case "EndDate":
                        events = events.OrderBy(s => s.EndDate).ThenBy(s => s.ID);
                        break;
                    case "DateOfConfirmation":
                        events = events.OrderBy(s => s.DateOfConfirmation).ThenBy(s => s.ID);
                        break;
                    case "ClosingDate":
                        events = events.OrderBy(s => s.ClosingDate).ThenBy(s => s.ID);
                        break;
                    case "Location":
                        events = events.OrderBy(s => s.Location).ThenBy(s => s.ID);
                        break;
                    default:
                        events = events.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "EventCode":
                        events = events.OrderByDescending(s => s.EventCode).ThenBy(s => s.ID);
                        break;
                    case "EventName":
                        events = events.OrderByDescending(s => s.EventName).ThenBy(s => s.ID);
                        break;
                    case "EventStatusDisplay":
                        events = events.OrderByDescending(s => s.EventStatusDisplay).ThenBy(s => s.ID);
                        break;
                    case "StartDate":
                        events = events.OrderByDescending(s => s.StartDate).ThenBy(s => s.ID);
                        break;
                    case "EndDate":
                        events = events.OrderByDescending(s => s.EndDate).ThenBy(s => s.ID);
                        break;
                    case "DateOfConfirmation":
                        events = events.OrderByDescending(s => s.DateOfConfirmation).ThenBy(s => s.ID);
                        break;
                    case "ClosingDate":
                        events = events.OrderByDescending(s => s.ClosingDate).ThenBy(s => s.ID);
                        break;
                    case "Location":
                        events = events.OrderByDescending(s => s.Location).ThenBy(s => s.ID);
                        break;
                    default:
                        events = events.OrderByDescending(s => s.ID);
                        break;
                }
            }


            recordsTotal = events.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = events.Skip(skip).Take(pageSize).ToList();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.EventCode,
                    m.EventName,
                    m.EventStatusDisplay,
                    m.BackgroundColor,
                    m.Location,
                    StartDate = m.StartDate.ToString("dd/MM/yyyy"),
                    EndDate = m.EndDate.ToString("dd/MM/yyyy"),
                    DateOfConfirmation = m.DateOfConfirmationStr,
                    ClosingDate = m.ClosingDateStr
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
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
            var eventData = _repo.GetEvent(eventId);
            if (eventData != null)
            {
                companies = eventData.EventCompanies.Select(m=>m.Company).Where(m =>
                       (string.IsNullOrEmpty(companyName) || (!string.IsNullOrEmpty(m.CompanyName) && m.CompanyName.ToLower().Contains(companyName))) &&
                       (string.IsNullOrEmpty(productService) || (!string.IsNullOrEmpty(m.ProductOrService) && m.ProductOrService.ToLower().Contains(productService))) &&
                       (string.IsNullOrEmpty(countryName) || (!string.IsNullOrEmpty(m.CountryCode) && m.CountryCode.ToLower().Contains(countryName)) ||
                       (!string.IsNullOrEmpty(m.CountryName) && m.CountryName.ToLower().Contains(countryName))) && 
                       (string.IsNullOrEmpty(tier) || (m.Tier.ToString().Contains(tier))) &&
                       (string.IsNullOrEmpty(sector) || (!string.IsNullOrEmpty(m.Sector) && m.Sector.ToLower().Contains(sector))) &&
                       (string.IsNullOrEmpty(industry) || (!string.IsNullOrEmpty(m.Industry) && m.Industry.ToLower().Contains(industry)))
                   );
            }



            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "Country":
                        companies = companies.OrderBy(s => s.CountryName).ThenBy(s => s.Tier);
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
                        companies = companies.OrderByDescending(s => s.CountryName).ThenBy(s => s.Tier);
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
                    Country = m.CountryName,
                    m.CompanyName,
                    m.ProductOrService,
                    m.Sector,
                    m.Tier,
                    m.Industry,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult LoadEventSalesRules(int eventId)
        {
            return PartialView(_repo.GetEvent(eventId));
        }

    }
}
