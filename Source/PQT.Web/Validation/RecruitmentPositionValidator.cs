using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using FluentValidation;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using Resources;

namespace PQT.Web.Validation
{
    public class RecruitmentPositionValidator : AbstractValidator<RecruitmentPosition>
    {
        private readonly IUnitRepository _repo;
        public RecruitmentPositionValidator(IUnitRepository repo)
        {
            _repo = repo;
            //RuleFor(m => m.PositionNo).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            //RuleFor(m => m.PositionNo).Must(BeCode).WithMessage("Position number exists");
        }

        private bool BeCode(RecruitmentPosition occ, string code)
        {
            var oc = _repo.GetRecruitmentPositionByNumber(code);
            return oc == null || oc.ID == occ.ID;
        }
    }
}