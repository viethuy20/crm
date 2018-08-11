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
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Models;

namespace PQT.Web.Controllers
{
    public class CompanyController : Controller
    {

        private readonly ICompanyRepository _comRepo;
        private readonly IUnitRepository _unitRepo;

        public CompanyController(ICompanyRepository comRepo, IUnitRepository unitRepo)
        {
            _comRepo = comRepo;
            _unitRepo = unitRepo;
        }
        [DisplayName(@"Company management")]
        public ActionResult Index()
        {
            //var models = _comRepo.GetAllCompanies();
            return View(new List<Company>());
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
            var searchValue = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("search[value]").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                searchValue = Request.Form.GetValues("search[value]").FirstOrDefault().Trim().ToLower();
            }
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
            var financialYear = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("FinancialYear") != null && Request.Form.GetValues("FinancialYear").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                financialYear = Request.Form.GetValues("FinancialYear").FirstOrDefault().Trim().ToLower();
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            IEnumerable<Company> companies = new HashSet<Company>();

            if (!string.IsNullOrEmpty(searchValue))
            {
                Func<Company, bool> predicate = m =>
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
                     (!string.IsNullOrEmpty(m.Industry) && m.Industry.ToLower().Contains(industry))) &&
                (string.IsNullOrEmpty(tier) ||
                 (m.Tier.ToString().Contains(tier))) &&
                    (string.IsNullOrEmpty(businessUnit) ||
                     (!string.IsNullOrEmpty(m.BusinessUnit) && m.BusinessUnit.ToLower().Contains(businessUnit))) &&
                    (string.IsNullOrEmpty(ownership) ||
                     (!string.IsNullOrEmpty(m.Ownership) && m.Ownership.ToLower().Contains(ownership))) &&
                    (string.IsNullOrEmpty(financialYear) ||
                     (m.FinancialYear > 0 && m.FinancialYear.ToString().Contains(financialYear))) &&
                   (
                        (m.Country != null && m.Country.Name.ToLower().Contains(searchValue)) ||
                        (m.CompanyName != null && m.CompanyName.ToLower().Contains(searchValue)) ||
                        (m.ProductOrService != null && m.ProductOrService.ToLower().Contains(searchValue)) ||
                        (m.Sector != null && m.Sector.ToLower().Contains(searchValue)) ||
                        (m.Industry != null && m.Industry.ToLower().Contains(searchValue)) ||
                        (m.BusinessUnit != null && m.BusinessUnit.ToLower().Contains(searchValue)));
                companies = _comRepo.GetAllCompanies(predicate).ToList();
            }
            else
            {
                Func<Company, bool> predicate = m =>
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
                        (!string.IsNullOrEmpty(m.Industry) && m.Industry.ToLower().Contains(industry))) &&
                       (string.IsNullOrEmpty(tier) ||
                        (m.Tier.ToString().Contains(tier))) &&
                       (string.IsNullOrEmpty(businessUnit) ||
                        (!string.IsNullOrEmpty(m.BusinessUnit) && m.BusinessUnit.ToLower().Contains(businessUnit))) &&
                       (string.IsNullOrEmpty(ownership) ||
                        (!string.IsNullOrEmpty(m.Ownership) && m.Ownership.ToLower().Contains(ownership))) &&
                       (string.IsNullOrEmpty(financialYear) ||
                        (m.FinancialYear > 0 && m.FinancialYear.ToString().Contains(financialYear)));
                companies = _comRepo.GetAllCompanies(predicate).ToList();
            }

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
    }
}
