using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Controllers
{
    public class UploadTemplateController : Controller
    {
        //
        // GET: /UploadTemplate/
        private readonly IUploadTemplateService _uploadService;
        private readonly IMembershipService _membershipService;

        public UploadTemplateController(IUploadTemplateService uploadService, IMembershipService membershipService)
        {
            _uploadService = uploadService;
            _membershipService = membershipService;
        }

        [DisplayName(@"Templates management")]
        public ActionResult Index()
        {
            return View();
        }

        [AjaxOnly]
        public ActionResult SalesmanTemplates()
        {
            var user = _membershipService.GetUser(CurrentUser.Identity.ID);
            return PartialView(_uploadService.GetAllUploadTemplates(user.Roles.Select(m => m.Name).ToArray()));
        }
        public ActionResult Create()
        {
            return View(new UploadTemplate());
        }
        [HttpPost]
        public ActionResult Create(UploadTemplate model)
        {
            if (ModelState.IsValid)
            {
                if (model.UploadFile != null)
                    model.FileName = FileUpload.Upload(FileUploadType.Template, model.UploadFile);
                if (_uploadService.CreateUploadTemplate(model) != null)
                {
                    TempData["message"] = "Upload successful";
                    return RedirectToAction("Index");
                }
            }
            TempData["error"] = "Upload failed";
            return View(model);
        }
        public ActionResult Edit(int id = 0)
        {
            var model = _uploadService.GetUploadTemplate(id);
            if (model == null)
            {
                TempData["error"] = "Data not found";
                return RedirectToAction("Index");
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(UploadTemplate model)
        {
            if (ModelState.IsValid)
            {
                if (model.UploadFile != null)
                    model.FileName = FileUpload.Upload(FileUploadType.Template, model.UploadFile);
                if (_uploadService.UpdateTemplate(model))
                {
                    TempData["message"] = "Update successful";
                    return RedirectToAction("Index");
                }
            }
            TempData["error"] = "Update failed";
            return View(model);
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (_uploadService.DeleteTemplate(id))
            {
                return Json(true);
            }
            return Json(false);
        }
        [AjaxOnly]
        public ActionResult AjaxGetIndex()
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
            //var saleId = PermissionHelper.SalesmanId();
            IEnumerable<UploadTemplate> bookings = new HashSet<UploadTemplate>();
            bookings = !string.IsNullOrEmpty(searchValue) ?
                _uploadService.GetAllUploadTemplates().Where(m => m.GroupName.ToLower().Contains(searchValue) || m.Department.ToLower().Contains(searchValue)) :
                _uploadService.GetAllUploadTemplates();
            // ReSharper disable once AssignNullToNotNullAttribute

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "Department":
                        bookings = bookings.OrderBy(s => s.Department).ThenBy(s => s.ID);
                        break;
                    case "UploadTime":
                        bookings = bookings.OrderBy(s => s.UploadTime).ThenBy(s => s.ID);
                        break;
                    case "GroupName":
                        bookings = bookings.OrderBy(s => s.GroupName).ThenBy(s => s.ID);
                        break;
                    case "FileName":
                        bookings = bookings.OrderBy(s => s.FileName).ThenBy(s => s.ID);
                        break;
                    default:
                        bookings = bookings.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "Department":
                        bookings = bookings.OrderByDescending(s => s.Department).ThenBy(s => s.ID);
                        break;
                    case "UploadTime":
                        bookings = bookings.OrderByDescending(s => s.UploadTime).ThenBy(s => s.ID);
                        break;
                    case "GroupName":
                        bookings = bookings.OrderByDescending(s => s.GroupName).ThenBy(s => s.ID);
                        break;
                    case "FileName":
                        bookings = bookings.OrderByDescending(s => s.FileName).ThenBy(s => s.ID);
                        break;
                    default:
                        bookings = bookings.OrderByDescending(s => s.ID);
                        break;
                }
            }

            #endregion sort

            recordsTotal = bookings.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = bookings.Skip(skip).Take(pageSize).ToList();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    UploadTime = m.UploadTimeStr,
                    GroupName = m.GroupName,
                    Department = m.Department,
                    FileName = m.FileName,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}
