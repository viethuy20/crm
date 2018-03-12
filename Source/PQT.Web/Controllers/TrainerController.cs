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
