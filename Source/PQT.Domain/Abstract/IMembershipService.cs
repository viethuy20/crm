using System;
using System.Collections.Generic;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Abstract
{
    public interface IMembershipService : IAuthenticationService
    {
        IEnumerable<User> GetUsers(Func<User, bool> predicate);
        IEnumerable<User> GetUsers();
        IEnumerable<User> GetUsersDeleted();
        User GetUser(int id);
        User GetUserByName(string username);
        User GetUserByEmail(string email);
        User CreateUser(User userInfo);
        bool UpdateUser(User userInfo);
        void UpdateUserPicture(int id, string fileName);
        bool DeleteUser(int id);
        IEnumerable<User> GetUsersInRole(params string[] roleName);
        IEnumerable<User> GetUsersContainsInRole(params string[] roleName);
        IEnumerable<User> GetAllUserOfEmailTemplate(string type, string nameTemplate,EmailType emailType);

        bool ReActiveUser(int id);

        IEnumerable<User> GetAllUserByEmail(string email);
        IEnumerable<User> GetAllSalesmans();
    }
}
