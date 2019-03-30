using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Models;

namespace PQT.Web.Controllers
{
    public class CompanyController : Controller
    {

        private readonly ICompanyRepository _comRepo;
        private readonly IUnitRepository _unitRepo;
        private readonly IBookingService _bookingService;

        public CompanyController(ICompanyRepository comRepo, IUnitRepository unitRepo, IBookingService bookingService)
        {
            _comRepo = comRepo;
            _unitRepo = unitRepo;
            _bookingService = bookingService;
        }
        [DisplayName(@"Company management")]
        public ActionResult Index()
        {
            //var models = _comRepo.GetAllCompanies();
            return View(new List<Company>());
        }
        public ActionResult Detail(int id)
        {
            var model = new CompanyModel();
            model.Prepare(id);
            return View(model);
        }

        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(int id = 0)
        {
            var model = new CompanyModel();
            model.Prepare(id);
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(CompanyModel model)
        {
            if (model.Company.CountryID == 0)
            {
                return Json(new
                {
                    Code = 6,
                    error = "Country should not be empty."
                });
            }
            if (ModelState.IsValid)
            {
                if (model.Company.ID == 0)
                {
                    model.Company.Country = _unitRepo.GetCountry((int)model.Company.CountryID);
                    model.Company = _comRepo.CreateCompany(model.Company, model.UsersSelected);
                    if (model.Company != null)
                    {
                        return Json(new
                        {
                            Code = 1,
                            Model = model.Company
                        });
                    }
                    return Json(new
                    {
                        Code = 2
                    });
                }
                if (_comRepo.UpdateCompany(model.Company, model.UsersSelected) != null)
                {
                    model.Company.Country = _unitRepo.GetCountry((int)model.Company.CountryID);
                    return Json(new
                    {
                        Code = 3,
                        Model = model.Company
                    });
                }
                return Json(new
                {
                    Code = 4
                });
            }
            return Json(new
            {
                Code = 5
            });
        }

        [DisplayName(@"Merge")]
        public ActionResult Merge(int id = 0)
        {
            var model = new CompanyModel();
            model.Prepare(id);
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Merge")]
        public ActionResult Merge(CompanyModel model)
        {
            if (model.MergeCompanyID == 0)
            {
                return Json(new
                {
                    Code = 6,
                    error = "Merge Company should not be empty."
                });
            }
            if (ModelState.IsValid)
            {
                var company = _comRepo.MergeCompany(model.CompanyID, model.MergeCompanyID);
                if (company != null)
                {
                    //company.Country = _unitRepo.GetCountry((int)company.CountryID);
                    return Json(new
                    {
                        Code = 1,
                        Model = company
                    });
                }
                return Json(new
                {
                    Code = 4
                });
            }
            return Json(new
            {
                Code = 5
            });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (_comRepo.DeleteCompany(id))
            {
                return Json(true);
            }
            return Json(false);
        }

        [DisplayName(@"Import From Excel")]
        public ActionResult ImportFromExcel()
        {
            return View(new CompanyImportModel());
        }

        [HttpPost]
        [DisplayName(@"Import From Excel")]
        public ActionResult ImportFromExcel(CompanyImportModel model)
        {
            if (model.FileImport == null)
                return View(new CompanyImportModel());
            if (model.FileImport.FileName.Substring(model.FileImport.FileName.LastIndexOf('.')).ToLower().Contains("xls"))
            {
                model.FilePath = ExcelUploadHelper.SaveFile(model.FileImport, FolderUpload.Companies);
                try
                {
                    model.check_data();
                }
                catch (Exception)
                {
                    TempData["error"] = "Import failed... Format file is wrong";
                    return View(model);
                }
                if (model.ImportRows.Any(m => !string.IsNullOrEmpty(m.Error)))
                {
                    return View(model);
                }
                model.ParseValue();
                model.SessionName = "SessionComImport" + Guid.NewGuid();
                Session[model.SessionName] = model;
                return View(model);
            }
            return View(model);
        }

        [AjaxOnly]
        public ActionResult GetCompaniesForAjaxDropdown(int id, string q)
        {
            var bookings = _comRepo.GetCompaniesForAjaxDropdown(id,q).Select(m => new { id = m.ID, text = m.CompanyName });
            return Json(bookings, JsonRequestBehavior.AllowGet);
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
                var model = (CompanyImportModel)Session[sessionName];
                model.ConfirmImport();
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }


        [AjaxOnly]
        public ActionResult AjaxGetAlls()
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
            //var searchValue = "";
            //// ReSharper disable once AssignNullToNotNullAttribute
            //if (Request.Form.GetValues("search[value]").FirstOrDefault() != null)
            //{
            //    // ReSharper disable once PossibleNullReferenceException
            //    searchValue = Request.Form.GetValues("search[value]").FirstOrDefault().Trim().ToLower();
            //}
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
                catch (Exception e)
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
            var businessUnit = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("BusinessUnit") != null && Request.Form.GetValues("BusinessUnit").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                businessUnit = Request.Form.GetValues("BusinessUnit").FirstOrDefault().Trim().ToLower();
            }

            var ownership = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Ownership") != null && Request.Form.GetValues("Ownership").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                ownership = Request.Form.GetValues("Ownership").FirstOrDefault().Trim().ToLower();
            }
            var financialYear = 0;
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("FinancialYear") != null && Request.Form.GetValues("FinancialYear").FirstOrDefault() != null)
            {
                try
                {
                    financialYear = Convert.ToInt32(Request.Form.GetValues("FinancialYear").FirstOrDefault().Trim().ToLower());
                }
                catch (Exception e)
                {
                }
                // ReSharper disable once PossibleNullReferenceException
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = _comRepo.GetCountCompanies(companyName, countryName, productService,sector,tier,industry,businessUnit,ownership, financialYear);
            var data = _comRepo.GetAllCompanies(companyName, countryName, productService, sector, tier, industry, businessUnit, ownership, financialYear, sortColumnDir, sortColumn, skip, pageSize);
            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.CountryName,
                    m.CompanyName,
                    m.ProductOrService,
                    m.Sector,
                    m.Industry,
                    m.BusinessUnit,
                    m.FinancialYear,
                    m.Tier,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult AjaxGetDelegates(int comId=0)
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
            // ReSharper disable once AssignNullToNotNullAttribute
            if (comId == 0 && Request.Form.GetValues("ComId") != null && !string.IsNullOrEmpty(Request.Form.GetValues("ComId").FirstOrDefault()))
            {
                // ReSharper disable once PossibleNullReferenceException
                comId = Convert.ToInt32(Request.Form.GetValues("ComId").FirstOrDefault());
            }
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int page = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            IEnumerable<PQT.Domain.Entities.Delegate> delegates = new HashSet<PQT.Domain.Entities.Delegate>();
            var bookings = _bookingService.GetAllBookings(m => m.CompanyID == comId && m.BookingStatusRecord == BookingStatus.Approved);
            if (!string.IsNullOrEmpty(searchValue))
            {
                Func<PQT.Domain.Entities.Delegate, bool> predicate = m =>
                    (!string.IsNullOrEmpty(m.EventCode) && m.EventCode.ToLower().Contains(searchValue)) ||
                    (!string.IsNullOrEmpty(m.EventDate) && m.EventDate.ToLower().Contains(searchValue)) ||
                    (!string.IsNullOrEmpty(m.EventName) && m.EventName.ToLower().Contains(searchValue)) ||
                    (!string.IsNullOrEmpty(m.PersonalEmail) && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                    (!string.IsNullOrEmpty(m.WorkEmail) && m.WorkEmail.ToLower().Contains(searchValue)) ||
                    (!string.IsNullOrEmpty(m.MobilePhone1) && m.MobilePhone1.ToLower().Contains(searchValue)) ||
                    (!string.IsNullOrEmpty(m.MobilePhone2) && m.MobilePhone2.ToLower().Contains(searchValue)) ||
                    (!string.IsNullOrEmpty(m.MobilePhone3) && m.MobilePhone3.ToLower().Contains(searchValue)) ||
                    (!string.IsNullOrEmpty(m.AttendanceStatusDisplay) &&
                     m.AttendanceStatusDisplay.ToLower().Contains(searchValue));
                delegates = bookings.SelectMany(m => m.Delegates.Select(d =>
                        d.PassInfoForTierCompany(m.EventID.ToString(), m.Event.EventName, m.Event.EventCode,
                            m.Event.EventDate)))
                    .Where(predicate);
            }
            else
            {
                delegates = bookings.SelectMany(m => m.Delegates.Select(d =>
                    d.PassInfoForTierCompany(m.EventID.ToString(), m.Event.EventName, m.Event.EventCode,
                        m.Event.EventDate)));
            }

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "EventName":
                        delegates = delegates.OrderBy(s => s.EventName).ThenBy(s => s.ID);
                        break;
                    case "EventCode":
                        delegates = delegates.OrderBy(s => s.EventCode).ThenBy(s => s.ID);
                        break;
                    case "EventDate":
                        delegates = delegates.OrderBy(s => s.EventDate).ThenBy(s => s.ID);
                        break;
                    case "DelegateName":
                        delegates = delegates.OrderBy(s => s.FullName).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        delegates = delegates.OrderBy(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone1":
                        delegates = delegates.OrderBy(s => s.MobilePhone1).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone2":
                        delegates = delegates.OrderBy(s => s.MobilePhone2).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone3":
                        delegates = delegates.OrderBy(s => s.MobilePhone3).ThenBy(s => s.ID);
                        break;
                    case "WorkEmail":
                        delegates = delegates.OrderBy(s => s.WorkEmail).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmail":
                        delegates = delegates.OrderBy(s => s.PersonalEmail).ThenBy(s => s.ID);
                        break;
                    case "DelegateEmail":
                        delegates = delegates.OrderBy(s => s.DelegateEmail).ThenBy(s => s.ID);
                        break;
                    case "DelegateContact":
                        delegates = delegates.OrderBy(s => s.DelegateContact).ThenBy(s => s.ID);
                        break;
                    case "AttendanceStatusDisplay":
                        delegates = delegates.OrderBy(s => s.AttendanceStatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        delegates = delegates.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "EventName":
                        delegates = delegates.OrderByDescending(s => s.EventName).ThenBy(s => s.ID);
                        break;
                    case "EventCode":
                        delegates = delegates.OrderByDescending(s => s.EventCode).ThenBy(s => s.ID);
                        break;
                    case "EventDate":
                        delegates = delegates.OrderByDescending(s => s.EventDate).ThenBy(s => s.ID);
                        break;
                    case "DelegateName":
                        delegates = delegates.OrderByDescending(s => s.FullName).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        delegates = delegates.OrderByDescending(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone1":
                        delegates = delegates.OrderByDescending(s => s.MobilePhone1).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone2":
                        delegates = delegates.OrderByDescending(s => s.MobilePhone2).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone3":
                        delegates = delegates.OrderByDescending(s => s.MobilePhone3).ThenBy(s => s.ID);
                        break;
                    case "WorkEmail":
                        delegates = delegates.OrderByDescending(s => s.WorkEmail).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmail":
                        delegates = delegates.OrderByDescending(s => s.PersonalEmail).ThenBy(s => s.ID);
                        break;
                    case "DelegateEmail":
                        delegates = delegates.OrderByDescending(s => s.DelegateEmail).ThenBy(s => s.ID);
                        break;
                    case "DelegateContact":
                        delegates = delegates.OrderByDescending(s => s.DelegateContact).ThenBy(s => s.ID);
                        break;
                    case "AttendanceStatusDisplay":
                        delegates = delegates.OrderByDescending(s => s.AttendanceStatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        delegates = delegates.OrderByDescending(s => s.ID);
                        break;
                }
            }

            #endregion sort

            recordsTotal = delegates.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = delegates.Skip(page).Take(pageSize).ToList();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.EventName,
                    m.EventCode,
                    m.EventDate,
                    DelegateName = m.FullName,
                    m.DirectLine,
                    m.MobilePhone1,
                    m.MobilePhone2,
                    m.MobilePhone3,
                    m.WorkEmail,
                    m.PersonalEmail,
                    m.DelegateContact,
                    m.DelegateEmail,
                    m.AttendanceStatusDisplay
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }


    }
}
