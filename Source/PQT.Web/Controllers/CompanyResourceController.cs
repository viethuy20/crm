using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Infrastructure.Utility;
using PQT.Web.Models;

namespace PQT.Web.Controllers
{
    public class CompanyResourceController : Controller
    {
        //
        // GET: /CompanyResource/
        private readonly ICompanyRepository _comRepo;
        private readonly ILeadService _leadRepo;
        private readonly IUnitRepository _unitRepo;
        private readonly IEventService _eventService;
        public CompanyResourceController(ICompanyRepository comRepo, ILeadService leadRepo, IUnitRepository unitRepo, IEventService eventService)
        {
            _comRepo = comRepo;
            _leadRepo = leadRepo;
            _unitRepo = unitRepo;
            _eventService = eventService;
        }
        public ActionResult Index()
        {
            var model = new CompanyResourceModel();
            return View(model);
        }

        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(int id = 0)
        {
            var model = new CompanyResourceModel();
            model.Prepare(id);
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(CompanyResourceModel model)
        {
            if (model.CompanyResource.CountryID == null || model.CompanyResource.CountryID == 0)
            {
                return Json(new
                {
                    Code = 6,
                    error = "Country should not be empty."
                });
            }
            if (ModelState.IsValid)
            {
                model.CompanyResource.Country = _unitRepo.GetCountry((int)model.CompanyResource.CountryID).Name;
                if (model.CompanyResource.ID == 0)
                {
                    TransactionWrapper.Do(() =>
                    {
                        var comExist = _comRepo.GetCompany(model.CompanyResource.Organisation);
                        if (comExist == null)
                        {
                            var newCom = new Company
                            {
                                CountryID = model.CompanyResource.CountryID,
                                CompanyName = model.CompanyResource.Organisation,
                            };
                            newCom = _comRepo.CreateCompany(newCom, new List<int>());
                            model.CompanyResource.CompanyID = newCom.ID;
                        }
                        else
                        {
                            model.CompanyResource.CompanyID = comExist.ID;
                            //_comRepo.UpdateCompany(comExist);
                        }
                        if (_comRepo.CreateCompanyResource(model.CompanyResource) != null)
                        {
                            return Json(new
                            {
                                Code = 1,
                                Model = model.CompanyResource
                            });
                        }
                        return Json(new
                        {
                            Code = 2
                        });
                    });
                }
                return TransactionWrapper.Do(() =>
                {
                    var comExist = _comRepo.GetCompany(model.CompanyResource.Organisation);
                    if (comExist == null)
                    {
                        var newCom = new Company
                        {
                            CountryID = model.CompanyResource.CountryID,
                            CompanyName = model.CompanyResource.Organisation,
                        };
                        newCom = _comRepo.CreateCompany(newCom, new List<int>());
                        model.CompanyResource.CompanyID = newCom.ID;
                    }
                    else
                    {
                        model.CompanyResource.CompanyID = comExist.ID;
                        //comExist.BusinessUnit = model.CompanyResource.BusinessUnit;
                        //comExist.BudgetMonth = model.CompanyResource.BudgetMonth;
                        //_comRepo.UpdateCompany(comExist);
                    }

                    if (_comRepo.UpdateCompanyResource(model.CompanyResource))
                    {
                        return Json(new
                        {
                            Code = 3,
                            Model = model.CompanyResource
                        });
                    }
                    return Json(new
                    {
                        Code = 4
                    });
                });
            }
            return Json(new
            {
                Code = 5
            });
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
                    TempData["error"] = "Import Failed... Format file is wrong";
                    return View(model);
                }
                if (model.ImportRows.Any(m => !string.IsNullOrEmpty(m.Error)))
                {
                    model.SessionName = "SessionComResourceImport" + Guid.NewGuid();
                    Session[model.SessionName] = model;
                    return View(model);
                }
                model.ParseValue();
                model.SessionName = "SessionComResourceImport" + Guid.NewGuid();
                Session[model.SessionName] = model;
                return View(model);
            }
            return View(model);
        }

        [AjaxOnly]
        public ActionResult ImportReview()
        {
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

            var session = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Session").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                session = Request.Form.GetValues("Session").FirstOrDefault();
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            if (!string.IsNullOrEmpty(session))
            {
                var model = (CompanyResourceImportModel)Session[session];
                if (model != null)
                {
                    var resourceJsons = model.ImportRows.AsEnumerable();
                    recordsTotal = resourceJsons.Count();
                    if (pageSize > recordsTotal)
                    {
                        pageSize = recordsTotal;
                    }

                    resourceJsons = resourceJsons.Skip(skip).Take(pageSize).ToList();
                    var json = new
                    {
                        draw = draw,
                        recordsFiltered = recordsTotal,
                        recordsTotal = recordsTotal,
                        data = resourceJsons.Select(m => new
                        {
                            m.Number,
                            m.Country,
                            m.Salutation,
                            m.FirstName,
                            m.LastName,
                            m.Organisation,
                            m.Role,
                            m.DirectLine,
                            m.MobilePhone1,
                            m.MobilePhone2,
                            m.MobilePhone3,
                            m.PersonalEmail,
                            m.WorkEmail,
                            m.Error,
                        })
                    };
                    return Json(json, JsonRequestBehavior.AllowGet);
                }
            }
            var data = new List<CompanyResourceJson>();
            var json1 = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data.Select(m => new
                {
                    m.Number,
                    m.Country,
                    m.Salutation,
                    m.FirstName,
                    m.LastName,
                    m.Organisation,
                    m.Role,
                    m.DirectLine,
                    m.MobilePhone1,
                    m.MobilePhone2,
                    m.MobilePhone3,
                    m.PersonalEmail,
                    m.WorkEmail,
                    m.Error,
                })
            };
            return Json(json1, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (_comRepo.DeleteCompanyResource(id))
            {
                return Json(true);
            }
            return Json(false);
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
                var model = (CompanyResourceImportModel)Session[sessionName];
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

            var country = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Country") != null && !string.IsNullOrEmpty(Request.Form.GetValues("Country").FirstOrDefault()))
            {
                // ReSharper disable once PossibleNullReferenceException
                country = Request.Form.GetValues("Country").FirstOrDefault().Trim().ToLower();
            }
            var organisation = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Organisation") != null && Request.Form.GetValues("Organisation").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                organisation = Request.Form.GetValues("Organisation").FirstOrDefault().Trim().ToLower();
            }
            var name = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Name") != null && !string.IsNullOrEmpty(Request.Form.GetValues("Name").FirstOrDefault()))
            {
                // ReSharper disable once PossibleNullReferenceException
                name = Request.Form.GetValues("Name").FirstOrDefault().Trim().ToLower();
            }
            var mobile = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Mobile") != null && Request.Form.GetValues("Mobile").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                mobile = Request.Form.GetValues("Mobile").FirstOrDefault().Trim().ToLower();
            }
            var email = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Email") != null && Request.Form.GetValues("Email").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                email = Request.Form.GetValues("Email").FirstOrDefault().Trim().ToLower();
            }
            var role = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Role") != null && Request.Form.GetValues("Role").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                role = Request.Form.GetValues("Role").FirstOrDefault().Trim().ToLower();
            }


            var searchValue = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Others") != null && Request.Form.GetValues("Others").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                searchValue = Request.Form.GetValues("Others").FirstOrDefault().Trim().ToLower();
            }


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            IEnumerable<CompanyResource> audits = new HashSet<CompanyResource>();
            if (!string.IsNullOrEmpty(searchValue))
            {
                if (!string.IsNullOrEmpty(searchValue))
                    audits = _comRepo.GetAllCompanyResources(m =>
                        (string.IsNullOrEmpty(country) ||
                         (!string.IsNullOrEmpty(m.Country) && m.Country.ToLower().Contains(country))) &&
                        (string.IsNullOrEmpty(organisation) ||
                         (!string.IsNullOrEmpty(m.Organisation) && m.Organisation.ToLower().Contains(organisation))) &&
                        (string.IsNullOrEmpty(name) ||
                         (!string.IsNullOrEmpty(m.FullName) && m.FullName.ToLower().Contains(name))) &&
                        (string.IsNullOrEmpty(mobile) ||
                         (!string.IsNullOrEmpty(m.DirectLine) && m.DirectLine.ToLower().Contains(mobile)) ||
                         (!string.IsNullOrEmpty(m.MobilePhone1) && m.MobilePhone1.ToLower().Contains(mobile)) ||
                         (!string.IsNullOrEmpty(m.MobilePhone2) && m.MobilePhone2.ToLower().Contains(mobile)) ||
                         (!string.IsNullOrEmpty(m.MobilePhone3) && m.MobilePhone3.ToLower().Contains(mobile))
                        ) &&
                        (string.IsNullOrEmpty(email) ||
                         (!string.IsNullOrEmpty(m.WorkEmail) && m.WorkEmail.ToLower().Contains(email)) ||
                         (!string.IsNullOrEmpty(m.PersonalEmail) && m.PersonalEmail.ToLower().Contains(email))
                        ) &&
                        (string.IsNullOrEmpty(role) ||
                         (!string.IsNullOrEmpty(m.Role) && m.Role.ToLower().Contains(role))) &&
                        (m.Country != null && m.Country.ToLower().Contains(searchValue)) ||
                        m.Organisation.ToLower().Contains(searchValue) ||
                        m.LastName.ToLower().Contains(searchValue) ||
                        m.FirstName.ToLower().Contains(searchValue) ||
                        m.Role.ToLower().Contains(searchValue) ||
                        (m.DirectLine != null && m.DirectLine.Contains(searchValue)) ||
                        (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                        (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                        (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                        (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                        (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue))
                    );
            }
            else
            {
                audits = _comRepo.GetAllCompanyResources(m =>
                    (string.IsNullOrEmpty(country) ||
                     (!string.IsNullOrEmpty(m.Country) && m.Country.ToLower().Contains(country))) &&
                    (string.IsNullOrEmpty(organisation) ||
                     (!string.IsNullOrEmpty(m.Organisation) && m.Organisation.ToLower().Contains(organisation))) &&
                    (string.IsNullOrEmpty(name) ||
                     (!string.IsNullOrEmpty(m.FullName) && m.FullName.ToLower().Contains(name))) &&
                    (string.IsNullOrEmpty(mobile) ||
                     (!string.IsNullOrEmpty(m.DirectLine) && m.DirectLine.ToLower().Contains(mobile)) ||
                     (!string.IsNullOrEmpty(m.MobilePhone1) && m.MobilePhone1.ToLower().Contains(mobile)) ||
                     (!string.IsNullOrEmpty(m.MobilePhone2) && m.MobilePhone2.ToLower().Contains(mobile)) ||
                     (!string.IsNullOrEmpty(m.MobilePhone3) && m.MobilePhone3.ToLower().Contains(mobile))
                    ) &&
                    (string.IsNullOrEmpty(email) ||
                     (!string.IsNullOrEmpty(m.WorkEmail) && m.WorkEmail.ToLower().Contains(email)) ||
                     (!string.IsNullOrEmpty(m.PersonalEmail) && m.PersonalEmail.ToLower().Contains(email))
                    ) &&
                    (string.IsNullOrEmpty(role) ||
                     (!string.IsNullOrEmpty(m.Role) && m.Role.ToLower().Contains(role))));
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
                    case "DirectLine":
                        audits = audits.OrderBy(s => s.DirectLine).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone1":
                        audits = audits.OrderBy(s => s.MobilePhone1).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone2":
                        audits = audits.OrderBy(s => s.MobilePhone2).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone3":
                        audits = audits.OrderBy(s => s.MobilePhone3).ThenBy(s => s.Organisation);
                        break;
                    case "WorkEmail":
                        audits = audits.OrderBy(s => s.WorkEmail).ThenBy(s => s.Organisation);
                        break;
                    case "PersonalEmail":
                        audits = audits.OrderBy(s => s.PersonalEmail).ThenBy(s => s.Organisation);
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
                    case "DirectLine":
                        audits = audits.OrderByDescending(s => s.DirectLine).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone1":
                        audits = audits.OrderByDescending(s => s.MobilePhone1).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone2":
                        audits = audits.OrderByDescending(s => s.MobilePhone2).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone3":
                        audits = audits.OrderByDescending(s => s.MobilePhone3).ThenBy(s => s.Organisation);
                        break;
                    case "WorkEmail":
                        audits = audits.OrderByDescending(s => s.WorkEmail).ThenBy(s => s.Organisation);
                        break;
                    case "PersonalEmail":
                        audits = audits.OrderByDescending(s => s.PersonalEmail).ThenBy(s => s.Organisation);
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
                    m.DirectLine,
                    m.MobilePhone1,
                    m.MobilePhone2,
                    m.MobilePhone3,
                    m.PersonalEmail,
                    m.WorkEmail,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        [AjaxOnly]
        public ActionResult AjaxGetCompanyResourceForCall(int eventId = 0)
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

            var comId = 0;
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("ComId") != null && !string.IsNullOrEmpty(Request.Form.GetValues("ComId").FirstOrDefault()))
            {
                // ReSharper disable once PossibleNullReferenceException
                comId = Convert.ToInt32(Request.Form.GetValues("ComId").FirstOrDefault());
            }
            var name = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Name") != null && !string.IsNullOrEmpty(Request.Form.GetValues("Name").FirstOrDefault()))
            {
                // ReSharper disable once PossibleNullReferenceException
                name = Request.Form.GetValues("Name").FirstOrDefault().Trim().ToLower();
            }
            var mobile = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Mobile") != null && Request.Form.GetValues("Mobile").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                mobile = Request.Form.GetValues("Mobile").FirstOrDefault().Trim().ToLower();
            }
            var email = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Email") != null && Request.Form.GetValues("Email").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                email = Request.Form.GetValues("Email").FirstOrDefault().Trim().ToLower();
            }
            var role = "";
            // ReSharper disable once AssignNullToNotNullAttribute
            if (Request.Form.GetValues("Role") != null && Request.Form.GetValues("Role").FirstOrDefault() != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                role = Request.Form.GetValues("Role").FirstOrDefault().Trim().ToLower();
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int page = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var currentUser = CurrentUser.Identity;
            IEnumerable<CompanyResource> companyResources = new HashSet<CompanyResource>();
            if (eventId > 0 && comId == 0)
            {
                var daysExpired = Settings.Lead.NumberDaysExpired();
                var companiesInNcl = _leadRepo.GetAllLeads(m => m.EventID == eventId).Where(m =>
                    m.UserID != currentUser.ID && m.User.TransferUserID != currentUser.ID &&
                    m.CheckInNCL(daysExpired)).Select(m => m.CompanyID).Distinct();// get list company blocked
                var eventLead = _eventService.GetEvent(eventId);
                var assignCompanies = eventLead.EventCompanies.Where(m =>
                        m.EntityStatus == EntityStatus.Normal && m.Company != null &&
                        m.Company.EntityStatus == EntityStatus.Normal && !companiesInNcl.Contains(m.CompanyID))
                    .Select(m => m.CompanyID).Distinct();
                companyResources = _comRepo.GetAllCompanyResources(m => m.CompanyID != null && assignCompanies.Contains((int)m.CompanyID));
            }
            else if (comId > 0)
                companyResources = _comRepo.GetAllCompanyResources(m => m.CompanyID == comId);

            Func<CompanyResource, bool> predicate = m =>
                (string.IsNullOrEmpty(name) ||
                 (!string.IsNullOrEmpty(m.FullName) && m.FullName.ToLower().Contains(name))) &&
                (string.IsNullOrEmpty(mobile) ||
                 (!string.IsNullOrEmpty(m.DirectLine) && m.DirectLine.ToLower().Contains(mobile)) ||
                 (!string.IsNullOrEmpty(m.MobilePhone1) && m.MobilePhone1.ToLower().Contains(mobile)) ||
                 (!string.IsNullOrEmpty(m.MobilePhone2) && m.MobilePhone2.ToLower().Contains(mobile)) ||
                 (!string.IsNullOrEmpty(m.MobilePhone3) && m.MobilePhone3.ToLower().Contains(mobile))
                 ) &&
                 (string.IsNullOrEmpty(email) ||
                 (!string.IsNullOrEmpty(m.WorkEmail) && m.WorkEmail.ToLower().Contains(email)) ||
                 (!string.IsNullOrEmpty(m.PersonalEmail) && m.PersonalEmail.ToLower().Contains(email))
                 ) &&
                (string.IsNullOrEmpty(role) ||
                 (!string.IsNullOrEmpty(m.Role) && m.Role.ToLower().Contains(role)));
            companyResources = companyResources.Where(predicate);

            #region sort
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "Country":
                        companyResources = companyResources.OrderBy(s => s.Country).ThenBy(s => s.Organisation);
                        break;
                    case "Salutation":
                        companyResources = companyResources.OrderBy(s => s.Salutation).ThenBy(s => s.Organisation);
                        break;
                    case "FirstName":
                        companyResources = companyResources.OrderBy(s => s.FirstName).ThenBy(s => s.Organisation);
                        break;
                    case "LastName":
                        companyResources = companyResources.OrderBy(s => s.LastName).ThenBy(s => s.Organisation);
                        break;
                    case "Organisation":
                        companyResources = companyResources.OrderBy(s => s.Organisation).ThenBy(s => s.Organisation);
                        break;
                    case "Role":
                        companyResources = companyResources.OrderBy(s => s.Role).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone1":
                        companyResources = companyResources.OrderBy(s => s.MobilePhone1).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone2":
                        companyResources = companyResources.OrderBy(s => s.MobilePhone2).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone3":
                        companyResources = companyResources.OrderBy(s => s.MobilePhone3).ThenBy(s => s.Organisation);
                        break;
                    case "WorkEmail":
                        companyResources = companyResources.OrderBy(s => s.WorkEmail).ThenBy(s => s.Organisation);
                        break;
                    case "PersonalEmail":
                        companyResources = companyResources.OrderBy(s => s.PersonalEmail).ThenBy(s => s.Organisation);
                        break;
                    case "Remarks":
                        companyResources = companyResources.OrderBy(s => s.Remarks).ThenBy(s => s.Organisation);
                        break;
                    default:
                        companyResources = companyResources.OrderBy(s => s.ID).ThenBy(s => s.Organisation);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "Country":
                        companyResources = companyResources.OrderByDescending(s => s.Country).ThenBy(s => s.Organisation);
                        break;
                    case "Salutation":
                        companyResources = companyResources.OrderByDescending(s => s.Salutation).ThenBy(s => s.Organisation);
                        break;
                    case "FirstName":
                        companyResources = companyResources.OrderByDescending(s => s.FirstName).ThenBy(s => s.Organisation);
                        break;
                    case "LastName":
                        companyResources = companyResources.OrderByDescending(s => s.LastName).ThenBy(s => s.Organisation);
                        break;
                    case "Organisation":
                        companyResources = companyResources.OrderByDescending(s => s.Organisation).ThenBy(s => s.Organisation);
                        break;
                    case "Role":
                        companyResources = companyResources.OrderByDescending(s => s.Role).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone1":
                        companyResources = companyResources.OrderByDescending(s => s.MobilePhone1).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone2":
                        companyResources = companyResources.OrderByDescending(s => s.MobilePhone2).ThenBy(s => s.Organisation);
                        break;
                    case "MobilePhone3":
                        companyResources = companyResources.OrderByDescending(s => s.MobilePhone3).ThenBy(s => s.Organisation);
                        break;
                    case "WorkEmail":
                        companyResources = companyResources.OrderByDescending(s => s.WorkEmail).ThenBy(s => s.Organisation);
                        break;
                    case "PersonalEmail":
                        companyResources = companyResources.OrderByDescending(s => s.PersonalEmail).ThenBy(s => s.Organisation);
                        break;
                    case "Remarks":
                        companyResources = companyResources.OrderByDescending(s => s.Remarks).ThenBy(s => s.Organisation);
                        break;
                    default:
                        companyResources = companyResources.OrderByDescending(s => s.ID).ThenBy(s => s.Organisation);
                        break;
                }
            }

            #endregion sort

            recordsTotal = companyResources.Count();
            if (pageSize > recordsTotal)
            {
                pageSize = recordsTotal;
            }
            var data = companyResources.Skip(page).Take(pageSize).ToList();

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
                    m.DirectLine,
                    m.MobilePhone1,
                    m.MobilePhone2,
                    m.MobilePhone3,
                    m.WorkEmail,
                    m.PersonalEmail,
                    m.Remarks,
                })
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

    }
}
