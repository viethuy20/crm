using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Models;

namespace PQT.Web.Controllers
{
    public class TrainerController : Controller
    {
        //
        // GET: /SalesGroup/
        private readonly ITrainerService _repo;

        public TrainerController(ITrainerService repo)
        {
            _repo = repo;
        }

        [DisplayName(@"Trainer management")]
        public ActionResult Index()
        {
            var models = _repo.GetAllTrainers();
            return View(models);
        }
        public ActionResult Create()
        {
            var model = new TrainerModel{};
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(TrainerModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Create())
                {
                    TempData["message"] = "Created successful";
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }


        public ActionResult Edit(int id)
        {
            var model = new TrainerModel
            {
                Trainer = _repo.GetTrainer(id),
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(TrainerModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Update())
                {
                    TempData["message"] = "Updated successful";
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }
        public ActionResult Delete(int id)
        {
            if (_repo.DeleteTrainer(id))
            {
                return Json(true);
            }
            return Json(false);
        }

    }
}
