using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Helpers;
using PQT.Web.Models;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Filters;
using DependencyHelper = NS.Mvc.DependencyHelper;

namespace PQT.Web.Controllers
{
    public class MenusController : Controller
    {
        private readonly IMenuRepository _menuRepo;
        private readonly IRoleService _roleService;

        public MenusController(IMenuRepository menuRepo, IRoleService roleService)
        {
            _menuRepo = menuRepo;
            _roleService = roleService;
        }
        [DisplayName(@"Menu management")]
        public ActionResult Index()
        {
            IEnumerable<Menu> menu = _menuRepo.GetAllChildren().ToList();
            return View(menu);
        }

        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(int parentID = 0, int id = 0)
        {
            var menu = _menuRepo.Get(id) ?? new Menu { ParentID = parentID, Order = 0 };
            var model = new MenuEditModel
            {
                Menu = menu,
                Roles = _roleService.GetAllRoles().ToList(),
                MenuRoles = menu.Roles.ToList()
            };
            return PartialView(model);
        }
        [HttpPost]
        [DisplayName(@"Create Or Edit")]
        public ActionResult CreateOrEdit(MenuEditModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Menu.ID == 0)
                {
                    if (_menuRepo.Create(model.Menu, model.SelectedRoles.ToArray()) != null)
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
                if (_menuRepo.Update(model.Menu, model.SelectedRoles.ToArray()))
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
            if (_menuRepo.Delete(id))
            {
                return Json(true);
            }
            return Json(false);
        }
        //[AjaxOnly]
        //public ActionResult UpdateMenuResources()
        //{
        //    var memory = DependencyHelper.GetService<MemoryMenuRepository>();
        //    var menus = memory.GetAll().ToList();
        //    try
        //    {
        //        var path1 = Server.MapPath("~/App_GlobalResources/MenuResource.resx");
        //        var xmlDoc1 = new XmlDocument();
        //        xmlDoc1.Load(path1);
        //        bool ischanged = false;
        //        foreach (var menu in menus)
        //        {
        //            var dataname = ("Menu" + Domain.Helpers.StringHelper.RemoveSpecialCharacters(menu.Title)).ToLower();
        //            XmlNodeList nodes = xmlDoc1.SelectNodes("//data[@name='" + dataname + "']");
        //            if (nodes.Count == 0)
        //            {
        //                ischanged = true;
        //                XmlElement data = xmlDoc1.CreateElement("data");
        //                XmlElement value = xmlDoc1.CreateElement("value");
        //                value.InnerText = menu.Title;
        //                XmlElement comment = xmlDoc1.CreateElement("comment");
        //                comment.InnerText = "";
        //                data.SetAttribute("name", dataname);
        //                data.SetAttribute("xml:space", "preserve");
        //                data.AppendChild(value);
        //                data.AppendChild(comment);
        //                xmlDoc1.DocumentElement.AppendChild(data);
        //            }
        //        }
        //        if (ischanged)
        //            xmlDoc1.Save(path1);
        //        return Json(1, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    return Json(0, JsonRequestBehavior.AllowGet);
        //}
    }
}
