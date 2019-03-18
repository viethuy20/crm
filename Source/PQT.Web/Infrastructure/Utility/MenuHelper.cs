using System.Collections.Generic;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Web.Infrastructure.Utility
{
    public class MenuHelper
    {
        #region Repository

        private static IMenuRepository MenuRepository
        {
            get { return DependencyHelper.GetService<IMenuRepository>(); }
        }

        #endregion

        public static IEnumerable<Menu> GetAll(int parentId = 0)
        {
            if (!CurrentUser.IsAuthenticated)
                return new Menu[] { };

            return MenuRepository.GetAllChildrenByUser(CurrentUser.Identity != null ? CurrentUser.Identity.ID : 0, parentId);
        }
    }
}
