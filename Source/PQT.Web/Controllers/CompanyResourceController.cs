using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NS.Entity;
using OfficeOpenXml;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Infrastructure.Utility;
using PQT.Web.Models;
using Resources;

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
            string error = "";
            if (!string.IsNullOrEmpty(model.CompanyResource.DirectLine))
            {
                var countExist =
                    _comRepo.GetAllCompanyResourcesCheckPhone(model.CompanyResource.ID, model.CompanyResource.DirectLine, null, null, null);
                if (countExist > 0)
                {
                    ModelState.AddModelError("CompanyResource.DirectLine", @"Phone number exising");
                    error = "Phone number exising";
                }
            }
            if (!string.IsNullOrEmpty(model.CompanyResource.MobilePhone1))
            {
                var countExist =
                    _comRepo.GetAllCompanyResourcesCheckPhone(model.CompanyResource.ID, null, model.CompanyResource.MobilePhone1, null, null);
                if (countExist > 0)
                {
                    ModelState.AddModelError("CompanyResource.MobilePhone1", @"Phone number exising");
                    error = "Phone number exising";
                }
            }
            if (!string.IsNullOrEmpty(model.CompanyResource.MobilePhone2))
            {
                var countExist =
                    _comRepo.GetAllCompanyResourcesCheckPhone(model.CompanyResource.ID, null, null, model.CompanyResource.MobilePhone2, null);
                if (countExist > 0)
                {
                    ModelState.AddModelError("CompanyResource.MobilePhone2", @"Phone number exising");
                    error = "Phone number exising";
                }
            }
            if (!string.IsNullOrEmpty(model.CompanyResource.MobilePhone3))
            {
                var countExist =
                    _comRepo.GetAllCompanyResourcesCheckPhone(model.CompanyResource.ID, null, null, null, model.CompanyResource.MobilePhone3);
                if (countExist > 0)
                {
                    error = "Phone number exising";
                    ModelState.AddModelError("CompanyResource.MobilePhone3", @"Phone number exising");
                }
            }

            if (string.IsNullOrEmpty(model.CompanyResource.DirectLine) &&
                string.IsNullOrEmpty(model.CompanyResource.MobilePhone1) &&
                string.IsNullOrEmpty(model.CompanyResource.MobilePhone2) &&
                string.IsNullOrEmpty(model.CompanyResource.MobilePhone3))
            {
                ModelState.AddModelError("CompanyResource.DirectLine", @"Phone number must not be empty");
                error = "Phone number must not be empty";
            }
            if (ModelState.IsValid)
            {
                model.CompanyResource.Country = _unitRepo.GetCountry((int)model.CompanyResource.CountryID).Code;
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
                Code = 5,
                error
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
        [HttpPost]
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


        [DisplayName(@"Print Excel Resources ")]
        [HttpPost]
        public ActionResult PrintExcel(CompanyResourceModel model)
        {
            var countries = !string.IsNullOrEmpty(model.Country) ? model.Country.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim()) : new List<string>();
            var organisations = !string.IsNullOrEmpty(model.Organisation) ? model.Organisation.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim()) : new List<string>();
            var searchNames = !string.IsNullOrEmpty(model.Name) ? model.Name.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim()) : new List<string>();
            var mobiles = !string.IsNullOrEmpty(model.Mobile) ? model.Mobile.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim()) : new List<string>();
            var roles = !string.IsNullOrEmpty(model.Role) ? model.Role.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim()) : new List<string>();
            var emails = !string.IsNullOrEmpty(model.Email) ? model.Email.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim()) : new List<string>();
            IEnumerable<CompanyResource> audits =
                _comRepo.GetAllCompanyResources(countries.ToArray(),organisations.ToArray(), roles.ToArray(), searchNames.ToArray(), emails.ToArray(), mobiles.ToArray());
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                var sheet = excelPackage.Workbook.Worksheets.Add("Resource");
                //For title
                sheet.Cells[1, 1].Value = "Country";
                sheet.Cells[1, 1].Style.Font.Bold = true;
                sheet.Cells[1, 2].Value = "Title";
                sheet.Cells[1, 2].Style.Font.Bold = true;
                sheet.Cells[1, 3].Value = "First Name";
                sheet.Cells[1, 3].Style.Font.Bold = true;
                sheet.Cells[1, 4].Value = "Last Name";
                sheet.Cells[1, 4].Style.Font.Bold = true;
                sheet.Cells[1, 5].Value = "Company";
                sheet.Cells[1, 5].Style.Font.Bold = true;
                sheet.Cells[1, 6].Value = "Job Title";
                sheet.Cells[1, 6].Style.Font.Bold = true;
                sheet.Cells[1, 7].Value = "Direct Line";
                sheet.Cells[1, 7].Style.Font.Bold = true;
                sheet.Cells[1, 8].Value = "Mobile Phone1";
                sheet.Cells[1, 8].Style.Font.Bold = true;
                sheet.Cells[1, 9].Value = "Mobile Phone2";
                sheet.Cells[1, 9].Style.Font.Bold = true;
                sheet.Cells[1, 10].Value = "Mobile Phone3";
                sheet.Cells[1, 10].Style.Font.Bold = true;
                sheet.Cells[1, 11].Value = "Work Email";
                sheet.Cells[1, 11].Style.Font.Bold = true;
                sheet.Cells[1, 12].Value = "Personal Email";
                sheet.Cells[1, 12].Style.Font.Bold = true;
                sheet.Cells["A2:L2"].AutoFitColumns();


                var row = 2;
                foreach (var item in audits)
                {
                    sheet.Cells[row, 1].Value = item.Country;
                    sheet.Cells[row, 2].Value = item.Salutation;
                    sheet.Cells[row, 3].Value = item.FirstName;
                    sheet.Cells[row, 4].Value = item.LastName;
                    sheet.Cells[row, 5].Value = item.Organisation;
                    sheet.Cells[row, 6].Value = item.Role;
                    sheet.Cells[row, 7].Value = item.DirectLine;
                    sheet.Cells[row, 8].Value = item.MobilePhone1;
                    sheet.Cells[row, 9].Value = item.MobilePhone2;
                    sheet.Cells[row, 10].Value = item.MobilePhone3;
                    sheet.Cells[row, 11].Value = item.WorkEmail;
                    sheet.Cells[row, 12].Value = item.PersonalEmail;
                    row++;
                }

                var excelByte = excelPackage.GetAsByteArray();
                if (excelByte != null)
                {

                    Response.ClearContent();
                    Response.BinaryWrite(excelByte);
                    Response.AddHeader("content-disposition",
                        "attachment;filename=" + string.Format("CompanyResource{0}.xlsx", DateTime.Now.ToString("MMddHHmmssfff")));
                    Response.ContentType = "application/excel";
                    Response.Flush();
                    Response.End();
                }
            }
            return null;
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


            //var searchValue = "";
            //// ReSharper disable once AssignNullToNotNullAttribute
            //if (Request.Form.GetValues("search[value]").FirstOrDefault() != null)
            //{
            //    // ReSharper disable once PossibleNullReferenceException
            //    searchValue = Request.Form.GetValues("search[value]").FirstOrDefault().Trim().ToLower();
            //}


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var countries = country.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim());
            var organisations = organisation.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim());
            var searchNames = name.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim());
            var mobiles = mobile.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim());
            var roles = role.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim());
            var emails = email.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim());
            int recordsTotal =
                _comRepo.GetCountCompanyResources(countries.ToArray(), organisations.ToArray(), roles.ToArray(), searchNames.ToArray(), emails.ToArray(), mobiles.ToArray());
            var data = _comRepo.GetAllCompanyResources(countries.ToArray(), organisations.ToArray(), roles.ToArray(), searchNames.ToArray(),
                emails.ToArray(), mobiles.ToArray(), sortColumnDir, sortColumn, skip, pageSize);

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

            //var searchValue = "";
            //// ReSharper disable once AssignNullToNotNullAttribute
            //if (Request.Form.GetValues("search[value]").FirstOrDefault() != null)
            //{
            //    // ReSharper disable once PossibleNullReferenceException
            //    searchValue = Request.Form.GetValues("search[value]").FirstOrDefault().Trim().ToLower();
            //}

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
            //var currentUser = CurrentUser.Identity;
            IEnumerable<CompanyResource> companyResources = new HashSet<CompanyResource>();
            //if (eventId > 0 && comId == 0)
            //{
            //    var daysExpired = Settings.Lead.NumberDaysExpired();
            //    var companiesInNcl = _leadRepo.GetAllLeads(m => m.EventID == eventId).Where(m =>
            //        m.UserID != currentUser.ID && m.User.UserStatus == UserStatus.Live &&
            //        m.User.TransferUserID != currentUser.ID &&
            //        m.CheckInNCL(daysExpired)).Select(m => m.CompanyID).Distinct();// get list company blocked
            //    var eventLead = _eventService.GetEvent(eventId);
            //    var assignCompanies = eventLead.EventCompanies.Where(m =>
            //            m.EntityStatus == EntityStatus.Normal && m.Company != null &&
            //            m.Company.EntityStatus == EntityStatus.Normal && !companiesInNcl.Contains(m.CompanyID))
            //        .Select(m => m.CompanyID).Distinct();
            //    companyResources = _comRepo.GetAllCompanyResources(m => m.CompanyID != null && assignCompanies.Contains((int)m.CompanyID));
            //}
            //else 
            if (comId > 0)
                companyResources = _comRepo.GetAllCompanyResources(comId, name, role, email, mobile);
            //companyResources = _comRepo.GetAllCompanyResources(m => m.CompanyID == comId);

            //Func<CompanyResource, bool> predicate = m =>
            //    (string.IsNullOrEmpty(name) ||
            //     (!string.IsNullOrEmpty(m.FullName) && m.FullName.ToLower().Contains(name))) &&
            //    (string.IsNullOrEmpty(mobile) ||
            //     (!string.IsNullOrEmpty(m.DirectLine) && m.DirectLine.ToLower().Contains(mobile)) ||
            //     (!string.IsNullOrEmpty(m.MobilePhone1) && m.MobilePhone1.ToLower().Contains(mobile)) ||
            //     (!string.IsNullOrEmpty(m.MobilePhone2) && m.MobilePhone2.ToLower().Contains(mobile)) ||
            //     (!string.IsNullOrEmpty(m.MobilePhone3) && m.MobilePhone3.ToLower().Contains(mobile))
            //     ) &&
            //     (string.IsNullOrEmpty(email) ||
            //     (!string.IsNullOrEmpty(m.WorkEmail) && m.WorkEmail.ToLower().Contains(email)) ||
            //     (!string.IsNullOrEmpty(m.PersonalEmail) && m.PersonalEmail.ToLower().Contains(email))
            //     ) &&
            //    (string.IsNullOrEmpty(role) ||
            //     (!string.IsNullOrEmpty(m.Role) && m.Role.ToLower().Contains(role)));
            //if (!string.IsNullOrEmpty(searchValue))
            //{
            //    predicate = m =>
            //         (!string.IsNullOrEmpty(m.FullName) && m.FullName.ToLower().Contains(searchValue)) ||
            //         (!string.IsNullOrEmpty(m.DirectLine) && m.DirectLine.ToLower().Contains(searchValue)) ||
            //         (!string.IsNullOrEmpty(m.MobilePhone1) && m.MobilePhone1.ToLower().Contains(searchValue)) ||
            //         (!string.IsNullOrEmpty(m.MobilePhone2) && m.MobilePhone2.ToLower().Contains(searchValue)) ||
            //         (!string.IsNullOrEmpty(m.MobilePhone3) && m.MobilePhone3.ToLower().Contains(searchValue)) ||
            //         (!string.IsNullOrEmpty(m.WorkEmail) && m.WorkEmail.ToLower().Contains(searchValue)) ||
            //         (!string.IsNullOrEmpty(m.PersonalEmail) && m.PersonalEmail.ToLower().Contains(searchValue)) ||
            //         (!string.IsNullOrEmpty(m.Role) && m.Role.ToLower().Contains(searchValue));
            //}
            //companyResources = companyResources.Where(predicate);

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
                    case "DirectLine":
                        companyResources = companyResources.OrderBy(s => s.DirectLine).ThenBy(s => s.Organisation);
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
                    case "DirectLine":
                        companyResources = companyResources.OrderByDescending(s => s.DirectLine).ThenBy(s => s.Organisation);
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
