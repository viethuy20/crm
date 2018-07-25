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
                    if (_comRepo.CreateCompany(model.Company, model.UsersSelected) != null)
                    {
                        model.Company.Country = _unitRepo.GetCountry((int)model.Company.CountryID);
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
                if (_comRepo.UpdateCompany(model.Company, model.UsersSelected))
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


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int page = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            IEnumerable<Company> companies = new HashSet<Company>();

            if (!string.IsNullOrEmpty(searchValue))
            {

                Func<Company, bool> predicate = m =>
                   (
                        (m.Country != null && m.Country.Name.ToLower().Contains(searchValue)) ||
                        (m.CompanyName != null && m.CompanyName.ToLower().Contains(searchValue)) ||
                        (m.ProductOrService != null && m.ProductOrService.ToLower().Contains(searchValue)) ||
                        (m.Sector != null && m.Sector.ToLower().Contains(searchValue)) ||
                        (m.Industry != null && m.Industry.ToLower().Contains(searchValue)));
                companies = _comRepo.GetAllCompanies(predicate, sortColumnDir, sortColumn,
                    page, pageSize);
                recordsTotal = _comRepo.GetCountCompanies(predicate);
            }
            else
            {
                companies = _comRepo.GetAllCompanies(sortColumnDir, sortColumn, page, pageSize);
                recordsTotal = _comRepo.GetCountCompanies(null);
            }

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = companies.Select(m => new
                {
                    m.ID,
                    m.CountryName,
                    m.CompanyName,
                    m.ProductOrService,
                    m.Sector,
                    m.Industry,
                    m.Tier,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}
