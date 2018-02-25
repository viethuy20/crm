using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface INotificationService<T> where T : class
    {
        void NotifyAll(T entity);
        void NotifyUser(User user, T entity = null);
        void NotifyRole(Role role, T entity = null);
    }
}
