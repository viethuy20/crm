using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NS.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Infrastructure.Utility;
using Quartz;

namespace PQT.Web.Models
{
    public class LeaveModel
    {
        public Leave Leave { get; set; }
        public IEnumerable<User> Users { get; set; }
        public int LeaveID { get; set; }
        public string Message { get; set; }
        public HttpPostedFileBase DocumentFile { get; set; }
        public void Prepare(int id = 0)
        {
            var leaveService = DependencyHelper.GetService<ILeaveService>();
            Leave = id > 0 ? leaveService.GetLeave(id) : new Leave();
            var memberService = DependencyHelper.GetService<IMembershipService>();
            Users = memberService.GetAllUsersForLeave(CurrentUser.Identity.ID);

        }
        public bool Create()
        {
            var leaveService = DependencyHelper.GetService<ILeaveService>();
            //var memberService = DependencyHelper.GetService<IMembershipService>();
            //Leave.LeaveStatus = LeaveStatus.Request;
            Leave.CreatedUserID = CurrentUser.Identity.ID;
            if (DocumentFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Leave, DocumentFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Leave.Document = uploadPicture;
                }
            }
            Leave = leaveService.CreateLeave(Leave);
            if (Leave != null)
            {
                //var supervisors = memberService.GetAllSupervisors().Where(m => m.ID != Leave.UserID);
                //LeaveNotificator.NotifyUser(NotifyAction.Request, supervisors.ToList(), Leave.ID, Leave.LeaveType.DisplayName + " Request ");
                return true;
            }
            return false;
        }
        public bool Update()
        {
            var leaveService = DependencyHelper.GetService<ILeaveService>();
            //var memberService = DependencyHelper.GetService<IMembershipService>();
            //var notify = false;
            //if (Leave.LeaveStatus == LeaveStatus.Rejected)
            //{
            //    Leave.LeaveStatus = LeaveStatus.Request;
            //    Leave.CreatedUserID = CurrentUser.Identity.ID;
            //    notify = true;
            //}
            if (DocumentFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Leave, DocumentFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Leave.Document = uploadPicture;
                }
            }
            if (leaveService.UpdateLeave(Leave))
            {
                //if (notify)
                //{
                //    var supervisors = memberService.GetAllSupervisors().Where(m => m.ID != Leave.UserID);
                //    LeaveNotificator.NotifyUser(NotifyAction.Request, supervisors.ToList(), Leave.ID, Leave.LeaveType.DisplayName + " Request ");
                //}
                return true;
            }
            return false;
        }
        public bool Delete()
        {
            var leaveService = DependencyHelper.GetService<ILeaveService>();
            return leaveService.DeleteLeave(LeaveID);
        }
        //public bool Approve()
        //{
        //    var leaveService = DependencyHelper.GetService<ILeaveService>();
        //    var leaveExist = leaveService.GetLeave(LeaveID);
        //    if (leaveExist == null)
        //    {
        //        throw new ObjectAlreadyExistsException("Leave not found");
        //    }
        //    if (leaveExist.LeaveStatus != LeaveStatus.Request)
        //    {
        //        throw new ObjectAlreadyExistsException("This leave has been " + leaveExist.LeaveStatus.DisplayName);
        //    }
        //    leaveExist.AprroveUserID = CurrentUser.Identity.ID;
        //    leaveExist.LeaveStatus = LeaveStatus.Approved;
        //    if (leaveService.UpdateLeave(leaveExist))
        //    {
        //        LeaveNotificator.NotifyUser(NotifyAction.Approved, new List<User> { leaveExist.User }, leaveExist.ID, "Leave has been approved");
        //        return true;
        //    }
        //    return false;
        //}
        //public bool Reject()
        //{
        //    var leaveService = DependencyHelper.GetService<ILeaveService>();
        //    var leaveExist = leaveService.GetLeave(LeaveID);
        //    if (leaveExist == null)
        //    {
        //        throw new ObjectAlreadyExistsException("Leave not found");
        //    }
        //    if (leaveExist.LeaveStatus != LeaveStatus.Request)
        //    {
        //        throw new ObjectAlreadyExistsException("This leave has been " + leaveExist.LeaveStatus.DisplayName);
        //    }
        //    leaveExist.AprroveUserID = CurrentUser.Identity.ID;
        //    leaveExist.LeaveStatus = LeaveStatus.Rejected;
        //    leaveExist.ReasonReject = Message;
        //    if (leaveService.UpdateLeave(leaveExist))
        //    {
        //        LeaveNotificator.NotifyUser(NotifyAction.Rejected, new List<User> { leaveExist.User }, leaveExist.ID, "Leave has been rejected");
        //        return true;
        //    }
        //    return false;
        //}
    }
}