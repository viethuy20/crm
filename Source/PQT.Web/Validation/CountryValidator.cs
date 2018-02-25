using PQT.Domain.Abstract;
using FluentValidation;
using PQT.Domain.Entities;
using Resources;

namespace PQT.Web.Validation
{
    public class CountryValidator : AbstractValidator<Country>
    {
        private readonly IUnitRepository _unitRepository;

        public CountryValidator(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
            RuleFor(m => m.Code).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.Name).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.Code).Must(BeCode).WithMessage(Resource.CodeHasExistedAlready);
        }
        private bool BeCode(Country occ, string Code)
        {
            var oc = _unitRepository.GetCountry(Code);
            return oc == null || oc.ID == occ.ID;
        }
    }
}
