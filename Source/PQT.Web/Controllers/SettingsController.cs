using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Web.Models;
using PQT.Web.Infrastructure.Filters;

namespace PQT.Web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ISettingRepository _settingRepo;
        public static readonly List<string> LoginRequired = new List<string> { "Dashboard" };
        public SettingsController(ISettingRepository settingRepo)
        {
            _settingRepo = settingRepo;
        }

        //
        // GET: /Settings/
        [DisplayName(@"Setting management")]
        public ViewResult Index()
        {
            return View(_settingRepo.GetSettings());
        }

        [HttpPost]
        [DisplayName(@"Setting management")]
        public ActionResult Index(SettingModel setting)
        {
            if (ModelState.IsValid)
            {
                var lstId = Request["Id"].Split(',');
                foreach (var id in lstId)
                {
                    var item = _settingRepo.GetSetting(Convert.ToInt32(id));
                    item.Value = Request[item.Name].Split(',')[0];
                    _settingRepo.UpdateSetting(item);
                }
            }

            return View(_settingRepo.GetSettings());
        }
    }
}
