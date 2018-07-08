using System;
using System.Threading.Tasks;
using System.Web.Helpers;
using AutoMapper;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Models;
using PQT.Web.Infrastructure;
using PQT.Web.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Hubs
{
    // ReSharper disable once ClassNeverInstantiated.Global
    //public class MyConnectionFactory : IConnectionIdFactory
    //{
    //    public string CreateConnectionId(IRequest request)
    //    {
    //        //if (Request.Cookies["srconnectionid"] != null)
    //        //{
    //        //    return Request.Cookies["srconnectionid"];
    //        //}
    //        return Guid.NewGuid().ToString();
    //    }
    //}

    public class NotificationHub : Hub
    {
        private static IMembershipService MembershipService
        {
            get { return DependencyHelper.GetService<IMembershipService>(); }
        }

        #region Override

        public override Task OnConnected()
        {
            var userID = Context.QueryString["UserID"];
            if (!string.IsNullOrEmpty(userID))
                AddConnectionIntoGroups(userID);
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            var userID = Context.QueryString["UserID"];
            if (!string.IsNullOrEmpty(userID))
                RemoveConnectionFromGroups(userID);
            return base.OnDisconnected();
        }
        public override Task OnReconnected()
        {
            var userID = Context.QueryString["UserID"];
            if (!string.IsNullOrEmpty(userID))
                AddConnectionIntoGroups(userID);
            return base.OnReconnected();
        }

        #endregion

        #region Connection manipulation


        public static string GetUserGroupName(string userId)
        {
            return "user_" + userId;
        }

        private void AddConnectionIntoGroups(string userId)
        {
            try
            {

                Groups.Add(Context.ConnectionId, GetUserGroupName(userId));
                //User user = MembershipService.GetUserByEmail(Context.User.Identity.Name);
                //if (user != null)
                //{
                //    // add connection to group by user
                //    Groups.Add(Context.ConnectionId, GetUserGroupName(user));
                //}
            }
            catch (Exception e)
            {
            }
        }

        private void RemoveConnectionFromGroups(string userId)
        {
            try
            {
                Groups.Remove(Context.ConnectionId, GetUserGroupName(userId));
                //User user = MembershipService.GetUserByEmail(Context.User.Identity.Name);
                //if (user != null)
                //{
                //    Groups.Remove(Context.ConnectionId, GetUserGroupName(user));
                //}
            }
            catch (Exception e)
            {
            }
        }

        #endregion

        #region Client interactive

        public static void Notify(string message, string dataType)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.All.notify(message, dataType);
        }

        public static void NotifyUser(User user, string message, string dataType)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(GetUserGroupName(user.ID.ToString())).notify(message, dataType);
        }

        public static void Notify(Lead lead)
        {
            Notify(Json.Encode(lead.Serializing()), GetEntityName(lead));
        }
        public static void Notify(Booking booking)
        {
            Notify(Json.Encode(booking.Serializing()), GetEntityName(booking));
        }
        public static void NotifyUser(User user, UserNotification notify)
        {
            NotifyUser(user, Json.Encode(notify), GetEntityName(notify));
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
