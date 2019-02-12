using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Models;

namespace PQT.Web.Controllers
{
    public class EventCategoryController : Controller
    {
        private readonly IUnitRepository _unitRepo;

        public EventCategoryController(IUnitRepository unitRepo)
        {
            _unitRepo = unitRepo;
        }
        [DisplayName(@"Event Category management")]
        public ActionResult Index()
        {
            var models = _unitRepo.GetEventCategories();
            return View(models);
        }

        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(int id = 0)
        {
            var model = new EventCategory();
            if (id > 0)
            {
                model = _unitRepo.GetEventCategory(id);
            }
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(EventCategory model)
        {
            if (ModelState.IsValid)
            {
                if (model.ID == 0)
                {
                    if (_unitRepo.CreateEventCategory(model) != null)
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
                if (_unitRepo.UpdateEventCategory(model))
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

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (_unitRepo.DeleteEventCategory(id))
            {
                return Json(true);
            }
            return Json(false);
        }

    }
}
