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
    public class UploadTemplateValidator : AbstractValidator<UploadTemplate>
    {
        private readonly IUploadTemplateService _repo;
        public UploadTemplateValidator(IUploadTemplateService repo)
        {
            _repo = repo;
            RuleFor(m => m.GroupName).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.GroupName).Must(BeCode).WithMessage(Resource.CodeExists);
        }

        private bool BeCode(UploadTemplate occ, string code)
        {
            var oc = _repo.GetUploadTemplate(code);
            return oc == null || oc.ID == occ.ID;
        }
    }
}