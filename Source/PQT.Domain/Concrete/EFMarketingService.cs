using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using NS;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Concrete
{
    public class EFMarketingService : Repository, IMarketingService
    {
        public EFMarketingService(DbContext db)
            : base(db)
        {
        }


        public int GetCountEmailResources(string searchValue)
        {
            IQueryable<EmailResource> queries = _db.Set<EmailResource>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m =>
                    m.Name.ToLower().Contains(searchValue) ||
                    m.Email.ToLower().Contains(searchValue) ||
                    m.Status.ToLower().Contains(searchValue) ||
                    (m.SublistEmail != null && m.SublistEmail.Description.ToLower().Contains(searchValue)));
            }
            return queries.Count();
        }

        public IEnumerable<EmailResource> GetAllEmailResources(string searchValue, string sortColumnDir,
            string sortColumn, int page, int pageSize)
        {
            IQueryable<EmailResource> queries = _db.Set<EmailResource>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m =>
                    m.Name.ToLower().Contains(searchValue) ||
                    m.Email.ToLower().Contains(searchValue) ||
                    m.Status.ToLower().Contains(searchValue) ||
                    (m.SublistEmail != null && m.SublistEmail.Description.ToLower().Contains(searchValue)));
            }

            switch (sortColumn)
            {
                case "Email":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Email)
                        : queries.OrderByDescending(s => s.Email);
                    break;
                case "Name":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Name)
                        : queries.OrderByDescending(s => s.Name);
                    break;
                case "Status":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Status)
                        : queries.OrderByDescending(s => s.Status);
                    break;
                case "SublistEmail":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.SublistEmail.Description)
                        : queries.OrderByDescending(s => s.SublistEmail.Description);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize).ToList();
        }

        public EmailResource GetEmailResource(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<EmailResource>().FirstOrDefault(m => m.ID == id);
        }

        public EmailResource CreateEmailResource(EmailResource info)
        {
            info = Create(info);
            return info;
        }

        public bool DeleteEmailResource(int id)
        {
            return Delete<EmailResource>(id);
        }


        public int GetCountSublistEmails(string searchValue)
        {
            IQueryable<SublistEmail> queries = _db.Set<SublistEmail>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m =>
                    (m.Description != null && m.Description.ToLower().Contains(searchValue)) ||
                    (m.RoundRecord != null && m.RoundRecord.Status.Value.ToLower().Contains(searchValue)) ||
                    (m.AssignUser != null && m.AssignUser.DisplayName.ToLower().Contains(searchValue)) ||
                    m.Status.ToLower().Contains(searchValue));
            }
            return queries.Count();
        }

        public IEnumerable<SublistEmail> GetAllSublistEmails(string searchValue, string sortColumnDir,
            string sortColumn, int page, int pageSize)
        {
            IQueryable<SublistEmail> queries = _db.Set<SublistEmail>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m =>
                    (m.Description != null && m.Description.ToLower().Contains(searchValue)) ||
                    (m.RoundRecord != null && m.RoundRecord.Status.Value.ToLower().Contains(searchValue)) ||
                    (m.AssignUser != null && m.AssignUser.DisplayName.ToLower().Contains(searchValue)) ||
                    m.Status.ToLower().Contains(searchValue));
            }

            switch (sortColumn)
            {
                case "Description":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Description)
                        : queries.OrderByDescending(s => s.Description);
                    break;
                case "RoundRecordStatus":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.RoundRecord.Status.Value)
                        : queries.OrderByDescending(s => s.RoundRecord.Status.Value);
                    break;
                case "AssignDate":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.AssignDate)
                        : queries.OrderByDescending(s => s.AssignDate);
                    break;
                case "AssignUser":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.AssignUser.DisplayName)
                        : queries.OrderByDescending(s => s.AssignUser.DisplayName);
                    break;
                case "Status":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.AssignUser.Status)
                        : queries.OrderByDescending(s => s.AssignUser.Status);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize).ToList();
        }

        public SublistEmail GetSublistEmail(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<SublistEmail>().FirstOrDefault(m => m.ID == id);
        }

        public SublistEmail CreateSublistEmail(SublistEmail info)
        {
            info = Create(info);
            return info;
        }

        public bool UpdateSublistEmail(SublistEmail info)
        {
            return Update(info);
        }

        public bool DeleteSublistEmail(int id)
        {
            return Delete<SublistEmail>(id);
        }


        public int GetCountEmailCompanys(string searchValue)
        {
            IQueryable<EmailCompany> queries = _db.Set<EmailCompany>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m =>
                    (m.Email != null && m.Email.ToLower().Contains(searchValue)) ||
                    (m.NumberSentForToday.ToString().Contains(searchValue)) ||
                    (m.AssignUser != null && m.AssignUser.DisplayName.ToLower().Contains(searchValue)));
            }
            return queries.Count();
        }

        public IEnumerable<EmailCompany> GetAllEmailCompanys(string searchValue, string sortColumnDir,
            string sortColumn, int page, int pageSize)
        {
            IQueryable<EmailCompany> queries = _db.Set<EmailCompany>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m =>
                    (m.Email != null && m.Email.ToLower().Contains(searchValue)) ||
                    (m.NumberSentForToday.ToString().Contains(searchValue)) ||
                    (m.AssignUser != null && m.AssignUser.DisplayName.ToLower().Contains(searchValue)));
            }
            switch (sortColumn)
            {
                case "Email":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Email)
                        : queries.OrderByDescending(s => s.Email);
                    break;
                case "NumberSentForToday":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.NumberSentForToday)
                        : queries.OrderByDescending(s => s.NumberSentForToday);
                    break;
                case "Name":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Name)
                        : queries.OrderByDescending(s => s.Name);
                    break;
                case "AssignUser":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.AssignUser.DisplayName)
                        : queries.OrderByDescending(s => s.AssignUser.DisplayName);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize).ToList();
        }

        public EmailCompany GetEmailCompany(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<EmailCompany>().FirstOrDefault(m => m.ID == id);
        }

        public EmailCompany CreateEmailCompany(EmailCompany info)
        {
            info = Create(info);
            return info;
        }

        public bool UpdateEmailCompany(EmailCompany info)
        {
            return Update(info);

        }

        public bool DeleteEmailCompany(int id)
        {
            return Delete<EmailCompany>(id);
        }

        public int GetCountEmailTemplates(string searchValue)
        {
            IQueryable<EmailTemplate> queries = _db.Set<EmailTemplate>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m =>
                    (m.Subject != null && m.Subject.ToLower().Contains(searchValue)));
            }
            return queries.Count();

        }

        public IEnumerable<EmailTemplate> GetAllEmailTemplates(string searchValue, string sortColumnDir,
            string sortColumn, int page, int pageSize)
        {
            IQueryable<EmailTemplate> queries = _db.Set<EmailTemplate>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m =>
                    (m.Subject != null && m.Subject.ToLower().Contains(searchValue)));
            }
            switch (sortColumn)
            {
                case "Subject":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Subject)
                        : queries.OrderByDescending(s => s.Subject);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize).ToList();
        }

        public EmailTemplate GetEmailTemplate(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<EmailTemplate>().FirstOrDefault(m => m.ID == id);
        }

        public EmailTemplate CreateEmailTemplate(EmailTemplate info)
        {
            info = Create(info);
            return info;
        }

        public bool UpdateEmailTemplate(EmailTemplate info)
        {
            return Update(info);
        }

        public bool DeleteEmailTemplate(int id)
        {
            return Delete<EmailTemplate>(id);
        }


        public int GetCountEmailSignatures(string searchValue)
        {
            IQueryable<EmailSignature> queries = _db.Set<EmailSignature>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m =>
                    (m.Description != null && m.Description.ToLower().Contains(searchValue)));
            }
            return queries.Count();
        }

        public IEnumerable<EmailSignature> GetAllEmailSignatures(string searchValue, string sortColumnDir,
            string sortColumn, int page, int pageSize)
        {
            IQueryable<EmailSignature> queries = _db.Set<EmailSignature>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m =>
                    (m.Description != null && m.Description.ToLower().Contains(searchValue)));
            }
            switch (sortColumn)
            {
                case "Description":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Description)
                        : queries.OrderByDescending(s => s.Description);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize).ToList();
        }

        public EmailSignature GetEmailSignature(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<EmailSignature>().FirstOrDefault(m => m.ID == id);
        }

        public EmailSignature CreateEmailSignature(EmailSignature info)
        {
            info = Create(info);
            return info;
        }

        public bool UpdateEmailSignature(EmailSignature info)
        {
            return Update(info);
        }

        public bool DeleteEmailSignature(int id)
        {
            return Delete<EmailSignature>(id);
        }
    }
}
