using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Models;
using PQT.Web.Infrastructure.Filters;

namespace PQT.Web.Controllers
{
    [ExcludeFilters(typeof(RequestAuthorizeAttribute))]
    public class ReportController : Controller
    {
        private readonly IUnitRepository _unitRepo;
        public ReportController(IUnitRepository unitRepo)
        {
            _unitRepo = unitRepo;
        }
    }
}
