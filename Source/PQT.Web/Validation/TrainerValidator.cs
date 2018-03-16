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
        }
    }
}
