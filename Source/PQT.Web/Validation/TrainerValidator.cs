using System.Text.RegularExpressions;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using FluentValidation;
using Resources;

namespace PQT.Web.Validation
{
    public class TrainerValidator : AbstractValidator<Trainer>
    {
        public TrainerValidator()
        {
            RuleFor(m => m.Name).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(u => u.BusinessPhone).Matches(new Regex(@"^[0-9\-\+\ \(\)]*$")).WithMessage(Resource.PhoneNumberIsInvalid);
            RuleFor(u => u.Email).EmailAddress().WithMessage(Resource.EmailIsInvalid);
            RuleFor(u => u.MobilePhone).Matches(new Regex(@"^[0-9\-\+\ \(\)]*$")).WithMessage(Resource.PhoneNumberIsInvalid);
        }
    }
}
