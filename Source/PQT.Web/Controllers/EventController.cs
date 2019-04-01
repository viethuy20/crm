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
using PQT.Web.Hubs;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
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
        private readonly IUnitRepository _unitRepository;
        private readonly IBookingService _bookingService;

        public EventController(IEventService repo, ICompanyRepository comRepo, ILeadService leadService, IUnitRepository unitRepository, IBookingService bookingService)
        {
            _repo = repo;
            _comRepo = comRepo;
            _leadService = leadService;
            _unitRepository = unitRepository;
            _bookingService = bookingService;
        }

        [DisplayName(@"Event management")]
        public ActionResult Index()
        {
            //var models = _repo.GetAllEvents();
            return View(new List<Event>());
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
        [HttpPost]
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
            var countries = countryName.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim());
            var productServices = productService.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim());
            var sectors = sector.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim());
            var industries = industry.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim());
            int recordsTotal = _comRepo.GetCountCompaniesForAssignEvent(type, saleIds.ToArray(), companyName, countries.ToArray(), productServices.ToArray(), sectors.ToArray(), industries.ToArray());
            var data = _comRepo.GetAllCompaniesForAssignEvent(type, saleIds.ToArray(), companyName, countries.ToArray(), productServices.ToArray(), sectors.ToArray(), industries.ToArray(), sortColumnDir, sortColumn, skip, pageSize);
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
                var leads = _leadService.GetAllLeadsByEvent(ID);
                var companyResources = _comRepo.GetAllCompanyResources(ev.EventCompanies.Select(n => n.CompanyID).Distinct().ToArray()).ToList();
                var count = 0;
                var totalCount = leads.Count();
                var userId = CurrentUser.Identity.ID;
                foreach (var lead in leads)
                {
                    count++;
                    var existResources =
                        companyResources.Where(
                            m =>
                                m.Role == lead.JobTitle && (
                                (!string.IsNullOrEmpty(m.DirectLine) && !string.IsNullOrEmpty(lead.DirectLine) &&
                                 m.DirectLine == lead.DirectLine) ||
                                (!string.IsNullOrEmpty(m.MobilePhone1) && !string.IsNullOrEmpty(lead.MobilePhone1) &&
                                 m.MobilePhone1 == lead.MobilePhone1) ||
                                (!string.IsNullOrEmpty(m.MobilePhone2) && !string.IsNullOrEmpty(lead.MobilePhone2) &&
                                 m.MobilePhone2 == lead.MobilePhone2) ||
                                (!string.IsNullOrEmpty(m.MobilePhone3) && !string.IsNullOrEmpty(lead.MobilePhone3) &&
                                 m.MobilePhone3 == lead.MobilePhone3))).ToList();
                    //var eventCompany = _repo.GetEventCompany(lead.EventID, lead.CompanyID);
                    if (existResources.Any())
                    {
                        foreach (var item in existResources)
                        {
                            item.CompanyID = lead.CompanyID;
                            item.CountryID = lead.Company.CountryID;
                            item.Country = lead.Company.CountryCode;
                            item.FirstName = lead.FirstName;
                            item.LastName = lead.LastName;
                            item.Organisation = lead.CompanyName;
                            item.PersonalEmail = lead.PersonalEmail;
                            item.Role = lead.JobTitle;
                            item.Salutation = lead.Salutation;
                            item.WorkEmail = lead.WorkEmail;
                            item.DirectLine = lead.DirectLine;
                            item.MobilePhone1 = lead.MobilePhone1;
                            item.MobilePhone2 = lead.MobilePhone2;
                            item.MobilePhone3 = lead.MobilePhone3;
                            _comRepo.UpdateCompanyResource(item);
                        }
                    }
                    else
                    {
                        var item = new CompanyResource()
                        {
                            CompanyID = lead.CompanyID,
                            CountryID = lead.Company.CountryID,
                            Country = lead.Company.CountryCode,
                            FirstName = lead.FirstName,
                            LastName = lead.LastName,
                            DirectLine = lead.DirectLine,
                            MobilePhone1 = lead.MobilePhone1,
                            MobilePhone2 = lead.MobilePhone2,
                            MobilePhone3 = lead.MobilePhone3,
                            Organisation = lead.CompanyName,
                            PersonalEmail = lead.PersonalEmail,
                            Role = lead.JobTitle,
                            Salutation = lead.Salutation,
                            WorkEmail = lead.WorkEmail,
                            Auto = true
                        };
                        item = _comRepo.CreateCompanyResource(item);
                        companyResources.Add(item);
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

        [DisplayName(@"Import From Excel")]
        public ActionResult ImportFromExcel()
        {
            return View(new EventImportModel());
        }

        [DisplayName(@"Import From Excel")]
        [HttpPost]
        public ActionResult ImportFromExcel(EventImportModel model)
        {
            if (model.FileImport == null)
                return View(new EventImportModel());
            if (model.FileImport.FileName.Substring(model.FileImport.FileName.LastIndexOf('.')).ToLower().Contains("xls"))
            {
                model.FilePath = ExcelUploadHelper.SaveFile(model.FileImport, FolderUpload.Events);
                try
                {
                    model.check_data();
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Import Failed... Format file is wrong";
                    return View(model);
                }
                if (model.ImportRows.Any(m => !string.IsNullOrEmpty(m.Error)))
                {
                    model.SessionName = "SessionEventImport" + Guid.NewGuid();
                    Session[model.SessionName] = model;
                    return View(model);
                }
                model.ParseValue();
                model.SessionName = "SessionEventImport" + Guid.NewGuid();
                Session[model.SessionName] = model;
                return View(model);
            }
            return View(model);
        }

        [AjaxOnly]
        public ActionResult ImportReview()
        {
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

            var session = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Session").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                session = Request.Form.GetValues("Session").FirstOrDefault();
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            if (!string.IsNullOrEmpty(session))
            {
                var model = (EventImportModel)Session[session];
                if (model != null)
                {
                    var resourceJsons = model.ImportRows.AsEnumerable();
                    recordsTotal = resourceJsons.Count();
                    if (pageSize > recordsTotal)
                    {
                        pageSize = recordsTotal;
                    }

                    resourceJsons = resourceJsons.Skip(skip).Take(pageSize).ToList();
                    var json = new
                    {
                        draw = draw,
                        recordsFiltered = recordsTotal,
                        recordsTotal = recordsTotal,
                        data = resourceJsons.Select(m => new
                        {
                            m.EventCode,
                            m.EventStatusStr,
                            m.EventCategoryStr,
                            m.EventName,
                            m.StartDateStr,
                            m.EndDateStr,
                            m.DateOfConfirmationStr,
                            m.ClosingDateStr,
                            m.DateOfOpenStr,
                            m.Location,
                            m.Summary,
                            m.Error,
                        })
                    };
                    return Json(json, JsonRequestBehavior.AllowGet);
                }
            }
            var data = new List<EventJson>();
            var json1 = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.EventCode,
                    m.EventStatusStr,
                    m.EventCategoryStr,
                    m.EventName,
                    m.StartDateStr,
                    m.EndDateStr,
                    m.DateOfConfirmationStr,
                    m.ClosingDateStr,
                    m.DateOfOpenStr,
                    m.Location,
                    m.Summary,
                    m.Error,
                })
            };
            return Json(json1, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        [HttpPost]
        public ActionResult ComfirmImport(string sessionName)
        {
            if (string.IsNullOrEmpty(sessionName) || Session[sessionName] == null)
            {
                return Json("Session is not exists or expired.", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var model = (EventImportModel)Session[sessionName];
                model.ConfirmImport();
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }


        [AjaxOnly]
        public ActionResult GetPossibleEvent(string q)
        {
            var bookings = _repo.GetAllPossibleEvents(q.ToLower());
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
            var salesUser = PermissionHelper.SalesmanId();
            int recordsTotal = _repo.GetCountEvents(salesUser, searchValue);
            var data = _repo.GetAllEvents(salesUser, searchValue, sortColumnDir, sortColumn, skip, pageSize);
            if (salesUser > 0)
            {
                var bookings = _bookingService.GetAllBookings(m => m.BookingStatusRecord == BookingStatus.Approved);
                foreach (var item in data)
                {
                    item.TotalDelegates = bookings.Where(m => m.EventID == item.ID).Sum(m => m.Delegates.Count);
                }
            }

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
                    m.EventCategoryDisplay,
                    m.BackgroundColor,
                    m.Location,
                    m.HotelVenue,
                    m.TotalDelegates,
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
            var tier = 0;
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Tier") != null && Request.Form.GetValues("Tier").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                try
                {
                    tier = Convert.ToInt32(Request.Form.GetValues("Tier").FirstOrDefault().Trim().ToLower());
                }
                catch
                {
                    
                }
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
            int recordsTotal = _repo.GetCountEventCompanies(eventId, companyName, productService, countryName, tier, sector, industry);
            var data = _repo.GetAllEventCompanies(eventId, companyName, productService, countryName, tier, sector, industry, sortColumnDir, sortColumn, skip, pageSize);

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
