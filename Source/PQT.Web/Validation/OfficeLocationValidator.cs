using PQT.Domain.Abstract;
using FluentValidation;
using PQT.Domain.Entities;
using Resources;

namespace PQT.Web.Validation
{
    public class OfficeLocationValidator : AbstractValidator<OfficeLocation>
    {
        private readonly IUnitRepository _unitRepository;

        public OfficeLocationValidator(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
            RuleFor(m => m.Name).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.Name).Must(BeCode).WithMessage(Resource.CodeExists);
        }
        private bool BeCode(OfficeLocation occ, string Code)
        {
            var oc = _unitRepository.GetOfficeLocation(Code);
            return oc == null || oc.ID == occ.ID;
        }
    }
}
