using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IMarketingService
    {
        int GetCountEmailResources(string searchValue);
        IEnumerable<EmailResource> GetAllEmailResources(string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        EmailResource GetEmailResource(int id);
        EmailResource CreateEmailResource(EmailResource info);
        bool DeleteEmailResource(int id);

        int GetCountSublistEmails(string searchValue);
        IEnumerable<SublistEmail> GetAllSublistEmails(string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        SublistEmail GetSublistEmail(int id);
        SublistEmail CreateSublistEmail(SublistEmail info);
        bool UpdateSublistEmail(SublistEmail info);
        bool DeleteSublistEmail(int id);


        int GetCountEmailCompanys(string searchValue);
        IEnumerable<EmailCompany> GetAllEmailCompanys(string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        EmailCompany GetEmailCompany(int id);
        EmailCompany CreateEmailCompany(EmailCompany info);
        bool UpdateEmailCompany(EmailCompany info);
        bool DeleteEmailCompany(int id);

        int GetCountEmailTemplates(string searchValue);
        IEnumerable<EmailTemplate> GetAllEmailTemplates(string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        EmailTemplate GetEmailTemplate(int id);
        EmailTemplate CreateEmailTemplate(EmailTemplate info);
        bool UpdateEmailTemplate(EmailTemplate info);
        bool DeleteEmailTemplate(int id);


        int GetCountEmailSignatures(string searchValue);
        IEnumerable<EmailSignature> GetAllEmailSignatures(string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        EmailSignature GetEmailSignature(int id);
        EmailSignature CreateEmailSignature(EmailSignature info);
        bool UpdateEmailSignature(EmailSignature info);
        bool DeleteEmailSignature(int id);

    }
}
