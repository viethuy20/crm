using FluentValidation;
using FluentValidation.Attributes;
using Resources;

namespace PQT.Web.Models
{
    [Validator(typeof(LoginViewModelValidator))]
    public class LoginViewModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    internal class LoginViewModelValidator : AbstractValidator<LoginViewModel>
    {
        public LoginViewModelValidator()
        {
            RuleFor(m => m.Email).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.Password).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
        }
    }
}
