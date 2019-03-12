using System.Text.RegularExpressions;
using FluentValidation;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using Resources;

namespace PQT.Web.Validation
{
    public class UserValidator : AbstractValidator<User>
    {
        private readonly IMembershipService _membershipService;

        public UserValidator(IMembershipService membershipService)
        {
            _membershipService = membershipService;

            //RuleFor(u => u.DisplayName).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            //RuleFor(u => u.Email).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(u => u.Email).EmailAddress().WithMessage(Resource.EmailIsInvalid);
            RuleFor(u => u.Email).Must(BeAUniqueEmail).WithMessage(Resource.EmailExists);
            //RuleFor(u => u.UserNo).Must(BeAUniqueUserNo).WithMessage("Employee ID existing");
            RuleFor(u => u.MobilePhone).Matches(new Regex(@"^[0-9]*$")).WithMessage(Resource.PhoneNumberIsInvalid);
            RuleFor(u => u.BusinessPhone).Matches(new Regex(@"^[0-9]*$")).WithMessage(Resource.PhoneNumberIsInvalid);
        }

        private bool BeAUniqueEmail(User user, string email)
        {
            if (string.IsNullOrEmpty(email)) return true;
            User existUser = _membershipService.GetUserByEmail(email);
            return existUser == null || existUser.ID == user.ID;
        }
        private bool BeAUniqueUserNo(User user, string email)
        {
            User existUser = _membershipService.GetUserByNo(email);
            return existUser == null || existUser.ID == user.ID;
        }

    }
}
