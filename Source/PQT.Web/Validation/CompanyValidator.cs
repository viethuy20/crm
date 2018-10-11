using System.Text.RegularExpressions;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using FluentValidation;
using Resources;

namespace PQT.Web.Validation
{
    public class CompanyValidator : AbstractValidator<Company>
    {
        private readonly ICompanyRepository _repo;
        public CompanyValidator(ICompanyRepository repo)
        {
            _repo = repo;
            RuleFor(m => m.CountryID).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.CountryID).GreaterThan(0).WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.CompanyName).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            //RuleFor(u => u.Email).EmailAddress().WithMessage(Resource.EmailIsInvalid);
            RuleFor(u => u.Tel).Matches(new Regex(@"^[0-9]*$")).WithMessage(Resource.PhoneNumberIsInvalid);
            RuleFor(u => u.Fax).Matches(new Regex(@"^[0-9]*$")).WithMessage(Resource.FaxNumberIsInvalid);
        }
    }
}
