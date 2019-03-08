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

namespace PQT.Web.Models
{
    public class RecruitmentPositionModel
    {
        public RecruitmentPosition RecruitmentPosition { get; set; }
        public IEnumerable<OfficeLocation> OfficeLocations { get; set; }
        public IEnumerable<Role> Roles { get; set; }
        public int RecruitmentPositionID { get; set; }
        public string Reason { get; set; }
        public RecruitmentPositionModel()
        {
            OfficeLocations = new List<OfficeLocation>();
            Roles = new List<Role>();
            RecruitmentPosition = new RecruitmentPosition();
        }

        public void Prepare(int id)
        {
            var unitService = DependencyHelper.GetService<IUnitRepository>();
            var roleService = DependencyHelper.GetService<IRoleService>();
            OfficeLocations = unitService.GetAllOfficeLocations();
            Roles = roleService.GetAllRoles();
            if (id > 0)
            {
                RecruitmentPosition = unitService.GetRecruitmentPosition(id);
            }
            if (string.IsNullOrEmpty(RecruitmentPosition.PositionNo))
            {
                RecruitmentPosition.PositionNo = unitService.GetTempRecruitmentPositionNo();
            }
        }

        public void PrepareDetail(int id)
        {
            var unitService = DependencyHelper.GetService<IUnitRepository>();
            if (id > 0)
            {
                RecruitmentPosition = unitService.GetRecruitmentPosition(id);
            }
        }
        public object SaveData()
        {
            var unitService = DependencyHelper.GetService<IUnitRepository>();
            if (RecruitmentPosition.ID == 0)
            {
                if (CurrentUser.HasRole("Manager"))
                {
                    RecruitmentPosition.RecruitmentPositionStatus = RecruitmentPositionStatus.Approved;
                }
                RecruitmentPosition = unitService.CreateRecruitmentPosition(RecruitmentPosition);
                if (RecruitmentPosition != null)
                {
                    RecruitmentPosition.OfficeLocation =
                        unitService.GetOfficeLocation((int)RecruitmentPosition.OfficeLocationID);
                    if (RecruitmentPosition.RecruitmentPositionStatus == RecruitmentPositionStatus.Request)
                    {
                        RecruitmentPositionNotificator.NotifyUser(NotifyAction.Request, RecruitmentPosition.ID, "New Request For Recruitment Position");
                    }
                    return new
                    {
                        Code = 1,
                        Model = RecruitmentPosition
                    };
                }
                return new
                {
                    Code = 2
                };
            }
            if (unitService.UpdateRecruitmentPosition(RecruitmentPosition))
            {
                RecruitmentPosition.OfficeLocation =
                    unitService.GetOfficeLocation((int)RecruitmentPosition.OfficeLocationID);
                return (new
                {
                    Code = 3,
                    Model = RecruitmentPosition
                });
            }
            return (new
            {
                Code = 4
            });
        }

        public object Approve()
        {
            var unitService = DependencyHelper.GetService<IUnitRepository>();
            var recruitmentPosition = unitService.GetRecruitmentPosition(RecruitmentPositionID);
            if (recruitmentPosition == null)
                return (new { IsSuccess = false, Message = "Data not found" });
            if (recruitmentPosition.RecruitmentPositionStatus != RecruitmentPositionStatus.Request)
                return (new { IsSuccess = false, Message = "Data has " + recruitmentPosition.RecruitmentPositionStatusDisplay });
            recruitmentPosition.RecruitmentPositionStatus = RecruitmentPositionStatus.Approved;
            recruitmentPosition.StatusDateTime = DateTime.Now;
            recruitmentPosition.StatusMessage = null;
            if (!unitService.UpdateRecruitmentPosition(recruitmentPosition))
            {
                return (new { IsSuccess = false, Message = "Approve failed" });
            }
            RecruitmentPositionNotificator.NotifyUser(NotifyAction.Approved, recruitmentPosition.ID, "New Recruitment Position has been approved");
            return (new { IsSuccess = true });
        }
        public object Reject()
        {
            var unitService = DependencyHelper.GetService<IUnitRepository>();

            var hotel = unitService.GetRecruitmentPosition(RecruitmentPositionID);
            if (hotel == null)
                return (new { IsSuccess = false, Message = "Data not found" });
            if (hotel.RecruitmentPositionStatus != RecruitmentPositionStatus.Request)
                return (new { IsSuccess = false, Message = "Data has " + hotel.RecruitmentPositionStatusDisplay });

            hotel.StatusMessage = Reason;
            hotel.StatusDateTime = DateTime.Now;
            hotel.RecruitmentPositionStatus = RecruitmentPositionStatus.Rejected;
            if (!unitService.UpdateRecruitmentPosition(hotel))
            {
                return (new { IsSuccess = false, Message = "Reject failed" });
            }
            RecruitmentPositionNotificator.NotifyUser(NotifyAction.Rejected, hotel.ID, "Recruitment Position has been rejected");
            return (new { IsSuccess = true });
        }
    }
}