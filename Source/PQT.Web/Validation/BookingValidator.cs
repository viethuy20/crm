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
    public class BookingValidator : AbstractValidator<Booking>
    {
        private readonly IBookingService _repo;
        public BookingValidator(IBookingService repo)
        {
            _repo = repo;
            RuleFor(u => u.Delegates).Must(CheckDelegates).WithMessage("Delegate Item is required");
            RuleFor(m => m.AuthoriserName).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(u => u.Tel).Matches(new Regex(@"^[0-9\-\+\ \(\)]*$")).WithMessage(Resource.PhoneNumberIsInvalid);
            RuleFor(u => u.Fax).Matches(new Regex(@"^[0-9\-\+\ \(\)]*$")).WithMessage(Resource.FaxNumberIsInvalid);
            RuleFor(u => u.SenderTel).Matches(new Regex(@"^[0-9\-\+\ \(\)]*$")).WithMessage(Resource.PhoneNumberIsInvalid);
            RuleFor(u => u.SenderMail).EmailAddress().WithMessage(Resource.EmailIsInvalid);
            RuleFor(u => u.AuthoriserTel).Matches(new Regex(@"^[0-9\-\+\ \(\)]*$")).WithMessage(Resource.PhoneNumberIsInvalid);
            RuleFor(u => u.AuthoriserMail).EmailAddress().WithMessage(Resource.EmailIsInvalid);
        }

        private bool CheckDelegates(Booking booking, ICollection<Domain.Entities.Delegate> delegates)
        {
            return delegates != null && delegates.Any();
        }
    }
    public class DelegateValidator : AbstractValidator<Domain.Entities.Delegate>
    {
        public DelegateValidator()
        {
            RuleFor(m => m.Name).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(u => u.Tel).Matches(new Regex(@"^[0-9\+\-\()\ ]*$")).WithMessage(Resource.PhoneNumberIsInvalid);
            RuleFor(u => u.Mail).EmailAddress().WithMessage(Resource.EmailIsInvalid);

        }
    }
}