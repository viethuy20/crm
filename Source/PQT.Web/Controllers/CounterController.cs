using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Web.Controllers
{
    public class CounterController : Controller
    {
        //
        // GET: /Admin/Counter/
        private readonly IUnitRepository _unitRepo;

        public CounterController(IUnitRepository unitRepo)
        {
            _unitRepo = unitRepo;
        }
        [DisplayName(@"Counter Management")]
        public ActionResult Index()
        {
            return View(_unitRepo.GetAllCounter());
        }
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(int id = 0)
        {
            var model = new Counter();
            if (id > 0)
            {
                model = _unitRepo.GetCounter(id);
            }
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(Counter model)
        {
            if (ModelState.IsValid)
            {
                if (_unitRepo.UpdateCounter(model))
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
    }
}
