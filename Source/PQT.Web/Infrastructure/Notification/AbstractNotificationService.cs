using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using NS.Mail;
using PQT.Web.Hubs;

namespace PQT.Web.Infrastructure.Notification
{
    public abstract class AbstractNotificationService
    {
        protected IMembershipService MembershipService
        {
            get { return DependencyResolver.Current.GetService<IMembershipService>(); }
        }
    }

    public abstract class AbstractNotificationService<T> : AbstractNotificationService, INotificationService<T> where T : class
    {
        #region INotificationService<T> Members

        public abstract void NotifyAll(T entity);

        public abstract void NotifyUser(IEnumerable<User> users, T entity);

        public abstract void NotifyRole(IEnumerable<Role> roles, T entity);

        #endregion
    }
}
