using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using FluentValidation;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using Resources;

namespace PQT.Web.Validation
{


    public class CandidateValidator : AbstractValidator<Candidate>
    {
        private readonly IRecruitmentService _recruitmentService;
        public CandidateValidator(IRecruitmentService recruitmentService)
        {
            _recruitmentService = recruitmentService;
            RuleFor(u => u.CandidateNo).Must(BeAUniqueNo).WithMessage("Candidate ID existing");
            RuleFor(m => m.FirstName).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.LastName).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.MobileNumber).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.MobileNumber).Matches(new Regex(@"^[0-9]*$")).WithMessage(Resource.PhoneNumberIsInvalid);
            RuleFor(m => m.PersonalEmail).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.PersonalEmail).EmailAddress().WithMessage(Resource.EmailIsInvalid);
        }
        private bool BeAUniqueNo(Candidate user, string email)
        {
            var existUser = _recruitmentService.GetCandidateByNo(email);
            return existUser == null || existUser.ID == user.ID;
        }
    }


    public class PsSummaryValidator : AbstractValidator<PsSummary>
    {
        public PsSummaryValidator()
        {
            RuleFor(u => u.Remarks).Must(BeAUniqueNo).WithMessage("Interviewer is required to put feedback to this summary");
        }
        private bool BeAUniqueNo(PsSummary user, string remarks)
        {
            if (user.Status != RecruitmentStatus.Initial)
                return !string.IsNullOrEmpty(remarks);
            return true;
        }
    }
    public class OneFaceToFaceSummaryValidator : AbstractValidator<OneFaceToFaceSummary>
    {
        public OneFaceToFaceSummaryValidator()
        {
            RuleFor(u => u.Remarks).Must(BeAUniqueNo).WithMessage("Interviewer is required to put feedback to this summary");
        }
        private bool BeAUniqueNo(OneFaceToFaceSummary user, string remarks)
        {
            if (user.Status != RecruitmentStatus.Initial)
                return !string.IsNullOrEmpty(remarks);
            return true;
        }
    }
    public class TwoFaceToFaceSummaryValidator : AbstractValidator<TwoFaceToFaceSummary>
    {
        public TwoFaceToFaceSummaryValidator()
        {
            RuleFor(u => u.Remarks).Must(BeAUniqueNo).WithMessage("Interviewer is required to put feedback to this summary");
        }
        private bool BeAUniqueNo(TwoFaceToFaceSummary user, string remarks)
        {
            if (user.Status != RecruitmentStatus.Initial)
                return !string.IsNullOrEmpty(remarks);
            return true;
        }
    }
}