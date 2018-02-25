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
        private readonly IRoleService _roleService;

        public UserValidator(IMembershipService membershipService, IRoleService roleService)
        {
            _membershipService = membershipService;
            _roleService = roleService;

            RuleFor(u => u.DisplayName).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(u => u.Email).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(u => u.Email).EmailAddress().WithMessage(Resource.TheEmailAddressEnteredIsInvalid);
            RuleFor(u => u.Email).Must(BeAUniqueEmail).WithMessage(Resource.UserEmailExists);
            RuleFor(u => u.MobilePhone).Matches(new Regex(@"^[0-9\+\-\()\ ]*$")).WithMessage(Resource.OnlyNumberAllowed);
            RuleFor(u => u.BusinessPhone).Matches(new Regex(@"^[0-9\+\-\()\ ]*$")).WithMessage(Resource.OnlyNumberAllowed);
        }

        private bool BeAUniqueEmail(User user, string email)
        {
            User existUser = _membershipService.GetUserByEmail(email);
            return existUser == null || existUser.ID == user.ID;
        }

    }
}
