using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Web.Models
{
    public class MenuModel
    {
        private readonly IMenuRepository _repo = DependencyResolver.Current.GetService<IMenuRepository>();

        public MenuModel()
        {
        }

        public MenuModel(Menu menu)
        {
            Menu = menu;
            Children = _repo.GetAllChildren(menu.ID).ToList();
        }

        public Menu Menu { get; set; }
        public List<Menu> Children { get; set; }
    }

    public class MenuEditModel
    {
        public Menu Menu { get; set; }
        public List<Role> Roles { get; set; }
        public List<Role> MenuRoles { get; set; }
        public List<int> SelectedRoles { get; set; }
    }
}
