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
    public class CountryController : Controller
    {
        private readonly IUnitRepository _unitRepo;

        public CountryController(IUnitRepository unitRepo)
        {
            _unitRepo = unitRepo;
        }
        [DisplayName(@"Country management")]
        public ActionResult Index()
        {
            var models = _unitRepo.GetAllCountries();
            return View(models);
        }

        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(int id = 0)
        {
            var model = new Country();
            if (id > 0)
            {
                model = _unitRepo.GetCountry(id);
            }
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(Country model)
        {
            if (ModelState.IsValid)
            {
                if (model.ID == 0)
                {
                    if (_unitRepo.CreateCountry(model) != null)
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
                if (_unitRepo.UpdateCountry(model))
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

        public ActionResult Delete(int id)
        {
            if (_unitRepo.DeleteCountry(id))
            {
                return Json(true);
            }
            return Json(false);
        }

    }
}
