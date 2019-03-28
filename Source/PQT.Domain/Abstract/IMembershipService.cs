using System;
using System.Collections.Generic;
using PQT.Domain.Concrete;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Abstract
{
    public interface IMembershipService : IAuthenticationService
    {
        //string GetTempUserNo();
        int GetCountUsers(string searchValue, bool isHrUser);
        IEnumerable<User> GetUsers(string searchValue, bool isHrUser, string sortColumnDir, string sortColumn, int page, int pageSize);
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
        EmailSetting GetEmailTemplate(string type, string nameTemplate);
        bool ReActiveUser(int id);

        IEnumerable<User> GetAllUsersByLive();
        IEnumerable<User> GetAllUsersForLeave(int supervisorId);
        IEnumerable<User> GetAllSalesmans();
        IEnumerable<User> GetPossibleUsers(string roleName, string searchValue,bool isSalesUser, User currentUser);
        IEnumerable<User> GetAllSupervisors();
        IEnumerable<User> GetAllInterviewers(string[] rolesReviews);
        IEnumerable<User> GetAllSupervisorsAssigned();

    }
}
