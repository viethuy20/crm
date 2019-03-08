using System;
using System.Collections.Generic;
using PQT.Domain.Concrete;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Abstract
{
    public interface IMembershipService : IAuthenticationService
    {
        string GetTempUserNo();
        int GetCountUsers(Func<User, bool> predicate);
        IEnumerable<User> GetUsers(Func<User, bool> predicate, string sortColumnDir, string sortColumn, int page, int pageSize);
        IEnumerable<User> GetUsers(string sortColumnDir, string sortColumn, int page, int pageSize);
        IEnumerable<User> GetUsers(Func<User, bool> predicate);
        IEnumerable<User> GetUsersDeleted();
        User GetUser(int id);
        User GetUserIncludeAll(int id);
        User GetUserByNo(string userNo);
        User GetUserByEmail(string email);
        User CreateUser(User userInfo);
        bool UpdateUser(User userInfo);
        bool UpdateUserIncludeCollection(User userInfo);
        bool DeleteUser(int id);
        IEnumerable<User> GetUsersInRole(params string[] roleName);
        IEnumerable<User> GetUsersInRoleLevel(params string[] roleName);
        EmailSetting GetEmailTemplate(string type, string nameTemplate);
        bool ReActiveUser(int id);

        IEnumerable<User> GetAllSalesmans();

    }
}
