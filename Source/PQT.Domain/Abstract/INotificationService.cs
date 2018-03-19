using System.Collections.Generic;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface INotificationService<T> where T : class
    {
        void NotifyAll(T entity);
        void NotifyUser(IEnumerable<User> users, T entity);
        void NotifyRole(IEnumerable<Role> roles, T entity);
    }
}
