using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Infrastructure.Filters;

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
            var models = _comRepo.GetAllCompanies();
            return View(models);
        }

        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(int id = 0)
        {
            var model = new Company();
            if (id > 0)
            {
                model = _comRepo.GetCompany(id);
            }
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(Company model)
        {
            if (model.CountryID == 0)
            {
                return Json(new
                {
                    Code = 6,
                    error = "Country should not be empty."
                });
            }
            if (ModelState.IsValid)
            {
                if (model.ID == 0)
                {
                    if (_comRepo.CreateCompany(model) != null)
                    {
                        model.Country = _unitRepo.GetCountry((int)model.CountryID);
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
                if (_comRepo.UpdateCompany(model))
                {
                    model.Country = _unitRepo.GetCountry((int)model.CountryID);
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

        public ActionResult Delete(int id)
        {
            if (_comRepo.DeleteCompany(id))
            {
                return Json(true);
            }
            return Json(false);
        }


    }
}
