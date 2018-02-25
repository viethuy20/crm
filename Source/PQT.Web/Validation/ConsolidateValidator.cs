using CPO.Domain.Abstract;
using CPO.Domain.Entities;
using FluentValidation;

namespace CPO.Web.Validation
{
    public class ConsolidateValidator : AbstractValidator<Consolidate>
    {
        //private readonly IIndentRepository _indentRepo;

        public ConsolidateValidator()
        {
            //_indentRepo = indentRepo;
            //RuleFor(m => m.ConsolidateName).NotEmpty().WithMessage("Please enter Consolidate name");
            //.Must(BeAUniqueCode).WithMessage("Consolidate with same code already exists");
        }

        //private bool BeAUniqueCode(Consolidate indent, string code)
        //{
        //    var existIndent = _indentRepo.GetConsolidate(code);
        //    return existIndent == null || existIndent.ID == indent.ID;
        //}
    }
}
