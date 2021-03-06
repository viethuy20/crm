using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Domain.Concrete
{
    public class EFMenuRepository : Repository, IMenuRepository
    {
        public EFMenuRepository(DbContext db) : base(db)
        {
        }

        #region IMenuRepository Members

        public IEnumerable<Menu> GetAll()
        {
            return GetAll<Menu>(m=> m.Roles.Select(r => r.Permissions)).ToList();
        }

        public virtual IEnumerable<Menu> GetAllChildren(int parentId = 0)
        {
            return GetAll<Menu>(m => m.ParentID.Equals(parentId))
                .OrderBy(m => m.Order).ToList();
        }

        public virtual IEnumerable<Menu> GetAllChildrenByUser(int userID,int parentID = 0)
        {
            var user = Get<User>(u => u.ID == userID, u => u.Roles);
            if (user == null || user.Roles.Count == 0)
                return null;

            List<int> userRoles = user.Roles.Select(r => r.ID).ToList();

            // Admin
            if (userRoles.Contains(1))
                return GetAllChildren(parentID);

            return GetAll<Menu>(m => m.ParentID == parentID &&
                                     m.Roles.Select(r => r.ID).Intersect(userRoles).Any(),
                                m => m.Roles)
                .OrderBy(m => m.Order)
                .ToList();
        }

        public virtual Menu Get(int id)
        {
            return Get<Menu>(m => m.ID == id,
                             m => m.Roles);
        }

        public virtual Menu Create(Menu info, int[] menuRoles)
        {
            info.Roles = GetAll<Role>(r => menuRoles.Contains(r.ID)).ToList();

            return Create(info);
        }

        public virtual bool Update(Menu menu, int[] menuRoles)
        {
            var existMenu = Get<Menu>(m => m.ID == menu.ID, m => m.Roles);
            //_db.Entry(existMenu).State = EntityState.Detached;

            menu.Roles = existMenu.Roles;
            var existMN = existMenu.Roles.Where(r => !menuRoles.Contains(r.ID)).ToList();
            foreach (Role removedRole in existMN)
                menu.Roles.Remove(removedRole);

            foreach (int addedRoleID in menuRoles.Where(id => !existMenu.Roles.Select(r => r.ID).Contains(id)))
                menu.Roles.Add(Get<Role>(addedRoleID));

            return Update(menu);
        }

        public virtual bool Delete(int id)
        {
            var menu = Get<Menu>(id);

            if (menu != null)
            {
                List<Menu> children = GetAllChildren(id).ToList();
                children.ForEach(m => Delete(m.ID));
            }

            return base.Delete(menu);
        }

        #endregion
    }
}
