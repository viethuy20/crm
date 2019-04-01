﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NS.Entity;
using NS.Helpers;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Infrastructure.Utility;
using PQT.Web.Models;
using Resources;

namespace PQT.Web.Controllers
{
    public class LeadController : Controller
    {
        //
        // GET: /Lead/
        private readonly ILeadService _repo;
        private readonly IBookingService _bookingService;
        private readonly ICompanyRepository _companyRepo;
        private readonly IEventService _eventService;

        public LeadController(ILeadService repo, ICompanyRepository companyRepo, IBookingService bookingService, IEventService eventService)
        {
            _repo = repo;
            _companyRepo = companyRepo;
            _bookingService = bookingService;
            _eventService = eventService;
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
            var userId = CurrentUser.Identity.ID;
            if (DateTime.Today > model.Event.ClosingDate || (model.Event.DateOfOpen > DateTime.Today && !model.Event.SalesGroups.SelectMany(g => g.Users.Select(u => u.ID)).Contains(userId) &&
                !model.Event.ManagerUsers.Select(u => u.ID).Contains(userId)))
            {
                TempData["error"] = "Closed this event / don't have access permission for this event";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
        [DisplayName(@"No Call List")]
        public ActionResult NCL(int id = 0)
        {
            var model = new LeadModelView();
            model.Prepare(id);
            if (model.Event == null)
            {
                TempData["error"] = "Event not found";
                return RedirectToAction("Index", "Home");
            }
            var userId = CurrentUser.Identity.ID;
            if (!(CurrentUser.HasRole("Finance") || CurrentUser.HasRole("Admin") || CurrentUser.HasRole("QC") || CurrentUser.HasRole("Manager")) && (DateTime.Today > model.Event.ClosingDate || (model.Event.DateOfOpen > DateTime.Today && !model.Event.SalesGroups.SelectMany(g => g.Users.Select(u => u.ID)).Contains(userId) &&
                                                                 !model.Event.ManagerUsers.Select(u => u.ID).Contains(userId))))
            {
                TempData["error"] = "Closed this event / don't have access permission for this event";
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

        public ActionResult Edit(int id)
        {
            var model = new CallingModel();
            model.PrepareCalling(id);
            var currentUser = CurrentUser.Identity;
            if (model.Lead == null)
            {
                TempData["error"] = "Data not found";
            }
            else if (model.Lead.UserID != currentUser.ID && model.Lead.User.TransferUserID != currentUser.ID)
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

            var currentUser = CurrentUser.Identity;
            if (model.Lead.UserID != currentUser.ID && model.Lead.User.TransferUserID != currentUser.ID && !CurrentUser.HasRoleLevel(RoleLevel.ManagerLevel))
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

        [DisplayName(@"Start Call Form")]
        public ActionResult StartCallForm(int id = 0, int comId = 0, int resourceId = 0)
        {
            var model = new CallingModel();
            model.PrepareCall(id, comId, resourceId);
            if (model.Event == null)
            {
                TempData["error"] = "Event not found";
                return RedirectToAction("Index");
            }

            if (model.Event.ClosingDate != null && model.Event.ClosingDate < DateTime.Today)
            {
                TempData["error"] = "Cannot call... Event has closed";
                return RedirectToAction("Index");
            }
            var userId = CurrentUser.Identity.ID;
            if (DateTime.Today > model.Event.ClosingDate || (model.Event.DateOfOpen > DateTime.Today && !model.Event.SalesGroups.SelectMany(g => g.Users.Select(u => u.ID)).Contains(userId) &&
                                                             !model.Event.ManagerUsers.Select(u => u.ID).Contains(userId)))
            {
                TempData["error"] = "Cannot call this event... don't have access permission";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [HttpPost]
        [DisplayName(@"Start Call Form")]
        public ActionResult StartCallForm(CallingModel model)
        {
            if (model.TypeSubmit == "requestnewevent")
            {
                if (string.IsNullOrEmpty(model.NewTopics))
                {
                    ModelState.AddModelError("NewTopics", Resource.TheFieldShouldNotBeEmpty);
                    TempData["error"] = "Please check your inputs";
                }
                if (string.IsNullOrEmpty(model.NewLocations))
                {
                    ModelState.AddModelError("NewLocations", Resource.TheFieldShouldNotBeEmpty);
                    TempData["error"] = "Please check your inputs";
                }
            }
            else if (model.TypeSubmit == "reportcall")
            {
                if (string.IsNullOrEmpty(model.ReportRemark))
                {
                    ModelState.AddModelError("ReportRemark", Resource.TheFieldShouldNotBeEmpty);
                    TempData["error"] = "Please check your inputs";
                }
            }

            if (ModelState.IsValid)
            {
                if (model.TypeSubmit == "reportcall")
                {
                    if (model.CreateReportCall())
                    {
                        TempData["message"] = "Report successful";
                        return RedirectToAction("Index", new { id = model.EventID });
                    }
                    TempData["error"] = "Report failed";
                }
                else if (model.TypeSubmit == "requestnewevent")
                {
                    if (model.CreateNewEvent())
                    {
                        TempData["message"] = "Request successful";
                        //Reload State for form
                        ModelState["NewTopics"].Value = new ValueProviderResult(null, null, System.Globalization.CultureInfo.CurrentCulture);
                        ModelState["NewLocations"].Value = new ValueProviderResult(null, null, System.Globalization.CultureInfo.CurrentCulture);
                        ModelState["NewDateFrom"].Value = new ValueProviderResult(null, null, System.Globalization.CultureInfo.CurrentCulture);
                        ModelState["NewDateTo"].Value = new ValueProviderResult(null, null, System.Globalization.CultureInfo.CurrentCulture);
                        ModelState["Remark"].Value = new ValueProviderResult(null, null, System.Globalization.CultureInfo.CurrentCulture);
                        ModelState["NewTrainingType"].Value = new ValueProviderResult(NewTrainingType.None, null, System.Globalization.CultureInfo.CurrentCulture);
                        model.NewTopics = null;
                        model.NewLocations = null;
                        model.NewDateFrom = null;
                        model.NewDateTo = null;
                        model.Remark = null;
                        model.NewTrainingType = NewTrainingType.None;
                        //return RedirectToAction("StartCallForm", new { id = model.EventID, comId = model.CompanyID });
                        model.PrepareCall(model.EventID, 0, 0);
                        if (model.Event == null)
                        {
                            model.Event = _eventService.GetEvent(model.EventID);
                        }
                        return View(model);
                    }
                    TempData["error"] = "Request failed";
                }
                else
                {
                    var isValid = true;
                    if ((!string.IsNullOrEmpty(model.WorkEmail) &&
                         (model.WorkEmail == model.WorkEmail1 ||
                          model.WorkEmail == model.PersonalEmail)) ||
                        (!string.IsNullOrEmpty(model.WorkEmail1) &&
                         (model.WorkEmail1 == model.WorkEmail ||
                          model.WorkEmail1 == model.PersonalEmail)) ||
                        (!string.IsNullOrEmpty(model.PersonalEmail) &&
                         (model.PersonalEmail == model.WorkEmail ||
                          model.PersonalEmail == model.WorkEmail1)))
                    {
                        TempData["error"] = "02 or more emails cannot be overlapped inside 01 entry";
                        isValid = false;
                        //return RedirectToAction("StartCallForm", new { id = model.EventID });
                    }

                    if ((!string.IsNullOrEmpty(model.MobilePhone1) &&
                         (model.MobilePhone1 == model.MobilePhone2 ||
                          model.MobilePhone1 == model.MobilePhone3)) ||
                        (!string.IsNullOrEmpty(model.MobilePhone2) &&
                         (model.MobilePhone2 == model.MobilePhone1 ||
                          model.MobilePhone2 == model.MobilePhone3)) ||
                        (!string.IsNullOrEmpty(model.MobilePhone3) &&
                         (model.MobilePhone3 == model.MobilePhone1 ||
                          model.MobilePhone3 == model.MobilePhone2))
                    )
                    {
                        TempData["error"] = "02 or more numbers cannot be overlapped inside 01 entry";
                        isValid = false;
                        //return RedirectToAction("StartCallForm", new {id = model.EventID});
                    }
                    if (isValid)
                    {
                        var currentUser = CurrentUser.Identity;
                        var daysExpired = Settings.Lead.NumberDaysExpired();
                        var allLeads = _repo.GetAllLeadsByEventExcludeReopen(model.EventID, 0);
                        var leadExists = allLeads.Where(m =>
                            m.CompanyID == model.CompanyID &&
                            m.UserID != currentUser.ID &&
                            m.User.UserStatus == UserStatus.Live &&
                            m.User.TransferUserID != currentUser.ID &&
                            m.CheckInNCL(daysExpired));
                        if (leadExists.Any())
                        {
                            TempData["error"] = "Cannot process... This company is existing in NCL";
                            return RedirectToAction("Index", new { id = model.EventID });
                        }
                        var emailExists = allLeads.Where(m => ((!string.IsNullOrEmpty(m.WorkEmail) &&
                                                                m.WorkEmail == model.WorkEmail) ||
                                                               (!string.IsNullOrEmpty(m.WorkEmail1) &&
                                                                m.WorkEmail1 == model.WorkEmail1) ||
                                                               (!string.IsNullOrEmpty(m.PersonalEmail) &&
                                                                m.PersonalEmail == model.PersonalEmail)) &&
                                                              (m.UserID == currentUser.ID));
                        if (emailExists.Any())
                        {
                            TempData["error"] = "Email existing in another entry of same event";
                            isValid = false;
                            //return RedirectToAction("StartCallForm", new { id = model.EventID });
                        }


                        var callExists = allLeads.Where(m => ((!string.IsNullOrEmpty(m.DirectLine) &&
                                                               m.DirectLine == model.DirectLine &&
                                                               m.LineExtension == model.LineExtension) ||
                                                              (!string.IsNullOrEmpty(m.MobilePhone1) &&
                                                               m.MobilePhone1 == model.MobilePhone1) ||
                                                              (!string.IsNullOrEmpty(m.MobilePhone2) &&
                                                               m.MobilePhone2 == model.MobilePhone2) ||
                                                              (!string.IsNullOrEmpty(m.MobilePhone3) &&
                                                               m.MobilePhone3 == model.MobilePhone3)) &&
                                                             (m.UserID == currentUser.ID));
                        if (callExists.Any())
                        {
                            TempData["error"] = "Number existing in another entry of same event";
                            isValid = false;
                            //return RedirectToAction("StartCallForm", new { id = model.EventID });
                        }
                    }

                    if (isValid)
                    {
                        if (model.Create())
                        {
                            TempData["message"] = "Call successful";
                            return RedirectToAction("Detail", new { id = model.Lead.ID });
                        }
                        TempData["error"] = "Save failed";
                    }
                }
            }
            model.PrepareCall(model.EventID, 0, 0);
            if (model.Event == null)
            {
                model.Event = _eventService.GetEvent(model.EventID);
            }
            //model.LoadCompanies(model.EventID);
            return View(model);
        }

        [DisplayName(@"Call Back Form")]
        public ActionResult CallingForm(int id = 0)
        {
            var model = new CallingModel();
            model.PrepareCalling(id);
            return View(model);
        }

        [HttpPost]
        [DisplayName(@"Call Back Form")]
        public ActionResult CallingForm(CallingModel model)
        {
            if (model.TypeSubmit == "requestnewevent")
            {
                var errorMessage = "";
                if (string.IsNullOrEmpty(model.NewTopics))
                {
                    errorMessage = "Please check your inputs";
                    ModelState.AddModelError("NewTopics", Resource.TheFieldShouldNotBeEmpty);
                }
                if (string.IsNullOrEmpty(model.NewLocations))
                {
                    errorMessage = "Please check your inputs";
                    ModelState.AddModelError("NewLocations", Resource.TheFieldShouldNotBeEmpty);
                }

                if (string.IsNullOrEmpty(errorMessage))
                {
                    if (model.CreateNewEvent())
                    {
                        TempData["message"] = "Request successful";
                        return RedirectToAction("Index", "NewEvent");
                    }
                }
                model.PrepareCalling(model.LeadID);
                if (model.Event == null)
                {
                    model.Event = _eventService.GetEvent(model.EventID);
                }
                if (string.IsNullOrEmpty(errorMessage))
                    TempData["error"] = "Request failed";
                else
                    TempData["error"] = errorMessage;
                return View(model);
            }
            var isValid = true;
            if ((!string.IsNullOrEmpty(model.WorkEmail) &&
                 (model.WorkEmail == model.WorkEmail1 ||
                  model.WorkEmail == model.PersonalEmail)) ||
                (!string.IsNullOrEmpty(model.WorkEmail1) &&
                 (model.WorkEmail1 == model.WorkEmail ||
                  model.WorkEmail1 == model.PersonalEmail)) ||
                (!string.IsNullOrEmpty(model.PersonalEmail) &&
                 (model.PersonalEmail == model.WorkEmail ||
                  model.PersonalEmail == model.WorkEmail1)))
            {
                TempData["error"] = "02 or more emails cannot be overlapped inside 01 entry";
                isValid = false;
                //return RedirectToAction("StartCallForm", new { id = model.EventID });
            }

            if ((!string.IsNullOrEmpty(model.MobilePhone1) &&
                 (model.MobilePhone1 == model.MobilePhone2 ||
                  model.MobilePhone1 == model.MobilePhone3)) ||
                (!string.IsNullOrEmpty(model.MobilePhone2) &&
                 (model.MobilePhone2 == model.MobilePhone1 ||
                  model.MobilePhone2 == model.MobilePhone3)) ||
                (!string.IsNullOrEmpty(model.MobilePhone3) &&
                 (model.MobilePhone3 == model.MobilePhone1 ||
                  model.MobilePhone3 == model.MobilePhone2))
            )
            {
                TempData["error"] = "02 or more numbers cannot be overlapped inside 01 entry";
                isValid = false;
                //return RedirectToAction("StartCallForm", new {id = model.EventID});
            }
            if (isValid)
            {
                var allLeads = _repo.GetAllLeadsByEventExcludeReopen(model.EventID, model.LeadID);
                var callExists = allLeads.Where(m =>
                    ((!string.IsNullOrEmpty(m.WorkEmail) && m.WorkEmail == model.WorkEmail) ||
                     (!string.IsNullOrEmpty(m.WorkEmail1) && m.WorkEmail1 == model.WorkEmail1) ||
                     (!string.IsNullOrEmpty(m.PersonalEmail) && m.PersonalEmail == model.PersonalEmail) ||
                     (!string.IsNullOrEmpty(m.DirectLine) && m.DirectLine == model.DirectLine &&
                      m.LineExtension == model.LineExtension) ||
                     (!string.IsNullOrEmpty(m.MobilePhone1) && m.MobilePhone1 == model.MobilePhone1) ||
                     (!string.IsNullOrEmpty(m.MobilePhone2) && m.MobilePhone2 == model.MobilePhone2) ||
                     (!string.IsNullOrEmpty(m.MobilePhone3) && m.MobilePhone3 == model.MobilePhone3)));
                if (callExists.Any())
                {
                    TempData["error"] = "Client contact exists in called list";
                    return RedirectToAction("CallingForm", new { id = model.LeadID });
                }
                if (model.Save())
                {
                    TempData["message"] = "Call successful";
                    return RedirectToAction("Detail", new { id = model.LeadID });
                }
                TempData["error"] = "Save failed";
            }

            model.PrepareCalling(model.LeadID);
            if (model.Event == null)
            {
                model.Event = _eventService.GetEvent(model.EventID);
            }
            return View(model);
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
                model.Lead = lead;
            }
            return PartialView(model);
        }

        [DisplayName(@"Reject NCL")]
        [HttpPost]
        public ActionResult RejectAction(LeadModel model)
        {
            if (string.IsNullOrEmpty(model.Reason))
            {
                return Json(new
                {
                    Message = "`Reason` must not be empty",
                    IsSuccess = false
                });
            }
            return Json(model.RejectRequest());
        }
        [AjaxOnly]
        public ActionResult Action(int leadId)
        {
            var lead = _repo.GetLead(leadId);
            if (lead != null)
            {
                lead.Booking = _bookingService.GetBookingByLeadId(leadId);
            }
            return PartialView(lead);
        }

        [DisplayName(@"Edit Call Record")]
        public ActionResult EditCallRecord(int id = 0)
        {
            var call = _repo.GetPhoneCall(id);
            return PartialView(call);
        }

        [DisplayName(@"Edit Call Record")]
        [HttpPost]
        public ActionResult EditCallRecord(PhoneCall model)
        {
            if (_repo.UpdatePhoneCall(model))
            {
                return Json(new
                {
                    Code = 1,
                    Model = model
                });
            }
            return Json(new
            {
                Code = 0
            });
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
            var saleId = CurrentUser.Identity.ID;
            int recordsTotal = _repo.GetCountLeads(eventId, saleId, searchValue);
            var data = _repo.GetAllLeads(eventId, saleId, searchValue, sortColumnDir, sortColumn, skip, pageSize);

            var daysExpired = Settings.Lead.NumberDaysExpired();
            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    m.EventID,
                    CreatedTime = m.StatusUpdateTimeStr,
                    Company = m.Company.CompanyName,
                    Country = m.Company.CountryCodeAndDialing,
                    m.JobTitle,
                    m.DirectLine,
                    CallBackDate = m.CallBackDate == default(DateTime) ? "" : m.CallBackDate.ToString("dd/MM/yyyy"),
                    m.Event.EventName,
                    m.Event.EventCode,
                    m.StatusDisplay,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.MobilePhone1,
                    m.MobilePhone2,
                    m.MobilePhone3,
                    m.PersonalEmail,
                    m.WorkEmail,
                    m.EstimatedDelegateNumber,
                    TrainingBudgetPerHead = m.TrainingBudgetPerHead != null ? Convert.ToDecimal(m.TrainingBudgetPerHead).ToString("N2") : "",
                    m.GoodTrainingMonth,
                    m.StatusCode,
                    m.ClassStatus,
                    m.MarkKPI,
                    m.KPIRemarks,
                    FirstFollowUpStatus = m.FirstFollowUpStatusDisplay,
                    m.FirstFollowUpStatusClass,
                    FinalStatus = m.FinalStatusDisplay,
                    CallBackDateTime = m.CallBackDateTimeDisplay,
                    NCLExpired = m.CheckNCLExpired(daysExpired),
                    actionBlock = m.LeadStatusRecord == LeadStatus.Blocked ? "Unblock" : "Block"
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AjaxOnly]
        public ActionResult AjaxGetNCList(int eventId)
        {
            var currentUser = CurrentUser.Identity;
            if (Request.Form != null && Request.Form.Count == 0)
            {
                var data1 = new List<Lead>();
                return Json(new
                {
                    recordsFiltered = 0,
                    recordsTotal = 0,
                    data = data1.Select(m => new
                    {
                        m.ID,
                        m.EventID,
                        m.Salesman,
                        DateCreatedDisplay = m.StatusUpdateTimeStr,
                        m.CompanyName,
                        m.CountryCode,
                        m.JobTitle,
                        m.ClassStatus,
                        ClassHighlight = m.LeadStatusRecord == LeadStatus.Booked ? "booked" : (
                            m.User.TransferUserID == currentUser.ID || m.UserID == currentUser.ID ? "lead_owner" : ""),
                        ClassNewHighlight = (m.User.TransferUserID != currentUser.ID && m.UserID != currentUser.ID) && m.UpdatedTime >= DateTime.Now.AddMinutes(-1) ? "ncl_new" : "",
                        m.StatusDisplay,
                    })
                }, JsonRequestBehavior.AllowGet);
            }
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

            var daysExpired = Settings.Lead.NumberDaysExpired();
            int recordsTotal = _repo.GetCountLeadsForNCL(eventId, daysExpired, searchValue);
            var data = _repo.GetAllLeadsForNCL(eventId, daysExpired, searchValue, sortColumnDir, sortColumn, skip, pageSize);


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
                    m.JobTitle,
                    m.ClassStatus,
                    ClassHighlight = m.LeadStatusRecord == LeadStatus.Booked ? "booked" : (m.User.TransferUserID == currentUser.ID || m.UserID == currentUser.ID ? "lead_owner" : ""),
                    ClassNewHighlight = m.User.TransferUserID != currentUser.ID && m.UserID != currentUser.ID && m.UpdatedTime >= DateTime.Now.AddMinutes(-1) ? "ncl_new" : "",
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
            var daysExpired = Settings.Lead.NumberDaysExpired();
            int recordsTotal = _repo.GetCountLeadsForNCLManger(eventId, searchValue);
            var data = _repo.GetAllLeadsForNCLManger(eventId, searchValue, sortColumnDir, sortColumn, skip, pageSize);
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
                    Country = m.Company.CountryCodeAndDialing,
                    Salesman = m.User.DisplayName,
                    m.JobTitle,
                    m.DirectLine,
                    CallBackDate = m.CallBackDate == default(DateTime) ? "" : m.CallBackDate.ToString("dd/MM/yyyy"),
                    m.Event.EventName,
                    m.Event.EventCode,
                    m.StatusDisplay,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.MobilePhone1,
                    m.MobilePhone2,
                    m.MobilePhone3,
                    m.PersonalEmail,
                    m.WorkEmail,
                    m.EstimatedDelegateNumber,
                    TrainingBudgetPerHead = m.TrainingBudgetPerHead != null ? Convert.ToDecimal(m.TrainingBudgetPerHead).ToString("N2") : "",
                    m.GoodTrainingMonth,
                    m.StatusCode,
                    m.ClassStatus,
                    FirstFollowUpStatus = m.FirstFollowUpStatusDisplay,
                    FinalStatus = m.FinalStatusDisplay,
                    NCLExpired = m.CheckNCLExpired(daysExpired),
                    actionBlock = m.LeadStatusRecord == LeadStatus.Blocked ? "Unblock" : "Block"
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult AjaxGetTotalNCL(int eventId)
        {
            //var saleId = PermissionHelper.SalesmanId();
            var daysExpired = Settings.Lead.NumberDaysExpired();
            var saleId = CurrentUser.Identity.ID;
            return Json(new
            {
                TotalLOI = _repo.CountLeadsTotalLOINCL(eventId, saleId, daysExpired),
                TotalBlocked = _repo.CountLeadsTotalBlockedNCL(eventId, saleId),
                TotalBooked = _repo.CountLeadsTotalBooked(eventId, saleId),
                TotalKPI = _repo.CountLeadsTotalTotalKPI(eventId, saleId)
            }, JsonRequestBehavior.AllowGet);
        }


        [AjaxOnly]
        public ActionResult AjaxGetTotalBlocked()
        {
            var saleId = CurrentUser.Identity.ID;
            return Json(new
            {
                TotalBlocked = _repo.CountLeadsTotalBlockedNCL(0, saleId),
            }, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult AjaxGetTotalNCLForManager(int eventId)
        {
            var daysExpired = Settings.Lead.NumberDaysExpired();
            return Json(new
            {
                TotalLOI = _repo.CountLeadsTotalLOINCL(eventId, 0, daysExpired),
                TotalBlocked = _repo.CountLeadsTotalBlockedNCL(eventId, 0),
                TotalBooked = _repo.CountLeadsTotalBooked(eventId, 0),
                TotalKPI = _repo.CountLeadsTotalTotalKPI(eventId, 0)
            }, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult EventCompanyInfo(int eventId, int companyId)
        {
            var model = _companyRepo.GetCompany(companyId);
            if (model == null)
            {
                model = new Company();
                return Json(new
                {
                    model.ID,
                    BudgetMonth = model.FinancialYear,
                    model.BusinessUnit,
                    model.Remarks,
                }, JsonRequestBehavior.AllowGet);
            }
            //if (model.UpdatedTime == null)
            //{
            //    var other = _eventService.GetEventCompany(companyId);
            //    if (other != null)
            //    {
            //        model = other;
            //    }
            //}
            return Json(new
            {
                model.ID,
                BudgetMonth = model.FinancialYear,
                model.BusinessUnit,
                model.Remarks,
            }, JsonRequestBehavior.AllowGet);
        }


        [AjaxOnly]
        public ActionResult AjaxGetEventCompanies(int eventId = 0)
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
            var industry = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Industry") != null && Request.Form.GetValues("Industry").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                industry = Request.Form.GetValues("Industry").FirstOrDefault().Trim().ToLower();
            }
            var tier = 0;
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Tier") != null && Request.Form.GetValues("Tier").FirstOrDefault() != null)
            {
                try
                {
                    // ReSharper disable once PossibleNullReferenceException
                    tier = Convert.ToInt32(Request.Form.GetValues("Tier").FirstOrDefault().Trim().ToLower());
                }
                catch
                {

                }
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

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var currentUser = CurrentUser.Identity;
            IEnumerable<int> companiesInNcl = new HashSet<int>();
            var daysExpired = Settings.Lead.NumberDaysExpired();
            companiesInNcl = _repo.GetCompaniesInNCL(eventId,currentUser.ID,daysExpired);// get list company blocked
            int recordsTotal = _eventService.GetCountEventCompaniesForCall(eventId, currentUser.ID, companyName, productService, countryName, tier, sector, industry, businessUnit, ownership);
            var data = _eventService.GetAllEventCompaniesForCall(eventId, currentUser.ID, companyName, productService, countryName, tier, sector, industry, businessUnit, ownership, sortColumnDir, sortColumn, skip, pageSize);

            foreach (var company in data)
            {
                company.BlockStartCall = companiesInNcl.Contains(company.ID);
                company.ComResourceNumber = _companyRepo.CountCompanyResources(company.ID);
            }

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.ID,
                    Country = m.CountryCode,
                    m.ComResourceNumber,
                    m.BlockStartCall,
                    m.CompanyName,
                    m.DialingCode,
                    m.ProductOrService,
                    m.Sector,
                    m.Industry,
                    m.BusinessUnit,
                    m.BudgetMonth,
                    m.FinancialYear,
                    m.Remarks,
                    m.Ownership,
                    m.Tier,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //[AjaxOnly]
        //public ActionResult AjaxGetTotalCallSummary(int eventId)
        //{
        //    //var saleId = PermissionHelper.SalesmanId();
        //    var leads = _repo.GetAllLeads(m => m.EventID == eventId && !m.ExpiredForReopen);// && (saleId == 0 || m.UserID == saleId ||
        //                                                                                    //(m.User != null && m.User.TransferUserID == saleId)));
        //    var eventData = _eventService.GetEvent(eventId);
        //    return Json(new
        //    {
        //        Tier3 = leads.DistinctBy(m => m.CompanyID).Count(m => m.Company != null && m.Company.Tier.ToString() == TierType.Tier3),
        //        Tier1 = leads.DistinctBy(m => m.CompanyID).Count(m => m.Company != null && m.Company.Tier.ToString() == TierType.Tier1),
        //        Tier2 = leads.DistinctBy(m => m.CompanyID).Count(m => m.Company != null && m.Company.Tier.ToString() == TierType.Tier2),
        //        TotalTier3 = eventData.EventCompanies.Count(m => m.EntityStatus == EntityStatus.Normal && m.Company != null && m.Company.EntityStatus == EntityStatus.Normal && m.Company.Tier.ToString() == TierType.Tier3),
        //        TotalTier1 = eventData.EventCompanies.Count(m => m.EntityStatus == EntityStatus.Normal && m.Company != null && m.Company.EntityStatus == EntityStatus.Normal && m.Company.Tier.ToString() == TierType.Tier1),
        //        TotalTier2 = eventData.EventCompanies.Count(m => m.EntityStatus == EntityStatus.Normal && m.Company != null && m.Company.EntityStatus == EntityStatus.Normal && m.Company.Tier.ToString() == TierType.Tier2),
        //        TotalBooked = leads.Count(m => m.LeadStatusRecord == LeadStatus.Booked),
        //    }, JsonRequestBehavior.AllowGet);
        //}
        [AjaxOnly]
        public ActionResult AjaxGetTotalCallSummaries(string eventIds)
        {
            if (string.IsNullOrEmpty(eventIds))
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
            //var saleId = PermissionHelper.SalesmanId();
            var ids = eventIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(m => Convert.ToInt32(m));
            var listObj = new List<object>();
            foreach (var id in ids)
            {
                listObj.Add(new
                {
                    Tier3 = _repo.CountLeadsByTier(id, Convert.ToInt16(TierType.Tier3)),
                    Tier1 = _repo.CountLeadsByTier(id, Convert.ToInt16(TierType.Tier1)),
                    Tier2 = _repo.CountLeadsByTier(id, Convert.ToInt16(TierType.Tier2)),
                    TotalTier3 = _eventService.CountEventCompanies(id, Convert.ToInt16(TierType.Tier3)),
                    TotalTier1 = _eventService.CountEventCompanies(id, Convert.ToInt16(TierType.Tier1)),
                    TotalTier2 = _eventService.CountEventCompanies(id, Convert.ToInt16(TierType.Tier2)),
                    TotalBooked = _repo.CountLeadsTotalBooked(id, 0),
                    EventId = id
                });
            }
            return Json(listObj, JsonRequestBehavior.AllowGet);
        }
    }
}
