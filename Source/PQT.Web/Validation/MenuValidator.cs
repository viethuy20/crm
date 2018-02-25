using FluentValidation;
using PQT.Domain.Entities;
using Resources;

namespace PQT.Web.Validation
{
    public class MenuValidator : AbstractValidator<Menu>
    {
        public MenuValidator()
        {
            RuleFor(m => m.Title).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
        }
    }
}
