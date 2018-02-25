using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using AutoMapper;
using PQT.Domain;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Models;
using iTextSharp.text.pdf.qrcode;
using PQT.Domain.Helpers;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using MultiLanguage;
using PQT.Domain.Abstract;
using PQT.Domain.Concrete;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Utility;
using Newtonsoft.Json;
using Ninject.Activation;
using NS.Mail;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using ServiceStack;

namespace PQT.Web.Controllers
{

    public class HomeController : Controller
    {
        private readonly IUnitRepository _unitRepository;

        public HomeController(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public ActionResult Index()
        {
            var model = new BrowseModel();
            return View(model);
        }
    }
}
