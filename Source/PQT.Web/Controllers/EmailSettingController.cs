using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;
using PQT.Web.Models;
using PQT.Web.Infrastructure.Filters;
using PQT.Web.Infrastructure.Helpers;
using NS.Entity;

namespace PQT.Web.Controllers
{
    public class EmailSettingController : Controller
    {
        //
        // GET: /EmailTemplateSetting/
        private readonly IUnitRepository _repoUnit;
        private readonly IMembershipService _membershipService;
        private readonly IRoleService _roleService;


        public EmailSettingController(IUnitRepository repoUnit, IMembershipService membershipService, IRoleService roleService)
        {
            _repoUnit = repoUnit;
            _membershipService = membershipService;
            _roleService = roleService;
        }
        [DisplayName(@"Recipient email addresses management")]
        public ActionResult Index()
        {
            var dInfo = new DirectoryInfo(HttpContext.Server.MapPath("~/Content/EmailTemplates"));
            DirectoryInfo[] subdirs = dInfo.GetDirectories();
            var model = subdirs.Where(m => m.Name != "Logs").Select(directoryInfo => new EmailSettingModel { Type = directoryInfo.Name, EmailReceiveItems = new List<EmailSettingItem>() }).ToList();
            return View(model);
        }

        [AjaxOnly]
        public string Submit(EmailSettingItem model)
        {
            //_repoUnit.DeleteEmailAllInTemplate(model.EmailReceive.Type, model.EmailReceive.TemplateName);
            var emailReceive = model.EmailSetting;
            emailReceive.EmailTo = string.Join(",", model.Tos);
            emailReceive.EmailCc = string.Join(",", model.Ccs);
            emailReceive.EmailBcc = string.Join(",", model.Bccs);
            _repoUnit.CreateEmailSetting(emailReceive);
            return "";
        }

        [AjaxOnly]
        public ActionResult EmailTemplate(string type)
        {
            var directoryInfo = new DirectoryInfo(HttpContext.Server.MapPath("~/Content/EmailTemplates/" + type));
            var emailReceiveModel = new EmailSettingModel { Type = directoryInfo.Name, EmailReceiveItems = new List<EmailSettingItem>() };
            FileInfo[] files = directoryInfo.GetFiles("*.html.cshtml"); //Getting Text files
            foreach (FileInfo file in files)
            {
                var templateName = file.Name.Replace(".html.cshtml", "");
                var strBody = "";

                strBody = EmailHelper.RenderMessage(emailReceiveModel.Type + "/" + templateName); //GenerateBodyMessage(emailReceiveModel.Type, templateName);
                EmailSettingItem emailItem;
                var emailTemplate = _repoUnit.GetEmailSetting(emailReceiveModel.Type, templateName);
                if (emailTemplate != null)
                {
                    emailItem = new EmailSettingItem
                    {
                        Tos = string.IsNullOrEmpty(emailTemplate.EmailTo) ? new string[] { } : emailTemplate.EmailTo.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries),
                        Ccs = string.IsNullOrEmpty(emailTemplate.EmailCc) ? new string[] { } : emailTemplate.EmailCc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries),
                        Bccs = string.IsNullOrEmpty(emailTemplate.EmailBcc) ? new string[] { } : emailTemplate.EmailBcc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries),
                        EMailBody = strBody,
                        EmailSetting = { ID = emailTemplate.ID, Type = emailTemplate.Type, TemplateName = emailTemplate.TemplateName },
                    };
                }
                else
                {

                    emailItem = new EmailSettingItem
                    {
                        EMailBody = strBody,
                        EmailSetting = { Type = emailReceiveModel.Type, TemplateName = templateName },
                    };
                }
                var result = new List<SelectListItem>();
                result.AddRange(
                    _roleService.GetAllRoles().Select(m => new SelectListItem
                    {
                        Value = StringHelper.RemoveSpecialCharacters(m.Name),
                        Text = @"Role: " + m.Name,
                    }));
                result.AddRange(
                    _membershipService.GetUsers(
                        m => m.Status == EntityStatus.Normal)
                        .Select(m => new SelectListItem
                        {
                            Value = m.ID.ToString(),
                            Text = m.DisplayName + @" <" + m.Email + @">",
                        }).ToList());

                emailItem.Emails = result;
                emailReceiveModel.EmailReceiveItems.Add(emailItem);
            }
            return PartialView(emailReceiveModel);
        }

    }
}
