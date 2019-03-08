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
    public class ReportCallController : Controller
    {

        private readonly IReportCallService _reportRepo;

        public ReportCallController(IReportCallService reportRepo)
        {
            _reportRepo = reportRepo;
        }
        [DisplayName(@"Report Call management")]
        public ActionResult Index()
        {
            return View(new List<ReportCall>());
        }
        public ActionResult Detail(int id)
        {
            var model = _reportRepo.GetReportCall(id);
            return View(model);
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

            IEnumerable<ReportCall> companies = new HashSet<ReportCall>();

            if (!string.IsNullOrEmpty(searchValue))
            {
                Func<ReportCall, bool> predicate = m =>
                        (m.SalesmanName.ToLower().Contains(searchValue)) ||
                        (m.DirectLine != null && m.DirectLine.ToLower().Contains(searchValue)) ||
                        (m.JobTitle != null && m.JobTitle.ToLower().Contains(searchValue)) ||
                        (m.LineExtension != null && m.LineExtension.ToLower().Contains(searchValue)) ||
                        (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                        (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                        (m.MobilePhone1 != null && m.MobilePhone1.ToLower().Contains(searchValue)) ||
                        (m.MobilePhone2 != null && m.MobilePhone2.ToLower().Contains(searchValue)) ||
                        (m.MobilePhone3 != null && m.MobilePhone3.ToLower().Contains(searchValue)) ||
                        (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                        (m.WorkEmail1 != null && m.WorkEmail1.ToLower().Contains(searchValue)) ||
                        (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                        (m.CompanyName != null && m.CompanyName.ToLower().Contains(searchValue)) ||
                        (m.Remark != null && m.Remark.ToLower().Contains(searchValue));
                companies = _reportRepo.GetAllReportCalls(predicate).ToList();
            }
            else
            {
                companies = _reportRepo.GetAllReportCalls();
            }

            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        companies = companies.OrderBy(s => s.CreatedTime).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "DirectLine":
                        companies = companies.OrderBy(s => s.DirectLine).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "JobTitle":
                        companies = companies.OrderBy(s => s.JobTitle).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "Remark":
                        companies = companies.OrderBy(s => s.Remark).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "FirstName":
                        companies = companies.OrderBy(s => s.FirstName).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "Salutation":
                        companies = companies.OrderBy(s => s.Salutation).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "LastName":
                        companies = companies.OrderBy(s => s.LastName).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "MobilePhone1":
                        companies = companies.OrderBy(s => s.MobilePhone1).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "MobilePhone2":
                        companies = companies.OrderBy(s => s.MobilePhone2).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "MobilePhone3":
                        companies = companies.OrderBy(s => s.MobilePhone3).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "WorkEmail":
                        companies = companies.OrderBy(s => s.WorkEmail).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "WorkEmail1":
                        companies = companies.OrderBy(s => s.WorkEmail1).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "PersonalEmail":
                        companies = companies.OrderBy(s => s.PersonalEmail).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "CompanyName":
                        companies = companies.OrderBy(s => s.CompanyName).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "SalesmanName":
                        companies = companies.OrderBy(s => s.SalesmanName).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    default:
                        companies = companies.OrderBy(s => s.ID).AsEnumerable();
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        companies = companies.OrderByDescending(s => s.CreatedTime).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "DirectLine":
                        companies = companies.OrderByDescending(s => s.DirectLine).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "JobTitle":
                        companies = companies.OrderByDescending(s => s.JobTitle).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "Remark":
                        companies = companies.OrderByDescending(s => s.Remark).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "FirstName":
                        companies = companies.OrderByDescending(s => s.FirstName).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "Salutation":
                        companies = companies.OrderByDescending(s => s.Salutation).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "LastName":
                        companies = companies.OrderByDescending(s => s.LastName).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "MobilePhone1":
                        companies = companies.OrderByDescending(s => s.MobilePhone1).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "MobilePhone2":
                        companies = companies.OrderByDescending(s => s.MobilePhone2).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "MobilePhone3":
                        companies = companies.OrderByDescending(s => s.MobilePhone3).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "WorkEmail":
                        companies = companies.OrderByDescending(s => s.WorkEmail).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "WorkEmail1":
                        companies = companies.OrderByDescending(s => s.WorkEmail1).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "PersonalEmail":
                        companies = companies.OrderByDescending(s => s.PersonalEmail).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "CompanyName":
                        companies = companies.OrderByDescending(s => s.CompanyName).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    case "SalesmanName":
                        companies = companies.OrderByDescending(s => s.SalesmanName).ThenBy(s => s.ID).AsEnumerable();
                        break;
                    default:
                        companies = companies.OrderByDescending(s => s.ID).AsEnumerable();
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
                    m.CreatedTimeDisplay,
                    m.CreatedTime,
                    m.SalesmanName,
                    m.CompanyName,
                    m.PersonalEmail,
                    m.WorkEmail,
                    m.WorkEmail1,
                    m.MobilePhone1,
                    m.MobilePhone2,
                    m.MobilePhone3,
                    m.LastName,
                    m.FirstName,
                    m.Salutation,
                    m.Remark,
                    m.LineExtension,
                    m.JobTitle,
                    m.DirectLine,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

    }
}
