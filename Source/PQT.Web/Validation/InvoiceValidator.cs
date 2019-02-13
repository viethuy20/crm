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
    public class InvoiceValidator : AbstractValidator<Invoice>
    {
        private readonly IInvoiceService _repo;
        public InvoiceValidator(IInvoiceService repo)
        {
            _repo = repo;
            RuleFor(m => m.InvoiceNo).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.InvoiceNo).Must(BeCode).WithMessage("Invoice number exists");
        }

        private bool BeCode(Invoice occ, string code)
        {
            var oc = _repo.GetInvoiceByInvoiceNumber(code);
            return oc == null || oc.ID == occ.ID;
        }
    }
}