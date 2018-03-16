using System.Threading.Tasks;
using System.Web.Helpers;
using AutoMapper;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Models;
using PQT.Web.Infrastructure;
using PQT.Web.Models;
using Microsoft.AspNet.SignalR;

namespace PQT.Web.Hubs
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class NotificationHub : Hub
    {
        private static IMembershipService MembershipService
        {
            get { return DependencyHelper.GetService<IMembershipService>(); }
        }

        #region Override

        public override Task OnConnected()
        {
            AddConnectionIntoGroups();
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            RemoveConnectionFromGroups();
            return base.OnDisconnected();
        }

        public override Task OnReconnected()
        {
            AddConnectionIntoGroups();
            return base.OnReconnected();
        }

        #endregion

        #region Connection manipulation

        public static string GetRoleGroupName(Role role)
        {
            return "role_" + role.ID;
        }

        public static string GetUserGroupName(User user)
        {
            return "user_" + user.ID;
        }

        private void AddConnectionIntoGroups()
        {
            User user = MembershipService.GetUserByEmail(Context.User.Identity.Name);
            if (user != null)
            {
                // add connection to group by role
                foreach (Role role in user.Roles)
                    Groups.Add(Context.ConnectionId, GetRoleGroupName(role));

                // add connection to group by user
                Groups.Add(Context.ConnectionId, GetUserGroupName(user));
            }
        }

        private void RemoveConnectionFromGroups()
        {
            User user = MembershipService.GetUserByEmail(Context.User.Identity.Name);
            if (user != null)
            {
                foreach (Role role in user.Roles)
                    Groups.Remove(Context.ConnectionId, GetRoleGroupName(role));

                Groups.Remove(Context.ConnectionId, GetUserGroupName(user));
            }
        }

        #endregion

        #region Client interactive

        public static void Notify(string message, string dataType)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.All.notify(message, dataType);
        }

        public static void Notify(UserNotification notify)
        {
            Notify(Json.Encode(notify), GetEntityName(notify));
        }


        #endregion

        #region Helpers

        private static string GetEntityName(object entity)
        {
            string name = entity.GetType().Name;

            int lastUnderscorePos = name.LastIndexOf('_');
            if (lastUnderscorePos >= 0)
                name = name.Substring(0, lastUnderscorePos);

            return name;
        }

        #endregion
    }
}
