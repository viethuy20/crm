using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;

namespace PQT.Domain.Concrete
{
    public class EFSalesGroupService : Repository, ISalesGroupService
    {
        public EFSalesGroupService(DbContext db)
            : base(db)
        {
        }

        public IEnumerable<SalesGroup> GetAllSalesGroups()
        {
            return GetAll<SalesGroup>().AsEnumerable();
        }

        public SalesGroup GetSalesGroup(int id)
        {
            return Get<SalesGroup>(id);
        }
        public SalesGroup GetSalesGroup(string name)
        {
            return Get<SalesGroup>(m=>m.GroupName.Trim().ToUpper() == name.Trim().ToUpper());
        }

        public SalesGroup CreateSalesGroup(SalesGroup info, IEnumerable<int> users)
        {
            info.Users = GetAll<User>(r => users.Contains(r.ID)).ToList();
            return Create(info);
        }

        public SalesGroup UpdateSalesGroup(int id, string groupName, IEnumerable<int> users)
        {
            return TransactionWrapper.Do(() =>
            {
                var groupExist = Get<SalesGroup>(id);
                if (groupExist == null) return null;
                groupExist.GroupName = groupName;
                groupExist.Users.Clear();
                groupExist.Users = GetAll<User>(r => users.Contains(r.ID)).ToList();
                Update(groupExist);
                return groupExist;
            });
        }

        public bool DeleteSalesGroup(int id)
        {
            return Delete<SalesGroup>(id);
        }
        public IEnumerable<User> GetAllSalesmans()
        {
            return GetAll<User>().AsEnumerable().Where(u => u.Roles.Any(r => r.RoleLevel == RoleLevel.SalesLevel));
        }

    }
}
