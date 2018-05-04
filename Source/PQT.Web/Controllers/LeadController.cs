using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Infrastructure.Utility;
using PQT.Web.Models;

namespace PQT.Web.Controllers
{
    public class LeadController : Controller
    {
        //
        // GET: /Lead/
        private readonly ILeadService _repo;
        private readonly ICompanyRepository _companyRepo;

        public LeadController(ILeadService repo, ICompanyRepository companyRepo)
        {
            _repo = repo;
            _companyRepo = companyRepo;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">event id</param>
        /// <returns></returns>
        [DisplayName(@"My Call List For Salesman")]
        public ActionResult Index(int id = 0)
        {
            var model = new LeadModelView();
            model.Prepare(id);
            if (model.Event == null)
            {
                TempData["error"] = "Event not found";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
        [DisplayName(@"No Call List For Salesman")]
        public ActionResult NCL(int id = 0)
        {
            var model = new LeadModelView();
            model.Prepare(id);
            if (model.Event == null)
            {
                TempData["error"] = "Event not found";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
        [DisplayName(@"No Call List For Manager")]
        public ActionResult NCLForManager(int id = 0)
        {
            var model = new LeadModelView();
            model.Prepare(id);
            if (model.Event == null)
            {
                TempData["error"] = "Event not found";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [DisplayName(@"Call KPI")]
        public ActionResult CallKPI()
        {
            var model = new LeadMarkKPIModel();
            return View(model);
        }
        public ActionResult Edit(int leadId)
        {
            var model = new CallingModel(leadId);
            if (model.Lead == null)
            {
                TempData["error"] = "Data not found";
            }
            else if (model.Lead.UserID != CurrentUser.Identity.ID)
            {
                TempData["error"] = "Don't have permission";
            }
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Edit(CallingModel model)
        {
            if (model.SaveEdit())
            {
                return Json(new { Code = 1, TypeSubmit = model.TypeSubmit, LeadID = model.PhoneCall.LeadID });
            }
            return Json(new { Code = 0, Message = "Save failed" });
        }

        [DisplayName(@"Lead Detail")]
        public ActionResult Detail(int id = 0, int eventId = 0)
        {
            var model = new LeadModelView();
            model.PrepareDetail(id);
            if (model.Lead == null)
            {
                TempData["error"] = "Call not found";
                if (CurrentUser.HasRoleLevel(RoleLevel.ManagerLevel))
                {
                    return RedirectToAction("NCLForManager", new { id = eventId });
                }
                else
                {
                    return RedirectToAction("Index", new { id = eventId });
                }

            }
            if (model.Lead.UserID != CurrentUser.Identity.ID && !CurrentUser.HasRoleLevel(RoleLevel.ManagerLevel))
            {
                TempData["error"] = "Don't have permission";
                if (CurrentUser.HasRoleLevel(RoleLevel.ManagerLevel))
                    return RedirectToAction("NCLForManager", new { id = eventId });
                else
                    return RedirectToAction("Index", new { id = eventId });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(LeadModel model)
        {
            return Json(model.DeleteLead());
        }
        //[DisplayName(@"Call Sheet")]
        //public ActionResult CallSheet(int eventId)
        //{
        //    var model = new CallSheetModel(eventId);
        //    return PartialView(model);
        //}
        //[HttpPost]
        //[DisplayName(@"Call Sheet")]
        //public ActionResult CallSheet(CallSheetModel model)
        //{
        //    if (model.Lead.CompanyID == 0)
        //    {
        //        return Json(new { Code = 0, Message = "Please select company" });
        //    }

        //    var leadExists = _repo.GetAllLeads(m =>
        //        m.EventID == model.Lead.EventID && m.UserID != CurrentUser.Identity.ID &&
        //        m.CompanyID == model.Lead.CompanyID &&
        //        (m.LeadStatusRecord == LeadStatus.Blocked || m.LeadStatusRecord == LeadStatus.Booked || m.LeadStatusRecord.UpdatedTime.Date >=
        //         DateTime.Today.AddDays(-Settings.Lead.NumberDaysExpired())) &&
        //        (m.LeadStatusRecord == LeadStatus.Blocked ||
        //         m.LeadStatusRecord == LeadStatus.Live ||
        //         m.LeadStatusRecord == LeadStatus.LOI ||
        //         m.LeadStatusRecord == LeadStatus.Booked));
        //    if (leadExists.Any())
        //    {
        //        return Json(new { Code = 0, Message = "Cannot process... This company is existing in NCL" });
        //    }
        //    if (model.TypeSubmit == "SaveCall")
        //    {
        //        if (model.Save())
        //        {
        //            var callingModel = new CallingModel
        //            {
        //                Lead = model.Lead,
        //                PhoneCall = { LeadID = model.Lead.ID }
        //            };
        //            return PartialView("CallingForm", callingModel);
        //        }
        //        return Json(new { Code = 0, Message = "Save failed" });
        //    }
        //    if (model.Save())
        //    {
        //        return Json(new { Code = 1, Model = model.Lead });
        //    }
        //    return Json(new { Code = 0, Message = "Save failed" });
        //}

        [DisplayName(@"Calling Form")]
        public ActionResult CallingForm(int eventId, int leadId = 0)
        {
            var model = new CallingModel(eventId, leadId);
            return View(model);
        }

        [HttpPost]
        [DisplayName(@"Calling Form")]
        public ActionResult CallingForm(CallingModel model)
        {
            if (model.CompanyID == 0)
            {
                return Json(new { Code = 0, Message = "Please select company" });
            }

            var leadExists = _repo.GetAllLeads(m =>
                m.EventID == model.EventID && m.UserID != CurrentUser.Identity.ID &&
                m.CompanyID == model.CompanyID &&
                (m.LeadStatusRecord == LeadStatus.Blocked || m.LeadStatusRecord == LeadStatus.Booked || m.LeadStatusRecord.UpdatedTime.Date >=
                 DateTime.Today.AddDays(-Settings.Lead.NumberDaysExpired())) &&
                (m.LeadStatusRecord == LeadStatus.Blocked ||
                 m.LeadStatusRecord == LeadStatus.Live ||
                 m.LeadStatusRecord == LeadStatus.LOI ||
                 m.LeadStatusRecord == LeadStatus.Booked));
            if (leadExists.Any())
            {
                return Json(new { Code = 0, Message = "Cannot process... This company is existing in NCL" });
            }
            if (model.TypeSubmit == "SaveCall")
            {
                if (model.LeadID > 0)
                {
                    if (model.Save())
                    {
                        var callingModel = new CallingModel
                        {
                            Lead = model.Lead,
                            PhoneCall = { LeadID = model.Lead.ID }
                        };
                        return PartialView("CallingForm", callingModel);
                    }
                }
                else if (model.Create())
                {
                    var callingModel = new CallingModel
                    {
                        Lead = model.Lead,
                        PhoneCall = { LeadID = model.Lead.ID }
                    };
                    return PartialView("CallingForm", callingModel);
                }
                return Json(new { Code = 0, Message = "Save failed" });
            }
            if (model.LeadID > 0)
            {
                if (model.Save())
                {
                    return Json(new { Code = 1, TypeSubmit = model.TypeSubmit, LeadID = model.PhoneCall.LeadID });
                }
            }
            else if (model.Create())
            {
                return Json(new { Code = 1, TypeSubmit = model.TypeSubmit, LeadID = model.PhoneCall.LeadID });
            }
            return Json(new { Code = 0, Message = "Save failed" });
        }

        [DisplayName(@"Request NCL")]
        public ActionResult RequestAction(int id, string requestType)
        {
            var model = new LeadModel(id, requestType);
            return PartialView(model);
        }

        [DisplayName(@"Request NCL")]
        [HttpPost]
        public ActionResult RequestAction(LeadModel model)
        {
            return Json(model.RequestAction());
        }

        [DisplayName(@"Cancel Request")]
        [HttpPost]
        public ActionResult CancelRequest(LeadModel model)
        {
            return Json(model.CancelRequest());
        }

        [DisplayName(@"Block")]
        public ActionResult BlockLead(LeadModel model)
        {
            return Json(model.BlockLead());
        }

        [DisplayName(@"Unblock")]
        public ActionResult UnblockLead(LeadModel model)
        {
            return Json(model.UnblockLead());
        }

        [DisplayName(@"Approval NCL")]
        [HttpPost]
        public ActionResult ApprovalAction(LeadModel model)
        {
            return Json(model.ApprovalRequest());
        }
        [DisplayName(@"Reject NCL")]
        public ActionResult RejectAction(int id)
        {
            var model = new LeadModel(id);
            var lead = _repo.GetLead(id);
            if (lead != null)
            {
                model.requestType = lead.StatusCode.ToString();
            }
            return PartialView(model);
        }

        [AjaxOnly]
        public ActionResult Action(int leadId)
        {
            return PartialView(_repo.GetLead(leadId));
        }

        [DisplayName(@"Reject NCL")]
        [HttpPost]
        public ActionResult RejectAction(LeadModel model)
        {
            if (string.IsNullOrEmpty(model.Reason))
            {
                return Json("`Reason` must not be empty");
            }
            return Json(model.RejectRequest());
        }

        [DisplayName(@"Mark Call KPI")]
        public ActionResult MarkCallKPI(LeadMarkKPIModel model)
        {

            return Json(new
            {
                success = false
            });
        }

        public ActionResult ImportVoIp()
        {
            var model = new LeadMarkKPIModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult ImportVoIp(LeadMarkKPIModel model)
        {
            model.ImportVoIp();
            model.SessionName = Guid.NewGuid().ToString("N");
            Session["SessionVoIpImport" + model.SessionName] = model;
            return View(model);
        }

        public ActionResult ConfirmKPI(string session)
        {
            var model = (LeadMarkKPIModel)Session["SessionVoIpImport" + session];
            if (model == null)
            {
    
            }
            return View(model);
        }
        [AjaxOnly]
        public ActionResult AjaxGetLeads(int eventId)
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
            var saleId = PermissionHelper.SalesmanId();
            IEnumerable<Lead> leads = new HashSet<Lead>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                leads = _repo.GetAllLeads(m => m.EventID == eventId &&
                                               (saleId == 0 || m.UserID == saleId ||
                                                (m.User != null && m.User.TransferUserID == saleId)) && (
                                                   m.DateCreatedDisplay.Contains(searchValue) ||
                                                   m.StatusDisplay.Contains(searchValue) ||
                                                   m.CompanyName.Contains(searchValue) ||
                                                   m.CountryCode.Contains(searchValue) ||
                                                   m.ClientName.Contains(searchValue) ||
                                                   m.DirectLine.Contains(searchValue) ||
                                                   m.CallBackDateDisplay.Contains(searchValue) ||
                                                   m.Salutation.Contains(searchValue) ||
                                                   m.FirstName.Contains(searchValue) ||
                                                   m.LastName.Contains(searchValue) ||
                                                   m.BusinessPhone.Contains(searchValue) ||
                                                   m.MobilePhone.Contains(searchValue) ||
                                                   m.WorkEmailAddress.Contains(searchValue) ||
                                                   m.WorkEmailAddress1.Contains(searchValue) ||
                                                   m.PersonalEmailAddress.Contains(searchValue)));
            }
            else
            {
                leads = _repo.GetAllLeads(m => m.EventID == eventId && (saleId == 0 || m.UserID == saleId || (m.User != null && m.User.TransferUserID == saleId)));
            }
            // ReSharper disable once AssignNullToNotNullAttribute

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        leads = leads.OrderBy(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        leads = leads.OrderBy(s => s.Company.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        leads = leads.OrderBy(s => s.Company.CountryCode).ThenBy(s => s.ID);
                        break;
                    case "ClientName":
                        leads = leads.OrderBy(s => s.ClientName).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderBy(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "CallBackDate":
                        leads = leads.OrderBy(s => s.CallBackDate).ThenBy(s => s.ID);
                        break;
                    case "Salutation":
                        leads = leads.OrderBy(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        leads = leads.OrderBy(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        leads = leads.OrderBy(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "BusinessPhone":
                        leads = leads.OrderBy(s => s.BusinessPhone).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone":
                        leads = leads.OrderBy(s => s.MobilePhone).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress":
                        leads = leads.OrderBy(s => s.WorkEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress1":
                        leads = leads.OrderBy(s => s.WorkEmailAddress1).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmailAddress":
                        leads = leads.OrderBy(s => s.PersonalEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        leads = leads.OrderBy(s => s.StatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        leads = leads.OrderByDescending(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        leads = leads.OrderByDescending(s => s.Company.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        leads = leads.OrderByDescending(s => s.Company.CountryCode).ThenBy(s => s.ID);
                        break;
                    case "ClientName":
                        leads = leads.OrderByDescending(s => s.ClientName).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderByDescending(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "CallBackDate":
                        leads = leads.OrderByDescending(s => s.CallBackDate).ThenBy(s => s.ID);
                        break;
                    case "Salutation":
                        leads = leads.OrderByDescending(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        leads = leads.OrderByDescending(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        leads = leads.OrderByDescending(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "BusinessPhone":
                        leads = leads.OrderByDescending(s => s.BusinessPhone).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone":
                        leads = leads.OrderByDescending(s => s.MobilePhone).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress":
                        leads = leads.OrderByDescending(s => s.WorkEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress1":
                        leads = leads.OrderByDescending(s => s.WorkEmailAddress1).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmailAddress":
                        leads = leads.OrderByDescending(s => s.PersonalEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        leads = leads.OrderByDescending(s => s.StatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderByDescending(s => s.ID);
                        break;
                }
            }

            #endregion sort

            recordsTotal = leads.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = leads.Skip(skip).Take(pageSize).ToList();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.EventID,
                    CreatedTime = m.CreatedTime.ToString("dd/MM/yyyy HH:mm:ss"),
                    Company = m.Company.CompanyName,
                    Country = m.Company.CountryCode,
                    m.GeneralLine,
                    m.ClientName,
                    m.DirectLine,
                    CallBackDate = m.CallBackDate == default(DateTime) ? "" : m.CallBackDate.ToString("dd/MM/yyyy"),
                    m.Event.EventName,
                    m.Event.EventCode,
                    m.StatusDisplay,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.BusinessPhone,
                    m.MobilePhone,
                    m.WorkEmailAddress,
                    m.WorkEmailAddress1,
                    m.PersonalEmailAddress,
                    m.StatusCode,
                    m.ClassStatus,
                    actionBlock = m.LeadStatusRecord == LeadStatus.Blocked ? "Unblock" : "Block"
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AjaxOnly]
        public ActionResult AjaxGetNCList(int eventId)
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

            IEnumerable<Lead> leads = new HashSet<Lead>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                leads = _repo.GetAllLeads(m => m.EventID == eventId &&
                                               (m.LeadStatusRecord == LeadStatus.Live ||
                                                m.LeadStatusRecord == LeadStatus.LOI ||
                                                m.LeadStatusRecord == LeadStatus.Booked ||
                                                m.LeadStatusRecord == LeadStatus.Blocked) &&
                                               (m.LeadStatusRecord == LeadStatus.Blocked || m.LeadStatusRecord == LeadStatus.Booked || m.LeadStatusRecord.UpdatedTime.Date >=
                                                DateTime.Today.AddDays(-Settings.Lead.NumberDaysExpired())));
            }
            else
            {
                leads = _repo.GetAllLeads(m => m.EventID == eventId &&
                                               (m.LeadStatusRecord == LeadStatus.Live ||
                                                m.LeadStatusRecord == LeadStatus.LOI ||
                                                m.LeadStatusRecord == LeadStatus.Booked ||
                                                m.LeadStatusRecord == LeadStatus.Blocked) &&
                                               (m.LeadStatusRecord == LeadStatus.Blocked || m.LeadStatusRecord == LeadStatus.Booked || m.LeadStatusRecord.UpdatedTime.Date >=
                                                DateTime.Today.AddDays(-Settings.Lead.NumberDaysExpired())));
            }
            // ReSharper disable once AssignNullToNotNullAttribute


            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "DateCreatedDisplay":
                        leads = leads.OrderBy(s => s.LeadStatusRecord.UpdatedTime).ThenBy(s => s.ID);
                        break;
                    case "CompanyName":
                        leads = leads.OrderBy(s => s.Company.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "CountryCode":
                        leads = leads.OrderBy(s => s.Company.CountryCode).ThenBy(s => s.ID);
                        break;
                    case "ClientName":
                        leads = leads.OrderBy(s => s.ClientName).ThenBy(s => s.ID);
                        break;
                    case "Salesman":
                        leads = leads.OrderBy(s => s.User.DisplayName).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        leads = leads.OrderBy(s => s.StatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderBy(s => s.ID);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "DateCreatedDisplay":
                        leads = leads.OrderByDescending(s => s.LeadStatusRecord.UpdatedTime).ThenBy(s => s.ID);
                        break;
                    case "CompanyName":
                        leads = leads.OrderByDescending(s => s.Company.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "CountryCode":
                        leads = leads.OrderByDescending(s => s.Company.CountryCode).ThenBy(s => s.ID);
                        break;
                    case "ClientName":
                        leads = leads.OrderByDescending(s => s.ClientName).ThenBy(s => s.ID);
                        break;
                    case "Salesman":
                        leads = leads.OrderByDescending(s => s.User.DisplayName).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        leads = leads.OrderByDescending(s => s.StatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderByDescending(s => s.ID);
                        break;
                }
            }


            recordsTotal = leads.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = leads.Skip(skip).Take(pageSize).ToList();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.EventID,
                    m.Salesman,
                    DateCreatedDisplay = m.StatusUpdateTimeStr,
                    m.CompanyName,
                    m.CountryCode,
                    m.ClientName,
                    m.ClassStatus,
                    ClassHighlight = m.LeadStatusRecord == LeadStatus.Booked ? "booked" : (m.UserID == CurrentUser.Identity.ID ? "lead_owner" : ""),
                    ClassNewHighlight = m.UserID != CurrentUser.Identity.ID && m.UpdatedTime >= DateTime.Now.AddMinutes(-1) ? "ncl_new" : "",
                    m.StatusDisplay,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult AjaxGetNoCallListForManager(int eventId)
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
            IEnumerable<Lead> leads = new HashSet<Lead>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                leads = _repo.GetAllLeads(m => m.EventID == eventId && m.LeadStatusRecord != LeadStatus.Initial && (
                                                   m.StatusUpdateTimeStr.Contains(searchValue) ||
                                                   m.StatusDisplay.Contains(searchValue) ||
                                                   m.CompanyName.Contains(searchValue) ||
                                                   m.CountryCode.Contains(searchValue) ||
                                                   m.ClientName.Contains(searchValue) ||
                                                   m.DirectLine.Contains(searchValue) ||
                                                   m.CallBackDateDisplay.Contains(searchValue) ||
                                                   m.Salutation.Contains(searchValue) ||
                                                   m.FirstName.Contains(searchValue) ||
                                                   m.LastName.Contains(searchValue) ||
                                                   m.BusinessPhone.Contains(searchValue) ||
                                                   m.MobilePhone.Contains(searchValue) ||
                                                   m.WorkEmailAddress.Contains(searchValue) ||
                                                   m.WorkEmailAddress1.Contains(searchValue) ||
                                                   m.PersonalEmailAddress.Contains(searchValue)));
            }
            else
            {
                leads = _repo.GetAllLeads(m => m.EventID == eventId && m.LeadStatusRecord != LeadStatus.Initial);
            }
            // ReSharper disable once AssignNullToNotNullAttribute

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        leads = leads.OrderBy(s => s.StatusUpdateTime).ThenBy(s => s.StatusCode);
                        break;
                    case "Company":
                        leads = leads.OrderBy(s => s.Company.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "Salesman":
                        leads = leads.OrderBy(s => s.User.DisplayName).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        leads = leads.OrderBy(s => s.Company.CountryCode).ThenBy(s => s.ID);
                        break;
                    case "ClientName":
                        leads = leads.OrderBy(s => s.ClientName).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderBy(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "CallBackDate":
                        leads = leads.OrderBy(s => s.CallBackDate).ThenBy(s => s.ID);
                        break;
                    case "Salutation":
                        leads = leads.OrderBy(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        leads = leads.OrderBy(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        leads = leads.OrderBy(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "BusinessPhone":
                        leads = leads.OrderBy(s => s.BusinessPhone).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone":
                        leads = leads.OrderBy(s => s.MobilePhone).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress":
                        leads = leads.OrderBy(s => s.WorkEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress1":
                        leads = leads.OrderBy(s => s.WorkEmailAddress1).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmailAddress":
                        leads = leads.OrderBy(s => s.PersonalEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        leads = leads.OrderBy(s => s.StatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderBy(s => s.StatusCode).ThenBy(s => s.StatusUpdateTime);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        leads = leads.OrderByDescending(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        leads = leads.OrderByDescending(s => s.Company.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "Salesman":
                        leads = leads.OrderByDescending(s => s.User.DisplayName).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        leads = leads.OrderByDescending(s => s.Company.CountryCode).ThenBy(s => s.ID);
                        break;
                    case "ClientName":
                        leads = leads.OrderByDescending(s => s.ClientName).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderByDescending(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "CallBackDate":
                        leads = leads.OrderByDescending(s => s.CallBackDate).ThenBy(s => s.ID);
                        break;
                    case "Salutation":
                        leads = leads.OrderByDescending(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        leads = leads.OrderByDescending(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        leads = leads.OrderByDescending(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "BusinessPhone":
                        leads = leads.OrderByDescending(s => s.BusinessPhone).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone":
                        leads = leads.OrderByDescending(s => s.MobilePhone).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress":
                        leads = leads.OrderByDescending(s => s.WorkEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress1":
                        leads = leads.OrderByDescending(s => s.WorkEmailAddress1).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmailAddress":
                        leads = leads.OrderByDescending(s => s.PersonalEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        leads = leads.OrderByDescending(s => s.StatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderByDescending(s => s.StatusCode).ThenBy(s => s.StatusUpdateTime);
                        break;
                }
            }

            #endregion sort

            recordsTotal = leads.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = leads.Skip(skip).Take(pageSize).ToList();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.EventID,
                    CreatedTime = m.StatusUpdateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                    Company = m.Company.CompanyName,
                    Country = m.Company.CountryCode,
                    Salesman = m.User.DisplayName,
                    m.GeneralLine,
                    m.ClientName,
                    m.DirectLine,
                    CallBackDate = m.CallBackDate == default(DateTime) ? "" : m.CallBackDate.ToString("dd/MM/yyyy"),
                    m.Event.EventName,
                    m.Event.EventCode,
                    m.StatusDisplay,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.BusinessPhone,
                    m.MobilePhone,
                    m.WorkEmailAddress,
                    m.WorkEmailAddress1,
                    m.PersonalEmailAddress,
                    m.StatusCode,
                    m.ClassStatus,
                    actionBlock = m.LeadStatusRecord == LeadStatus.Blocked ? "Unblock" : "Block"
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult AjaxGetTotalNCL(int eventId)
        {
            var saleId = PermissionHelper.SalesmanId();
            var leads = _repo.GetAllLeads(m => m.EventID == eventId &&
                                               (saleId == 0 || m.UserID == saleId ||
                                                (m.User != null && m.User.TransferUserID == saleId)) &&
                                           (m.LeadStatusRecord == LeadStatus.LOI ||
                                            m.LeadStatusRecord == LeadStatus.Booked ||
                                            m.LeadStatusRecord == LeadStatus.Blocked) &&
                                           (m.LeadStatusRecord == LeadStatus.Blocked || m.LeadStatusRecord == LeadStatus.Booked || m.LeadStatusRecord.UpdatedTime.Date >=
                                            DateTime.Today.AddDays(-Settings.Lead.NumberDaysExpired())));

            return Json(new
            {
                TotalLOI = leads.Count(m => m.LeadStatusRecord == LeadStatus.LOI),
                TotalBlocked = leads.Count(m => m.LeadStatusRecord == LeadStatus.Blocked),
                TotalBooked = leads.Count(m => m.LeadStatusRecord == LeadStatus.Booked)
            }, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult AjaxGetCallKPI()
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
            IEnumerable<Lead> leads = new HashSet<Lead>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                leads = _repo.GetAllLeads(m => (m.LeadStatusRecord == LeadStatus.Live || m.LeadStatusRecord == LeadStatus.LOI || m.LeadStatusRecord == LeadStatus.Booked) && (
                                                   m.StatusUpdateTimeStr.Contains(searchValue) ||
                                                   m.StatusDisplay.Contains(searchValue) ||
                                                   m.CompanyName.Contains(searchValue) ||
                                                   m.CountryCode.Contains(searchValue) ||
                                                   m.ClientName.Contains(searchValue) ||
                                                   m.DirectLine.Contains(searchValue) ||
                                                   m.CallBackDateDisplay.Contains(searchValue) ||
                                                   m.Salutation.Contains(searchValue) ||
                                                   m.FirstName.Contains(searchValue) ||
                                                   m.LastName.Contains(searchValue) ||
                                                   m.BusinessPhone.Contains(searchValue) ||
                                                   m.MobilePhone.Contains(searchValue) ||
                                                   m.WorkEmailAddress.Contains(searchValue) ||
                                                   m.WorkEmailAddress1.Contains(searchValue) ||
                                                   m.PersonalEmailAddress.Contains(searchValue)));
            }
            else
            {
                leads = _repo.GetAllLeads(m => (m.LeadStatusRecord == LeadStatus.Live || m.LeadStatusRecord == LeadStatus.LOI || m.LeadStatusRecord == LeadStatus.Booked));
            }
            // ReSharper disable once AssignNullToNotNullAttribute

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        leads = leads.OrderBy(s => s.StatusUpdateTime).ThenBy(s => s.StatusCode);
                        break;
                    case "Company":
                        leads = leads.OrderBy(s => s.Company.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "Salesman":
                        leads = leads.OrderBy(s => s.User.DisplayName).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        leads = leads.OrderBy(s => s.Company.CountryCode).ThenBy(s => s.ID);
                        break;
                    case "ClientName":
                        leads = leads.OrderBy(s => s.ClientName).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderBy(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "CallBackDate":
                        leads = leads.OrderBy(s => s.CallBackDate).ThenBy(s => s.ID);
                        break;
                    case "Salutation":
                        leads = leads.OrderBy(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        leads = leads.OrderBy(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        leads = leads.OrderBy(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "BusinessPhone":
                        leads = leads.OrderBy(s => s.BusinessPhone).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone":
                        leads = leads.OrderBy(s => s.MobilePhone).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress":
                        leads = leads.OrderBy(s => s.WorkEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress1":
                        leads = leads.OrderBy(s => s.WorkEmailAddress1).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmailAddress":
                        leads = leads.OrderBy(s => s.PersonalEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        leads = leads.OrderBy(s => s.StatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderBy(s => s.StatusCode).ThenBy(s => s.StatusUpdateTime);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "CreatedTime":
                        leads = leads.OrderByDescending(s => s.CreatedTime).ThenBy(s => s.ID);
                        break;
                    case "Company":
                        leads = leads.OrderByDescending(s => s.Company.CompanyName).ThenBy(s => s.ID);
                        break;
                    case "Salesman":
                        leads = leads.OrderByDescending(s => s.User.DisplayName).ThenBy(s => s.ID);
                        break;
                    case "Country":
                        leads = leads.OrderByDescending(s => s.Company.CountryCode).ThenBy(s => s.ID);
                        break;
                    case "ClientName":
                        leads = leads.OrderByDescending(s => s.ClientName).ThenBy(s => s.ID);
                        break;
                    case "DirectLine":
                        leads = leads.OrderByDescending(s => s.DirectLine).ThenBy(s => s.ID);
                        break;
                    case "CallBackDate":
                        leads = leads.OrderByDescending(s => s.CallBackDate).ThenBy(s => s.ID);
                        break;
                    case "Salutation":
                        leads = leads.OrderByDescending(s => s.Salutation).ThenBy(s => s.ID);
                        break;
                    case "FirstName":
                        leads = leads.OrderByDescending(s => s.FirstName).ThenBy(s => s.ID);
                        break;
                    case "LastName":
                        leads = leads.OrderByDescending(s => s.LastName).ThenBy(s => s.ID);
                        break;
                    case "BusinessPhone":
                        leads = leads.OrderByDescending(s => s.BusinessPhone).ThenBy(s => s.ID);
                        break;
                    case "MobilePhone":
                        leads = leads.OrderByDescending(s => s.MobilePhone).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress":
                        leads = leads.OrderByDescending(s => s.WorkEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "WorkEmailAddress1":
                        leads = leads.OrderByDescending(s => s.WorkEmailAddress1).ThenBy(s => s.ID);
                        break;
                    case "PersonalEmailAddress":
                        leads = leads.OrderByDescending(s => s.PersonalEmailAddress).ThenBy(s => s.ID);
                        break;
                    case "StatusDisplay":
                        leads = leads.OrderByDescending(s => s.StatusDisplay).ThenBy(s => s.ID);
                        break;
                    default:
                        leads = leads.OrderByDescending(s => s.StatusCode).ThenBy(s => s.StatusUpdateTime);
                        break;
                }
            }

            #endregion sort

            recordsTotal = leads.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = leads.Skip(skip).Take(pageSize).ToList();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.EventID,
                    CreatedTime = m.StatusUpdateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                    Company = m.Company.CompanyName,
                    Country = m.Company.CountryCode,
                    Salesman = m.User.DisplayName,
                    m.GeneralLine,
                    m.ClientName,
                    m.DirectLine,
                    CallBackDate = m.CallBackDate == default(DateTime) ? "" : m.CallBackDate.ToString("dd/MM/yyyy"),
                    m.Event.EventName,
                    m.Event.EventCode,
                    m.StatusDisplay,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.BusinessPhone,
                    m.MobilePhone,
                    m.WorkEmailAddress,
                    m.WorkEmailAddress1,
                    m.PersonalEmailAddress,
                    m.StatusCode,
                    m.ClassStatus,
                    actionBlock = m.LeadStatusRecord == LeadStatus.Blocked ? "Unblock" : "Block"
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [DisplayName(@"Get List Of Company Resource")]
        public ActionResult CompanyResourceList(int companyID)
        {
            var model =
                _companyRepo.GetAllCompanyResources(m => m.CompanyID == companyID);
            return PartialView(model);
        }
    }
}
