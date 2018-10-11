﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using FluentValidation;
using PQT.Domain.Entities;
using Resources;

namespace PQT.Web.Validation
{


    public class CandidateValidator : AbstractValidator<Candidate>
    {
        public CandidateValidator()
        {
            RuleFor(m => m.FirstName).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.LastName).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.MobileNumber).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.MobileNumber).Matches(new Regex(@"^[0-9]*$")).WithMessage(Resource.PhoneNumberIsInvalid);
            RuleFor(m => m.PersonalEmail).NotEmpty().WithMessage(Resource.TheFieldShouldNotBeEmpty);
            RuleFor(m => m.PersonalEmail).EmailAddress().WithMessage(Resource.EmailIsInvalid);
        }
    }
}