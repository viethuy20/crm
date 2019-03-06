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

    }
}