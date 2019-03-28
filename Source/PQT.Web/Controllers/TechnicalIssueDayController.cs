using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Infrastructure.Filters;

namespace PQT.Web.Controllers
{
    public class TechnicalIssueDayController : Controller
    {
        //
        // GET: /Admin/Counter/
        private readonly ILeaveService _unitRepo;

        public TechnicalIssueDayController(ILeaveService unitRepo)
        {
            _unitRepo = unitRepo;
        }
        [DisplayName(@"List Management")]
        public ActionResult Index()
        {
            return View(new List<TechnicalIssueDay>());
        }
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(int id = 0)
        {
            var model = new TechnicalIssueDay();
            if (id > 0)
            {
                model = _unitRepo.GetTechnicalIssueDay(id);
                if (model != null)
                {
                    model.TempMonth = model.IssueMonth.ToString("MM/yyyy");
                }
            }
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(TechnicalIssueDay model)
        {
            model.IssueMonth = DateTime.ParseExact(model.TempMonth, "MM/yyyy", CultureInfo.InvariantCulture);
            var exist = _unitRepo.GetTechnicalIssueDayByMonth(model.IssueMonth, model.UserID);
            if (exist != null && exist.ID != model.ID)
                return Json(new
                {
                    Code = 5,
                    ErrorMessage = "Month exists"
                });
            if (ModelState.IsValid)
            {
                if (model.ID == 0)
                {
                    if (_unitRepo.CreateTechnicalIssueDay(model) != null)
                    {
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
                if (_unitRepo.UpdateTechnicalIssueDay(model))
                {
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
            IEnumerable<TechnicalIssueDay> data = new HashSet<TechnicalIssueDay>();
            recordsTotal = _unitRepo.GetCountTechnicalIssueDays(searchValue);
            data = _unitRepo.GetAllTechnicalIssueDays(searchValue, sortColumnDir, sortColumn, skip, pageSize);
            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.UserDisplay,
                    m.Remarks,
                    m.UserID,
                    m.TechnicalIssueDays,
                    m.IssueMonth,
                    m.IssueMonthDisplay
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}
