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
    public class SalesGroupController : Controller
    {
        //
        // GET: /SalesGroup/
        private readonly ISalesGroupService _repo;
        private readonly IMembershipService _memRepo;

        public SalesGroupController(ISalesGroupService repo, IMembershipService memRepo)
        {
            _repo = repo;
            _memRepo = memRepo;
        }

        [DisplayName(@"Sales Group management")]
        public ActionResult Index()
        {
            var models = _repo.GetAllSalesGroups();
            return View(models);
        }

        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(int id = 0)
        {
            var model = new SalesGroupModel();
            if (id > 0)
            {
                var group = _repo.GetSalesGroup(id);
                if (group!=null)
                {
                    model.ID = group.ID;
                    model.GroupName = group.GroupName;
                    model.UsersSelected = group.Users.Select(m=>m.ID).ToList();
                }
            }
            model.Users = _memRepo.GetAllSalesmans();
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(SalesGroupModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ID == 0)
                {
                    model.AddNewGroup();
                    return Json(new
                    {
                        Code = 1,
                        Model = model.SalesGroup
                    });
                }
                else
                {
                    var message = model.UpdateGroup();
                    if (string.IsNullOrEmpty(message))
                    {
                        return Json(new
                        {
                            Code = 3,
                            Model = model.SalesGroup
                        });
                    }
                    return Json(new
                    {
                        Code = 4,
                        message
                    });
                }
            }
            return Json(new
            {
                Code = 5
            });
        }

        public ActionResult Delete(int id)
        {
            if (_repo.DeleteSalesGroup(id))
            {
                return Json(true);
            }
            return Json(false);
        }

    }
}
