using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
            var models = _comRepo.GetAllCompanies();
            return View(models);
        }

        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(int id = 0)
        {
            var model = new Company();
            if (id > 0)
            {
                model = _comRepo.GetCompany(id);
            }
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(Company model)
        {
            if (model.CountryID == 0)
            {
                return Json(new
                {
                    Code = 6,
                    error = "Country should not be empty."
                });
            }
            if (ModelState.IsValid)
            {
                if (model.ID == 0)
                {
                    if (_comRepo.CreateCompany(model) != null)
                    {
                        model.Country = _unitRepo.GetCountry((int)model.CountryID);
                        return Json(new
                        {
                            Code = 1,
                            Model = model
                        });
                    }
                    return Json(new
                    {
                        Code = 2
                    });
                }
                if (_comRepo.UpdateCompany(model))
                {
                    model.Country = _unitRepo.GetCountry((int)model.CountryID);
                    return Json(new
                    {
                        Code = 3,
                        Model = model
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
                TempData["error"] = "Session does not exist or expired";
                return RedirectToAction("ImportFromExcel");
            }
            else
            {
                var model = (CompanyImportModel)Session[sessionName];
                model.ConfirmImport();
                TempData["message"] = "Import completed";
                return RedirectToAction("Index");
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


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            IEnumerable<Company> audits = new HashSet<Company>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                if (!string.IsNullOrEmpty(searchValue))
                    audits = _comRepo.GetAllCompanies(m =>
                        m.CountryName.ToLower().Contains(searchValue) ||
                        m.CompanyName.ToLower().Contains(searchValue) ||
                        m.ProductOrService.ToLower().Contains(searchValue) ||
                        m.Sector.ToLower().Contains(searchValue) ||
                        m.Industry.ToLower().Contains(searchValue)
                       );
            }
            else
            {
                audits = _comRepo.GetAllCompanies();
            }

            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CountryName":
                        audits = audits.OrderBy(s => s.CountryName).ThenBy(s => s.CompanyName);
                        break;
                    case "ProductOrService":
                        audits = audits.OrderBy(s => s.ProductOrService).ThenBy(s => s.CompanyName);
                        break;
                    case "Sector":
                        audits = audits.OrderBy(s => s.Sector).ThenBy(s => s.CompanyName);
                        break;
                    case "Industry":
                        audits = audits.OrderBy(s => s.Industry).ThenBy(s => s.CompanyName);
                        break;
                    default:
                        audits = audits.OrderBy(s => s.CompanyName);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CountryName":
                        audits = audits.OrderByDescending(s => s.CountryName).ThenBy(s => s.CompanyName);
                        break;
                    case "ProductOrService":
                        audits = audits.OrderByDescending(s => s.ProductOrService).ThenBy(s => s.CompanyName);
                        break;
                    case "Sector":
                        audits = audits.OrderByDescending(s => s.Sector).ThenBy(s => s.CompanyName);
                        break;
                    case "Industry":
                        audits = audits.OrderByDescending(s => s.Industry).ThenBy(s => s.CompanyName);
                        break;
                    default:
                        audits = audits.OrderByDescending(s => s.CompanyName);
                        break;
                }
            }


            recordsTotal = audits.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = audits.Skip(skip).Take(pageSize).ToList();

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
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}
