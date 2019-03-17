using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NS;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Infrastructure.Utility;
using Resources;

namespace PQT.Web.Models
{
    public class RecruitmentModel
    {
        public int id { get; set; }
        public int RoleID { get; set; }
        public string BackAction { get; set; }
        public Candidate Candidate { get; set; }
        public User Employee { get; set; }
        public HttpPostedFileBase ResumeFile { get; set; }
        public HttpPostedFileBase InformationFile { get; set; }
        public IEnumerable<User> Interviewers { get; set; }
        public HttpPostedFileBase SignedContractFile { get; set; }
        public HttpPostedFileBase BirthCertificationFile { get; set; }
        public HttpPostedFileBase FamilyCertificationFile { get; set; }
        public HttpPostedFileBase FilledDeclarationFormFile { get; set; }
        public HttpPostedFileBase CertOfHighestEducationFile { get; set; }
        public HttpPostedFileBase IDCardFile { get; set; }
        public HttpPostedFileBase TerminationLetterFile { get; set; }
        public HttpPostedFileBase OfferLetterFile { get; set; }
        public RecruitmentModel()
        {
            Candidate = new Candidate();
            Interviewers = new HashSet<User>();
        }

        public string hubConnectionId { get; set; }
        public string Reason { get; set; }

        public void PrepareEdit(int candidateId)
        {
            var recruitRepo = DependencyHelper.GetService<IRecruitmentService>();
            Candidate = recruitRepo.GetCandidate(candidateId);
            id = candidateId;
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
            if (InformationFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Recruitment, InformationFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Candidate.Information = uploadPicture;
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
            if (InformationFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Recruitment, InformationFile);
                if (!string.IsNullOrEmpty(uploadPicture))
                {
                    Candidate.Information = uploadPicture;
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
            var currentUserId = CurrentUser.Identity.ID;
            return TransactionWrapper.Do(() =>
            {
                var recruitmentService = DependencyHelper.GetService<IRecruitmentService>();
                var membershipService = DependencyHelper.GetService<IMembershipService>();
                var roleService = DependencyHelper.GetService<IRoleService>();
                var candidate = recruitmentService.GetCandidate(id);
                if (candidate == null) return "Submit failed";
                if (candidate.CandidateStatusRecord == CandidateStatus.ApprovedEmployment)
                    return "Cannot process ... This candidate has been approved";
                if (!string.IsNullOrEmpty(Employee.Email))
                {
                    var exist = membershipService.GetUserByEmail(Employee.Email);
                    if (exist != null && Employee.ID != exist.ID)
                        return Resource.EmailExists;
                }

                if (SignedContractFile != null)
                {
                    string uploadPicture = UserPicture.UploadContract(SignedContractFile);
                    if (!string.IsNullOrEmpty(uploadPicture))
                    {
                        Employee.UserContracts = new List<UserContract>{new UserContract
                        {
                            Order = 1,
                            EmploymentDate = Employee.EmploymentDate,
                            EmploymentEndDate = Employee.EmploymentEndDate,
                            SignedContract = uploadPicture
                        }};
                    }
                }

                if (BirthCertificationFile != null)
                {
                    string uploadPicture = UserPicture.UploadContract(BirthCertificationFile);
                    if (!string.IsNullOrEmpty(uploadPicture))
                    {
                        Employee.BirthCertification = uploadPicture;
                    }
                }
                if (FamilyCertificationFile != null)
                {
                    string uploadPicture = UserPicture.UploadContract(FamilyCertificationFile);
                    if (!string.IsNullOrEmpty(uploadPicture))
                    {
                        Employee.FamilyCertification = uploadPicture;
                    }
                }
                if (FilledDeclarationFormFile != null)
                {
                    string uploadPicture = UserPicture.UploadContract(FilledDeclarationFormFile);
                    if (!string.IsNullOrEmpty(uploadPicture))
                    {
                        Employee.FilledDeclarationForm = uploadPicture;
                    }
                }
                if (CertOfHighestEducationFile != null)
                {
                    string uploadPicture = UserPicture.UploadContract(CertOfHighestEducationFile);
                    if (!string.IsNullOrEmpty(uploadPicture))
                    {
                        Employee.CertOfHighestEducation = uploadPicture;
                    }
                }
                if (IDCardFile != null)
                {
                    string uploadPicture = UserPicture.UploadContract(IDCardFile);
                    if (!string.IsNullOrEmpty(uploadPicture))
                    {
                        Employee.IDCard = uploadPicture;
                    }
                }
                if (OfferLetterFile != null)
                {
                    string uploadPicture = UserPicture.UploadContract(OfferLetterFile);
                    if (!string.IsNullOrEmpty(uploadPicture))
                    {
                        Employee.OfferLetter = uploadPicture;
                    }
                }
                if (TerminationLetterFile != null)
                {
                    string uploadPicture = UserPicture.UploadContract(TerminationLetterFile);
                    if (!string.IsNullOrEmpty(uploadPicture))
                    {
                        Employee.TerminationLetter = uploadPicture;
                    }
                }
                Employee.Status = EntityUserStatus.RequestEmployment;
                if (Employee.ID == 0)
                {
                    Employee = membershipService.CreateUser(Employee);
                }
                else
                {
                    membershipService.UpdateUserIncludeCollection(Employee);
                }
                if (RoleID > 0)
                    roleService.AssignRoles(Employee, new List<int> { RoleID });

                candidate.EmployeeID = Employee.ID;
                candidate.CandidateStatusRecord = new CandidateStatusRecord(candidate.ID, CandidateStatus.RequestEmployment, currentUserId, "");
                if (!recruitmentService.UpdateCandidate(candidate)) return "Submit failed";
                RecruitmentNotificator.NotifyUser(NotifyAction.Request, candidate.ID, "HR Request Employment");
                return "";
            });
        }

        public object ApprovalRequest()
        {
            var currentUserId = CurrentUser.Identity.ID;
            return TransactionWrapper.Do(() =>
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
                candidate.CandidateStatusRecord = new CandidateStatusRecord(candidate.ID, CandidateStatus.ApprovedEmployment, currentUserId);
                var membershipService = DependencyHelper.GetService<IMembershipService>();
                var user = membershipService.GetUser((int)candidate.EmployeeID);
                user.Status = EntityUserStatus.ApprovedEmployment;
                membershipService.UpdateUser(user);
                if (!recruitmentService.UpdateCandidate(candidate))
                    return new
                    {
                        Message = "Approval failed",
                        IsSuccess = false
                    };
                //var titleNotify = "Candidate has been approved";
                //var membershipService = DependencyHelper.GetService<IMembershipService>();
                //LeadNotificator.NotifyUser(new List<User> { candidate.User.TransferUserID > 0 ? membershipService.GetUser((int)candidate.User.TransferUserID) : candidate.User }, candidate.ID, titleNotify); // notify for manager
                RecruitmentNotificator.NotifyUser(NotifyAction.Approved, new List<User> { candidate.User }, candidate.ID, "Request Employment has been approved");
                return new
                {
                    Message = "",
                    IsSuccess = true
                };
            });
        }
        public object RejectRequest()
        {
            var currentUserId = CurrentUser.Identity.ID;
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
            RecruitmentNotificator.NotifyUser(NotifyAction.Rejected, new List<User> { candidate.User }, candidate.ID, "Request Employment has been rejected");
            return new
            {
                Message = "",
                IsSuccess = true
            };
        }
    }
}