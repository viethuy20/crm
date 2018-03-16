using System.Text.RegularExpressions;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using FluentValidation;
using Resources;

namespace PQT.Web.Validation
{
    public class LeadValidator : AbstractValidator<Lead>
    {
        public LeadValidator()
        {
            RuleFor(m => m.CompanyID).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.CompanyID).GreaterThan(0).WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.GeneralLine).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.ClientName).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.DirectLine).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
        }
    }
}
