using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Models;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Utility;
using Resources;

namespace PQT.Web.Controllers
{
    public class RolesController : Controller
    {
        private readonly IRoleService _repo;

        public RolesController(IRoleService repo)
        {
            _repo = repo;
        }

        [DisplayName(@"Role management")]
        public ActionResult Index()
        {
            IEnumerable<Role> roles = _repo.GetAllRoles();
            return View(roles);
        }

        public ActionResult Create()
        {
            var model = new RoleModel
            {
                Role = new Role(),
                PermissionAdmins = RequestPermissionProvider.GetPermissions(),
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(RoleModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Role.Name))
                ModelState.AddModelError("Role.Name", Resource.TheFieldShouldNotBeEmpty);

            if (string.IsNullOrEmpty(model.Role.RoleLevel))
                ModelState.AddModelError("Role.RoleLevel", Resource.TheFieldShouldNotBeEmpty);

            Role role = _repo.GetRoleByName(model.Role.Name);
            if (role != null)
                ModelState.AddModelError("Role.Name", Resource.NameExists);

            if (!ModelState.IsValid)
            {
                model = new RoleModel
                {
                    Role = model.Role,
                    PermissionAdmins = RequestPermissionProvider.GetPermissions(),
                };
                return View(model);
            }

            List<int> rolePermissions = (from string key in Request.Form.Keys
                                         where key.StartsWith(RoleModel.PermissionNameAdminPrefix)
                                         select key.Replace(RoleModel.PermissionNameAdminPrefix, "").Split(new[] { '.' })
                                             into segments
                                         let target = segments[0]
                                         let right = segments[1]
                                         select _repo.EnsurePermissionRecord(target, right)
                                                 into permission
                                         select permission.ID).ToList();
            _repo.CreateRole(model.Role, rolePermissions);
            TempData["message"] = Resource.SaveSuccessful;
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var model = new RoleModel
            {
                Role = _repo.GetRole(id),
                PermissionAdmins = RequestPermissionProvider.GetPermissions(),
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(RoleModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Role.Name))
                ModelState.AddModelError("Role.Name", Resource.TheFieldShouldNotBeEmpty);

            if (string.IsNullOrEmpty(model.Role.RoleLevel))
                ModelState.AddModelError("Role.RoleLevel", Resource.TheFieldShouldNotBeEmpty);

            Role role = _repo.GetRoleByName(model.Role.Name);
            if (role != null && role.ID != model.Role.ID)
                ModelState.AddModelError("Role.Name", Resource.NameExists);

            if (!ModelState.IsValid)
                return Edit(model.Role.ID);

            List<int> rolePermissions = (from string key in Request.Form.Keys
                                             //where key.StartsWith(RoleModel.PermissionNamePrefix) && Request.Form[key].Contains("true")
                                         where key.StartsWith(RoleModel.PermissionNameAdminPrefix)
                                         select key.Replace(RoleModel.PermissionNameAdminPrefix, "").Split(new[] { '.' })
                                             into segments
                                         let target = segments[0]
                                         let right = segments[1]
                                         select _repo.EnsurePermissionRecord(target, right)
                                                 into permission
                                         select permission.ID).ToList();


            if (_repo.UpdateRole(model.Role.ID, model.Role, rolePermissions) != null)
            {
                TempData["message"] = Resource.SaveSuccessful;
                return RedirectToAction("Index");
            }

            ViewBag.Success = true;
            ViewBag.Message = Resource.SaveSuccessful;
            return Edit(model.Role.ID);
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            _repo.DeleteRole(id);
            return RedirectToAction("Index");
        }
    }
}
