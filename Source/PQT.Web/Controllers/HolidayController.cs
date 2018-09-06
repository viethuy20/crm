using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Web.Infrastructure.Filters;
using PQT.Domain.Entities;
using ServiceStack.Text;

namespace PQT.Web.Controllers
{
    public class HolidayController : Controller
    {
        //
        // GET: /Holiday/
        private readonly ISettingRepository _repo;
        private readonly IUnitRepository _unitRepo;

        public HolidayController(ISettingRepository repo, IUnitRepository unitRepo)
        {
            _repo = repo;
            _unitRepo = unitRepo;
        }
        [DisplayName("Holiday management")]
        public ActionResult Index()
        {
            var currentYear = DateTime.Today.Year;
            return View(_repo.GetAllHolidays(new[] { currentYear - 1, currentYear, currentYear + 1, currentYear + 2 }));
        }
        [AjaxOnly]
        [HttpPost]
        public ActionResult Create(string title, string start, string end, string locationId = "")
        {
            int locationid = 0;
            try
            {
                locationid = !string.IsNullOrEmpty(locationId) ? Convert.ToInt32(locationId) : 0;
            }
            catch (Exception e)
            {
            }
            var holiday = new Holiday
            {
                Description = title,
                StartDate = Convert.ToDateTime(start),
                EndDate = Convert.ToDateTime(end),
                CountryID = locationid > 0 ? locationid : (int?)null
            };
            holiday = _repo.CreateHoliday(holiday);
            if (holiday != null)
            {
                var location = "";
                if (locationid > 0)
                {
                    var locationData = _unitRepo.GetOfficeLocation(locationid);
                    if (locationData!=null)
                    {
                        location = locationData.Name;
                    }
                }
                return Json(new
                {
                    success = true,
                    id = holiday.ID,
                    title = holiday.Description,
                    startUnixTime = holiday.StartDate.ToUnixTime(),
                    start = holiday.StartDate.ToString("yyyy-MM-dd"),
                    end = holiday.EndDate.ToString("yyyy-MM-dd"),
                    holidayDate = holiday.HolidayDate(),
                    location,
                });
            }
            return Json(new { success = false });
        }
        [AjaxOnly]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            return Json(_repo.DeleteHoliday(id) ? new { success = true } : new { success = false });
        }

    }
}
