using System.Text.RegularExpressions;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using FluentValidation;
using Resources;

namespace PQT.Web.Validation
{
    public class EventValidator : AbstractValidator<Event>
    {
        private readonly IEventService _repo;
        public EventValidator(IEventService repo)
        {
            _repo = repo;
            RuleFor(m => m.EventCode).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.EventName).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.StartDate).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.EndDate).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.EventCode).Must(BeCode).WithMessage(Resource.CodeExists);
        }
        private bool BeCode(Event occ, string code)
        {
            var oc = _repo.GetEvent(code);
            return oc == null || oc.ID == occ.ID;
        }
    }
}
