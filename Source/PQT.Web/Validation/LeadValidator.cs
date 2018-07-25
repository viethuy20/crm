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
            RuleFor(m => m.JobTitle).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.DirectLine).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.WorkEmail).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.GoodTrainingMonth).GreaterThan(0).WithMessage(Resource.TheFieldShouldNotBeEmpty);
        }
    }
    public class PhoneCallValidator : AbstractValidator<PhoneCall>
    {
        public PhoneCallValidator()
        {
            //RuleFor(m => m.Remark).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
        }
    }
}
