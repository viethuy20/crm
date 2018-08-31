using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NS;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Models
{
    public class RecruitmentModel
    {
        public int id { get; set; }
        public Candidate Candidate { get; set; }
        public HttpPostedFileBase ResumeFile { get; set; }
        public IEnumerable<User> Interviewers { get; set; }

        public RecruitmentModel()
        {
            Candidate= new Candidate();
            Interviewers = new HashSet<User>();
        }

        public string hubConnectionId { get; set; }
        public string Reason { get; set; }

        public void PrepareEdit(int leadId)
        {
            var recruitRepo = DependencyHelper.GetService<IRecruitmentService>();
            Candidate = recruitRepo.GetCandidate(leadId);
            id = leadId;
        }

        public bool SaveEdit()
        {
            var recruitmentService = DependencyHelper.GetService<IRecruitmentService>();
            if (ResumeFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Recruitment, ResumeFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Candidate.Resume = uploadPicture;
                }
            }
            return recruitmentService.UpdateCandidate(Candidate);
        }

        public string DeleteCandidate()
        {
            var recruitmentService = DependencyHelper.GetService<IRecruitmentService>();
            var candidate = recruitmentService.GetCandidate(id);
            if (candidate.CandidateStatusRecord == CandidateStatus.Deleted)
                return "Cannot process ... Call status: " + candidate.StatusDisplay;
            candidate.CandidateStatusRecord = new CandidateStatusRecord(candidate.ID, CandidateStatus.Deleted, CurrentUser.Identity.ID);
            return recruitmentService.UpdateCandidate(candidate) ? "" : "Delete failed";
        }

        public bool Create()
        {
            var recruitmentService = DependencyHelper.GetService<IRecruitmentService>();
            if (ResumeFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Recruitment, ResumeFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Candidate.Resume = uploadPicture;
                }
            }
            Candidate = recruitmentService.CreateCandidate(Candidate);
            if (Candidate != null)
            {
                Candidate.CandidateStatusRecord = new CandidateStatusRecord(Candidate.ID, CandidateStatus.Initial, CurrentUser.Identity.ID);
                recruitmentService.UpdateCandidate(Candidate);
                return true;
            }
            return false;
        }


        public string RequestAction()
        {
            var recruitmentService = DependencyHelper.GetService<IRecruitmentService>();
            var candidate = recruitmentService.GetCandidate(id);
            if (candidate == null) return "Submit failed";
            if (candidate.CandidateStatusRecord == CandidateStatus.ApprovedEmployment)
                return "Cannot process ... This candidate has been approved";
            candidate.CandidateStatusRecord = new CandidateStatusRecord(candidate.ID, CandidateStatus.RequestEmployment, CurrentUser.Identity.ID, "");
            if (!recruitmentService.UpdateCandidate(candidate)) return "Submit failed";
            //LeadNotificator.NotifyUser(NotifyAction.Request, candidate.ID);
            //LeadNotificator.NotifyUpdateNCL(candidate.ID);
            return "";
        }

        public object ApprovalRequest()
        {
            var recruitmentService = DependencyHelper.GetService<IRecruitmentService>();
            var candidate = recruitmentService.GetCandidate(id);
            if (candidate == null)
                return new
                {
                    Message = "Approval failed",
                    IsSuccess = false
                };
            if (candidate.CandidateStatusRecord != CandidateStatus.RequestEmployment)
                return new
                {
                    Message = "Cannot process ... Candidate status is " + candidate.StatusDisplay,
                    IsSuccess = false
                };
            var currentUserId = CurrentUser.Identity.ID;
            candidate.CandidateStatusRecord = new CandidateStatusRecord(candidate.ID, CandidateStatus.ApprovedEmployment, currentUserId);
            if (!recruitmentService.UpdateCandidate(candidate))
                return new
                {
                    Message = "Approval failed",
                    IsSuccess = false
                };
            //var titleNotify = "Candidate has been approved";
            //var membershipService = DependencyHelper.GetService<IMembershipService>();
            //LeadNotificator.NotifyUser(new List<User> { candidate.User.TransferUserID > 0 ? membershipService.GetUser((int)candidate.User.TransferUserID) : candidate.User }, candidate.ID, titleNotify); // notify for manager
            //LeadNotificator.NotifyUpdateNCL(candidate.ID, hubConnectionId);
            return new
            {
                Message = "",
                IsSuccess = true
            };
        }
        public object RejectRequest()
        {
            var recruitmentService = DependencyHelper.GetService<IRecruitmentService>();
            var candidate = recruitmentService.GetCandidate(id);
            if (candidate == null)
                return new
                {
                    Message = "Reject failed",
                    IsSuccess = false
                };
            if (candidate.CandidateStatusRecord != CandidateStatus.RequestEmployment)
                return new
                {
                    Message = "Cannot process ... Candidate status is " + candidate.StatusDisplay,
                    IsSuccess = false
                };

            var currentUserId = CurrentUser.Identity.ID;
            candidate.CandidateStatusRecord = new CandidateStatusRecord(candidate.ID, CandidateStatus.RejectedEmployment, currentUserId, Reason);
            if (!recruitmentService.UpdateCandidate(candidate))
                return new
                {
                    Message = "Reject failed",
                    IsSuccess = false
                };
            //var titleNotify = "Candidate has been rejected";
            //var membershipService = DependencyHelper.GetService<IMembershipService>();
            //LeadNotificator.NotifyUser(new List<User> { candidate.User.TransferUserID > 0 ? membershipService.GetUser((int)candidate.User.TransferUserID) : candidate.User }, candidate.ID, titleNotify); // notify for manager
            //LeadNotificator.NotifyUpdateNCL(candidate.ID, hubConnectionId);

            return new
            {
                Message = "",
                IsSuccess = true
            };
        }
    }
}