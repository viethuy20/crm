using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Infrastructure;

namespace PQT.Web.Hubs
{
    public class ProgressHub : Hub
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
            }
            catch (Exception e)
            {
            }
        }

        #endregion
        public static void SendMessage(int userId, string msg)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            hubContext.Clients.Group(GetUserGroupName(userId.ToString())).sendMessage(msg);
        }
    }
}