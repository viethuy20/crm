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
    public class RecruitmentPositionController : Controller
    {
        private readonly IUnitRepository _unitRepo;

        public RecruitmentPositionController(IUnitRepository unitRepo)
        {
            _unitRepo = unitRepo;
        }
        [DisplayName(@"Recruitment Position management")]
        public ActionResult Index()
        {
            var models = _unitRepo.GetAllRecruitmentPositions();
            return View(models);
        }

        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(int id = 0)
        {
            var model = new RecruitmentPositionModel();
            model.Prepare(id);
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(RecruitmentPositionModel model)
        {
            if (ModelState.IsValid)
            {
                var exist = _unitRepo.GetRecruitmentPosition(model.RecruitmentPosition.Position,
                    model.RecruitmentPosition.OfficeLocationID);
                if (exist != null && exist.ID != model.RecruitmentPosition.ID)
                    return Json(new
                    {
                        Code = 6
                    });
                return Json(model.SaveData());
            }
            return Json(new
            {
                Code = 5
            });
        }

        public ActionResult Delete(int id)
        {
            if (_unitRepo.DeleteRecruitmentPosition(id))
            {
                return Json(true);
            }
            return Json(false);
        }

    }
}
