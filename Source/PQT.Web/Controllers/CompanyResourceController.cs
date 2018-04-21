using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
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
            model.CompanyResources = _comRepo.GetAllCompanyResources();
            return View(model);
        }

        public ActionResult ImportFromExcel()
        {
            return View(new CompanyResourceImportModel());
        }

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
    }
}
