﻿using System;
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
    public class CompanyResourceController : Controller
    {
        //
        // GET: /CompanyResource/
        private readonly ICompanyRepository _comRepo;
        public CompanyResourceController(ICompanyRepository comRepo)
        {
            _comRepo = comRepo;
        }
        public ActionResult Index()
        {
            var model = new CompanyResourceModel();
            return View(model);
        }

        [DisplayName(@"Import From Excel")]
        public ActionResult ImportFromExcel()
        {
            return View(new CompanyResourceImportModel());
        }

        [DisplayName(@"Import From Excel")]
        [HttpPost]
        public ActionResult ImportFromExcel(CompanyResourceImportModel model)
        {
            if (model.FileImport == null)
                return View(new CompanyResourceImportModel());
            if (model.FileImport.FileName.Substring(model.FileImport.FileName.LastIndexOf('.')).ToLower().Contains("xls"))
            {
                model.FilePath = ExcelUploadHelper.SaveFile(model.FileImport, FolderUpload.CompanyResources);
                try
                {
                    model.check_data();
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Import Failed";
                    return View(model);
                }
                if (model.ImportRows.Any(m => !string.IsNullOrEmpty(m.Error)))
                {
                    return View(model);
                }
                model.ParseValue();
                model.SessionName = "SessionImport" + Guid.NewGuid();
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
                TempData["error"] = "Session is not exists or expired.";
                return RedirectToAction("ImportFromExcel");
            }
            else
            {
                var model = (CompanyResourceImportModel)Session[sessionName];
                model.ConfirmImport();
                TempData["message"] = "Import completed.";
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

            IEnumerable<CompanyResource> audits = new HashSet<CompanyResource>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                if (!string.IsNullOrEmpty(searchValue))
                    audits = _comRepo.GetAllCompanyResources(m =>
                        m.Country.ToLower().Contains(searchValue) ||
                        m.Organisation.ToLower().Contains(searchValue) ||
                        m.LastName.ToLower().Contains(searchValue) ||
                        m.FirstName.ToLower().Contains(searchValue) ||
                        m.Role.ToLower().Contains(searchValue) ||
                        m.BusinessPhone.ToLower().Contains(searchValue) ||
                        m.MobilePhone.ToLower().Contains(searchValue) ||
                        m.WorkEmailAddress.ToLower().Contains(searchValue) ||
                        m.PersonalEmailAddress.ToLower().Contains(searchValue)
                       );
            }
            else
            {
                audits = _comRepo.GetAllCompanyResources();
            }
            
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "Country":
                        audits = audits.OrderBy(s => s.Country).ThenBy(s => s.Organisation);
                        break;
                    case "LastName":
                        audits = audits.OrderBy(s => s.LastName).ThenBy(s => s.Organisation);
                        break;
                    case "FirstName":
                        audits = audits.OrderBy(s => s.FirstName).ThenBy(s => s.Organisation);
                        break;
                    case "Role":
                        audits = audits.OrderBy(s => s.Role).ThenBy(s => s.Organisation);
                        break;
                    case "BusinessPhone":
                        audits = audits.OrderBy(s => s.BusinessPhone).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone":
                        audits = audits.OrderBy(s => s.MobilePhone).ThenBy(s => s.Organisation);
                        break;
                    case "WorkEmailAddress":
                        audits = audits.OrderBy(s => s.WorkEmailAddress).ThenBy(s => s.Organisation);
                        break;
                    case "PersonalEmailAddress":
                        audits = audits.OrderBy(s => s.PersonalEmailAddress).ThenBy(s => s.Organisation);
                        break;
                    default:
                        audits = audits.OrderBy(s => s.Organisation);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "Country":
                        audits = audits.OrderByDescending(s => s.Country).ThenBy(s => s.Organisation);
                        break;
                    case "LastName":
                        audits = audits.OrderByDescending(s => s.LastName).ThenBy(s => s.Organisation);
                        break;
                    case "FirstName":
                        audits = audits.OrderByDescending(s => s.FirstName).ThenBy(s => s.Organisation);
                        break;
                    case "Role":
                        audits = audits.OrderByDescending(s => s.Role).ThenBy(s => s.Organisation);
                        break;
                    case "BusinessPhone":
                        audits = audits.OrderByDescending(s => s.BusinessPhone).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone":
                        audits = audits.OrderByDescending(s => s.MobilePhone).ThenBy(s => s.Organisation);
                        break;
                    case "WorkEmailAddress":
                        audits = audits.OrderByDescending(s => s.WorkEmailAddress).ThenBy(s => s.Organisation);
                        break;
                    case "PersonalEmailAddress":
                        audits = audits.OrderByDescending(s => s.PersonalEmailAddress).ThenBy(s => s.Organisation);
                        break;
                    default:
                        audits = audits.OrderByDescending(s => s.Organisation);
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
                    m.Country,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.Organisation,
                    m.Role,
                    m.BusinessPhone,
                    m.MobilePhone,
                    m.PersonalEmailAddress,
                    m.WorkEmailAddress,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}
