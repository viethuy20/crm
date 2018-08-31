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
    public class NotifySettingController : Controller
    {
        private readonly ISettingRepository _unitRepo;

        public NotifySettingController(ISettingRepository unitRepo)
        {
            _unitRepo = unitRepo;
        }
        [DisplayName(@"Notify Settings management")]
        public ActionResult Index()
        {
            var models = _unitRepo.GetAllNotifySettings();
            return View(models);
        }

        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(int id = 0)
        {
            var model = new NotifySetting();
            if (id > 0)
            {
                model = _unitRepo.GetNotifySetting(id);
            }
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(NotifySetting model)
        {
            if (ModelState.IsValid)
            {
                if (model.ID == 0)
                {
                    if (_unitRepo.CreateNotifySetting(model) != null)
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
                if (_unitRepo.UpdateNotifySetting(model))
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
            if (_unitRepo.DeleteNotifySetting(id))
            {
                return Json(true);
            }
            return Json(false);
        }

    }
}
