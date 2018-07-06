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

        //public override Task OnConnected()
        //{
        //    AddConnectionIntoGroups();
        //    return base.OnConnected();
        //}

        //public override Task OnDisconnected()
        //{
        //    RemoveConnectionFromGroups();
        //    return base.OnDisconnected();
        //}

        //public override Task OnReconnected()
        //{
        //    AddConnectionIntoGroups();
        //    return base.OnReconnected();
        //}

        #endregion

        #region Connection manipulation

        public static string GetUserGroupName(string userId)
        {
            return "user_" + userId;
        }

        //private void AddConnectionIntoGroups()
        //{
        //    try
        //    {
        //        User user = MembershipService.GetUserByEmail(Context.User.Identity.Name);
        //        if (user != null)
        //        {
        //            // add connection to group by user
        //            Groups.Add(Context.ConnectionId, GetUserGroupName(user.ID));
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //    }
        //}

        //private void RemoveConnectionFromGroups()
        //{
        //    try
        //    {
        //        User user = MembershipService.GetUserByEmail(Context.User.Identity.Name);
        //        if (user != null)
        //        {
        //            Groups.Remove(Context.ConnectionId, GetUserGroupName(user.ID));
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //    }
        //}

        #endregion
        public static void SendMessage(int userId, string msg)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            hubContext.Clients.Group(GetUserGroupName(userId.ToString())).sendMessage(msg);
        }
    }
}