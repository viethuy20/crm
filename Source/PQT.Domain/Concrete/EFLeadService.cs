using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NS;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using ServiceStack;

namespace PQT.Domain.Concrete
{
    public class EFLeadService : Repository, ILeadService
    {
        public EFLeadService(DbContext db)
            : base(db)
        {
        }

        public int CountLeadsByTier(int eventId, int tier)
        {
            return _db.Set<Lead>().Count(m => !m.ExpiredForReopen && m.EventID == eventId && m.Company.Tier == tier &&
                                              m.EntityStatus.Value == EntityStatus.Normal.Value);
        }
        public int CountLeadsTotalBooked(int eventId, int saleId)
        {
            IQueryable<Lead> queries = _db.Set<Lead>().Where(m => m.EventID == eventId && m.LeadStatusRecord.Status.Value == LeadStatus.Booked.Value);
            if (saleId > 0)
            {
                queries = queries.Where(m => m.UserID == saleId);
            }
            return queries.Count();
        }
        public int CountLeadsTotalLOINCL(int eventId, int saleId, int daysExpired)
        {
            var expiredDate = DateTime.Today.AddDays(-daysExpired);
            IQueryable<Lead> queries = _db.Set<Lead>().Where(m => m.EventID == eventId && !m.ExpiredForReopen &&
                                                                  m.EntityStatus.Value == EntityStatus.Normal.Value &&
                                                                  m.LeadStatusRecord.Status.Value == LeadStatus.LOI.Value &&
                                                                  m.LeadStatusRecord.UpdatedTime >= expiredDate);
            if (saleId > 0)
            {
                queries = queries.Where(m => m.UserID == saleId);
            }
            else
            {
                queries = queries.Where(m => m.User.UserStatus.Value == UserStatus.Live.Value ||
                                             m.User.DirectSupervisorID > 0);
            }
            return queries.Count();
        }
        public int CountLeadsTotalBlockedNCL(int eventId, int saleId)
        {
            IQueryable<Lead> queries = _db.Set<Lead>().Where(m => !m.ExpiredForReopen &&
                                                                  m.EntityStatus.Value == EntityStatus.Normal.Value &&
                                                                  m.LeadStatusRecord.Status.Value == LeadStatus.Blocked.Value);
            if (eventId > 0)
            {
                queries = queries.Where(m => m.EventID == eventId);
            }

            if (saleId > 0)
            {
                queries = queries.Where(m => m.UserID == saleId || (m.User != null && m.User.TransferUserID == saleId));
            }
            else
            {
                queries = queries.Where(m => m.User.UserStatus.Value == UserStatus.Live.Value ||
                                             m.User.DirectSupervisorID > 0);
            }
            return queries.Count();
        }
        public int CountLeadsTotalTotalKPI(int eventId, int saleId)
        {
            IQueryable<Lead> queries = _db.Set<Lead>().Where(m => m.EventID == eventId && !m.ExpiredForReopen &&
                                                                  m.EntityStatus.Value == EntityStatus.Normal.Value &&
                                                                  m.MarkKPI);
            if (saleId > 0)
            {
                queries = queries.Where(m => m.UserID == saleId);
            }
            else
            {
                queries = queries.Where(m => m.User.UserStatus.Value == UserStatus.Live.Value ||
                                             m.User.DirectSupervisorID > 0);
            }
            return queries.Count();
        }


        public IEnumerable<Lead> GetAllLeadsForCallResources(int comId, int userId, string name, string role,
            string email, string phone)
        {

            IQueryable<Lead> queries = _db.Set<Lead>().Where(m => m.UserID == userId && m.CompanyID == comId &&
                                                                  m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (!string.IsNullOrEmpty(name))
                queries = queries.Where(m => (m.Salutation + " " + m.FirstName + " " + m.LastName).ToLower().Contains(name));
            if (!string.IsNullOrEmpty(role))
                queries = queries.Where(m => !string.IsNullOrEmpty(m.JobTitle) && m.JobTitle.ToLower().Contains(role));
            if (!string.IsNullOrEmpty(email))
                queries = queries.Where(m => (!string.IsNullOrEmpty(m.WorkEmail) && m.WorkEmail.ToLower().Contains(email)) ||
                                             (!string.IsNullOrEmpty(m.PersonalEmail) && m.PersonalEmail.ToLower().Contains(email)));
            if (!string.IsNullOrEmpty(phone))
                queries = queries.Where(m => (!string.IsNullOrEmpty(m.MobilePhone1) && m.MobilePhone1.ToLower().Contains(phone)) ||
                                             (!string.IsNullOrEmpty(m.MobilePhone2) && m.MobilePhone2.ToLower().Contains(phone)) ||
                                             (!string.IsNullOrEmpty(m.MobilePhone3) && m.MobilePhone3.ToLower().Contains(phone)) ||
                                             (!string.IsNullOrEmpty(m.DirectLine) && m.DirectLine.ToLower().Contains(phone)));
            return queries.ToList();
        }

        public IEnumerable<Lead> GetAllLeadsByEvent(int eventId)
        {
            return _db.Set<Lead>().Where(m => m.EventID == eventId &&
                                              m.EntityStatus.Value == EntityStatus.Normal.Value).ToList();
        }

        public IEnumerable<Lead> GetAllLeadsByEventExcludeReopen(int eventId, int excludeLeaveId)
        {
            IQueryable<Lead> queries = _db.Set<Lead>().Where(m => m.EventID == eventId && !m.ExpiredForReopen &&
                                              m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value &&
                                              m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (excludeLeaveId > 0)
            {
                queries = queries.Where(m => m.ID != excludeLeaveId);
            }
            return queries.ToList();
        }

        public int GetCountLeadsForKpiReview(int eventId, string searchValue)
        {
            IQueryable<Lead> queries = _db.Set<Lead>()
                .Where(m => !m.MarkKPI &&
                            m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                            m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                            m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value &&
                            m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (eventId > 0)
            {
                queries = queries.Where(m => m.EventID == eventId);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m =>
                    m.Company.CompanyName.ToLower().Contains(searchValue) ||
                    m.DirectLine.ToLower().Contains(searchValue) ||
                    m.JobTitle.ToLower().Contains(searchValue) ||
                    m.FirstName.ToLower().Contains(searchValue) ||
                    m.LastName.ToLower().Contains(searchValue) ||
                    (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                    (m.MobilePhone2 != null && m.MobilePhone1.Contains(searchValue)) ||
                    (m.MobilePhone3 != null && m.MobilePhone1.Contains(searchValue)) ||
                    (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                    (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                    (m.KPIRemarks != null && m.KPIRemarks.ToLower().Contains(searchValue)));
            }
            return queries.Count();
        }

        public IEnumerable<Lead> GetAllLeadsForKpiReview(int eventId, string searchValue, string sortColumnDir, string sortColumn,
            int page, int pageSize)
        {

            IQueryable<Lead> queries = _db.Set<Lead>()
                .Where(m => !m.MarkKPI &&
                            m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                            m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                            m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value &&
                            m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (eventId > 0)
            {
                queries = queries.Where(m => m.EventID == eventId);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m =>
                    m.Company.CompanyName.ToLower().Contains(searchValue) ||
                    m.DirectLine.ToLower().Contains(searchValue) ||
                    m.JobTitle.ToLower().Contains(searchValue) ||
                    m.FirstName.ToLower().Contains(searchValue) ||
                    m.LastName.ToLower().Contains(searchValue) ||
                    (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                    (m.MobilePhone2 != null && m.MobilePhone1.Contains(searchValue)) ||
                    (m.MobilePhone3 != null && m.MobilePhone1.Contains(searchValue)) ||
                    (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                    (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                    (m.KPIRemarks != null && m.KPIRemarks.ToLower().Contains(searchValue)));
            }
            switch (sortColumn)
            {
                case "CompanyName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.CompanyName)
                        : queries.OrderByDescending(s => s.Company.CompanyName);
                    break;
                case "DirectLine":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.DirectLine)
                        : queries.OrderByDescending(s => s.DirectLine);
                    break;
                case "JobTitle":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.JobTitle)
                        : queries.OrderByDescending(s => s.JobTitle);
                    break;
                case "FirstName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FirstName)
                        : queries.OrderByDescending(s => s.FirstName);
                    break;
                case "LastName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LastName)
                        : queries.OrderByDescending(s => s.LastName);
                    break;
                case "MobilePhone1":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone1)
                        : queries.OrderByDescending(s => s.MobilePhone1);
                    break;
                case "PersonalEmail":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.PersonalEmail)
                        : queries.OrderByDescending(s => s.PersonalEmail);
                    break;
                case "WorkEmail":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.WorkEmail)
                        : queries.OrderByDescending(s => s.WorkEmail);
                    break;
                case "KPIRemarks":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.KPIRemarks)
                        : queries.OrderByDescending(s => s.KPIRemarks);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.CreatedTime)
                        : queries.OrderByDescending(s => s.CreatedTime);
                    break;
            }
            return queries.Skip(page).Take(pageSize).ToList();
        }

        public int GetCountEnquireLeadsForKpi(bool isSales, User currentUser, int eventId, int userId, string statusCode, DateTime datefrom,
            DateTime dateto, string searchValue)
        {
            IQueryable<Lead> queries = _db.Set<Lead>()
                .Where(m => m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                            m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                            m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value &&
                            m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (datefrom != default(DateTime))
            {
                queries = queries.Where(m => m.CreatedTime >= datefrom);
            }
            if (dateto != default(DateTime))
            {
                dateto = dateto.AddDays(1);
                queries = queries.Where(m => m.CreatedTime < dateto);
            }
            if (eventId > 0)
            {
                queries = queries.Where(m => m.EventID == eventId);
            }
            if (userId > 0)
            {
                queries = queries.Where(m => m.UserID == userId);
            }
            if (!string.IsNullOrEmpty(statusCode))
            {
                if (statusCode == KPIStatus.KPI)
                    queries = queries.Where(m => m.MarkKPI);
                else if (statusCode == KPIStatus.NoKPI)
                    queries = queries.Where(m => !m.MarkKPI && !string.IsNullOrEmpty(m.FileNameImportKPI));
                else if (statusCode == KPIStatus.NoCheck)
                    queries = queries.Where(m => string.IsNullOrEmpty(m.FileNameImportKPI));
            }
            if (isSales)
            {
                if (currentUser.SalesManagementUnit != SalesManagementUnit.None)
                    queries = queries.Where(m => m.UserID == currentUser.ID || m.User.DirectSupervisorID == currentUser.ID);
                else if (currentUser.BusinessDevelopmentUnit != BusinessDevelopmentUnit.None)
                    queries = queries.Where(m => m.UserID == currentUser.ID);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.LeadStatusRecord.UpdatedTime == dtSearch);
                else
                {
                    var searchStatus = Enumeration.GetAll<LeadStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        m.Event.EventName.ToLower().Contains(searchValue) ||
                        m.User.DisplayName.ToLower().Contains(searchValue) ||
                        searchStatus.Contains(m.LeadStatusRecord.Status.Value) ||
                        m.Company.CompanyName.ToLower().Contains(searchValue) ||
                        m.JobTitle.ToLower().Contains(searchValue) ||
                        m.DirectLine.ToLower().Contains(searchValue) ||
                        (m.Salutation != null && m.Salutation.Contains(searchValue)) ||
                        (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                        (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                        (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                        (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                        (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                        (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                        (m.WorkEmail1 != null && m.WorkEmail1.ToLower().Contains(searchValue)) ||
                        (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)));
                }
            }
            return queries.Count();
        }

        public IEnumerable<Lead> GetAllEnquireLeadsForKpi(bool isSales, User currentUser, int eventId, int userId, string statusCode,
            DateTime datefrom, DateTime dateto, string searchValue, string sortColumnDir, string sortColumn, int page,
            int pageSize)
        {

            IQueryable<Lead> queries = _db.Set<Lead>()
                .Where(m => m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                            m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                            m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value &&
                            m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (datefrom != default(DateTime))
            {
                queries = queries.Where(m => m.CreatedTime >= datefrom);
            }
            if (dateto != default(DateTime))
            {
                dateto = dateto.AddDays(1);
                queries = queries.Where(m => m.CreatedTime < dateto);
            }
            if (eventId > 0)
            {
                queries = queries.Where(m => m.EventID == eventId);
            }
            if (userId > 0)
            {
                queries = queries.Where(m => m.UserID == userId);
            }
            if (!string.IsNullOrEmpty(statusCode))
            {
                if (statusCode == KPIStatus.KPI)
                    queries = queries.Where(m => m.MarkKPI);
                else if (statusCode == KPIStatus.NoKPI)
                    queries = queries.Where(m => !m.MarkKPI && !string.IsNullOrEmpty(m.FileNameImportKPI));
                else if (statusCode == KPIStatus.NoCheck)
                    queries = queries.Where(m => string.IsNullOrEmpty(m.FileNameImportKPI));
            }
            if (isSales)
            {
                if (currentUser.SalesManagementUnit != SalesManagementUnit.None)
                    queries = queries.Where(m => m.UserID == currentUser.ID || m.User.DirectSupervisorID == currentUser.ID);
                else if (currentUser.BusinessDevelopmentUnit != BusinessDevelopmentUnit.None)
                    queries = queries.Where(m => m.UserID == currentUser.ID);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.LeadStatusRecord.UpdatedTime == dtSearch);
                else
                {
                    var searchStatus = Enumeration.GetAll<LeadStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        m.Event.EventName.ToLower().Contains(searchValue) ||
                        m.User.DisplayName.ToLower().Contains(searchValue) ||
                        searchStatus.Contains(m.LeadStatusRecord.Status.Value) ||
                        m.Company.CompanyName.ToLower().Contains(searchValue) ||
                        m.Company.Country.Code.ToLower().Contains(searchValue) ||
                        m.JobTitle.ToLower().Contains(searchValue) ||
                        m.DirectLine.ToLower().Contains(searchValue) ||
                        (m.Salutation != null && m.Salutation.Contains(searchValue)) ||
                        (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                        (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                        (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                        (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                        (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                        (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                        (m.WorkEmail1 != null && m.WorkEmail1.ToLower().Contains(searchValue)) ||
                        (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)));
                }
            }
            switch (sortColumn)
            {
                case "EventName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Event.EventName)
                        : queries.OrderByDescending(s => s.Event.EventName);
                    break;
                case "CreatedTime":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.CreatedTime)
                        : queries.OrderByDescending(s => s.CreatedTime);
                    break;
                case "CompanyName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.CompanyName)
                        : queries.OrderByDescending(s => s.Company.CompanyName);
                    break;
                case "Country":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.Country.Code)
                        : queries.OrderByDescending(s => s.Company.Country.Code);
                    break;
                case "JobTitle":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.JobTitle)
                        : queries.OrderByDescending(s => s.JobTitle);
                    break;
                case "DirectLine":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.DirectLine)
                        : queries.OrderByDescending(s => s.DirectLine);
                    break;
                case "Salutation":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Salutation)
                        : queries.OrderByDescending(s => s.Salutation);
                    break;
                case "FirstName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FirstName)
                        : queries.OrderByDescending(s => s.FirstName);
                    break;
                case "LastName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LastName)
                        : queries.OrderByDescending(s => s.LastName);
                    break;
                case "MobilePhone1":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone1)
                        : queries.OrderByDescending(s => s.MobilePhone1);
                    break;
                case "MobilePhone2":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone2)
                        : queries.OrderByDescending(s => s.MobilePhone2);
                    break;
                case "MobilePhone3":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone3)
                        : queries.OrderByDescending(s => s.MobilePhone3);
                    break;
                case "WorkEmail":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.WorkEmail)
                        : queries.OrderByDescending(s => s.WorkEmail);
                    break;
                case "PersonalEmail":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.PersonalEmail)
                        : queries.OrderByDescending(s => s.PersonalEmail);
                    break;
                case "EstimatedDelegateNumber":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.EstimatedDelegateNumber)
                        : queries.OrderByDescending(s => s.EstimatedDelegateNumber);
                    break;
                case "TrainingBudgetPerHead":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.TrainingBudgetPerHead)
                        : queries.OrderByDescending(s => s.TrainingBudgetPerHead);
                    break;
                case "GoodTrainingMonth":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.GoodTrainingMonth)
                        : queries.OrderByDescending(s => s.GoodTrainingMonth);
                    break;
                case "UserName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.User.DisplayName)
                        : queries.OrderByDescending(s => s.User.DisplayName);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize).ToList();
        }

        public int GetCountLeads(int eventId, int salesId, string searchValue)
        {
            IQueryable<Lead> queries = _db.Set<Lead>()
                .Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (eventId > 0)
            {
                queries = queries.Where(m => m.EventID == eventId);
            }
            if (salesId > 0)
            {
                queries = queries.Where(m => m.UserID == salesId || (m.User != null && m.User.TransferUserID == salesId));
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd-MMM-yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.LeadStatusRecord.UpdatedTime == dtSearch || m.CreatedTime == dtSearch);
                else
                {
                    var searchStatus = Enumeration.GetAll<LeadStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchFirstFollowUpStatus = Enumeration.GetAll<FollowUpStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchFinalStatus = Enumeration.GetAll<FinalStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        searchStatus.Contains(m.LeadStatusRecord.Status.Value) ||
                        searchFirstFollowUpStatus.Contains(m.FirstFollowUpStatus.Value) ||
                        searchFinalStatus.Contains(m.FinalStatus.Value) ||
                        m.Company.CompanyName.ToLower().Contains(searchValue) ||
                        m.Company.Country.Code.ToLower().Contains(searchValue) ||
                        m.JobTitle.ToLower().Contains(searchValue) ||
                        m.DirectLine.ToLower().Contains(searchValue) ||
                        (m.Salutation != null && m.Salutation.Contains(searchValue)) ||
                        (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                        (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                        (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                        (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                        (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                        (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                        (m.WorkEmail1 != null && m.WorkEmail1.ToLower().Contains(searchValue)) ||
                        (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)));
                }
            }
            return queries.Count();
        }

        public IEnumerable<Lead> GetAllLeads(int eventId, int salesId, string searchValue, string sortColumnDir,
            string sortColumn, int page, int pageSize)
        {

            IQueryable<Lead> queries = _db.Set<Lead>()
                .Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (eventId > 0)
            {
                queries = queries.Where(m => m.EventID == eventId);
            }
            if (salesId > 0)
            {
                queries = queries.Where(m => m.UserID == salesId || (m.User != null && m.User.TransferUserID == salesId));
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd-MMM-yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.LeadStatusRecord.UpdatedTime == dtSearch);
                else
                {
                    var searchStatus = Enumeration.GetAll<LeadStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchFirstFollowUpStatus = Enumeration.GetAll<FollowUpStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchFinalStatus = Enumeration.GetAll<FinalStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        searchStatus.Contains(m.LeadStatusRecord.Status.Value) ||
                        searchFirstFollowUpStatus.Contains(m.FirstFollowUpStatus.Value) ||
                        searchFinalStatus.Contains(m.FinalStatus.Value) ||
                        m.Company.CompanyName.ToLower().Contains(searchValue) ||
                        m.Company.Country.Code.ToLower().Contains(searchValue) ||
                        m.JobTitle.ToLower().Contains(searchValue) ||
                        m.DirectLine.ToLower().Contains(searchValue) ||
                        (m.Salutation != null && m.Salutation.Contains(searchValue)) ||
                        (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                        (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                        (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                        (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                        (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                        (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                        (m.WorkEmail1 != null && m.WorkEmail1.ToLower().Contains(searchValue)) ||
                        (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)));
                }
            }
            switch (sortColumn)
            {
                case "CreatedTime":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LeadStatusRecord.UpdatedTime)
                        : queries.OrderByDescending(s => s.LeadStatusRecord.UpdatedTime);
                    break;
                case "Company":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.CompanyName)
                        : queries.OrderByDescending(s => s.Company.CompanyName);
                    break;
                case "Country":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.Country.Code)
                        : queries.OrderByDescending(s => s.Company.Country.Code);
                    break;
                case "JobTitle":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.JobTitle)
                        : queries.OrderByDescending(s => s.JobTitle);
                    break;
                case "DirectLine":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.DirectLine)
                        : queries.OrderByDescending(s => s.DirectLine);
                    break;
                case "Salutation":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Salutation)
                        : queries.OrderByDescending(s => s.Salutation);
                    break;
                case "FirstName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FirstName)
                        : queries.OrderByDescending(s => s.FirstName);
                    break;
                case "LastName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LastName)
                        : queries.OrderByDescending(s => s.LastName);
                    break;
                case "MobilePhone1":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone1)
                        : queries.OrderByDescending(s => s.MobilePhone1);
                    break;
                case "MobilePhone2":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone2)
                        : queries.OrderByDescending(s => s.MobilePhone2);
                    break;
                case "MobilePhone3":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone3)
                        : queries.OrderByDescending(s => s.MobilePhone3);
                    break;
                case "WorkEmail":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.WorkEmail)
                        : queries.OrderByDescending(s => s.WorkEmail);
                    break;
                case "PersonalEmail":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.PersonalEmail)
                        : queries.OrderByDescending(s => s.PersonalEmail);
                    break;
                case "EstimatedDelegateNumber":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.EstimatedDelegateNumber)
                        : queries.OrderByDescending(s => s.EstimatedDelegateNumber);
                    break;
                case "TrainingBudgetPerHead":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.TrainingBudgetPerHead)
                        : queries.OrderByDescending(s => s.TrainingBudgetPerHead);
                    break;
                case "GoodTrainingMonth":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.GoodTrainingMonth)
                        : queries.OrderByDescending(s => s.GoodTrainingMonth);
                    break;
                case "StatusDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LeadStatusRecord.Status.Value)
                        : queries.OrderByDescending(s => s.LeadStatusRecord.Status.Value);
                    break;
                case "KPIRemarks":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.KPIRemarks)
                        : queries.OrderByDescending(s => s.KPIRemarks);
                    break;
                case "MarkKPI":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MarkKPI)
                        : queries.OrderByDescending(s => s.MarkKPI);
                    break;
                case "FirstFollowUpStatus":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FirstFollowUpStatus.DisplayName)
                        : queries.OrderByDescending(s => s.FirstFollowUpStatus.DisplayName);
                    break;
                case "FinalStatus":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FinalStatus.DisplayName)
                        : queries.OrderByDescending(s => s.FinalStatus.DisplayName);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize).ToList();
        }

        public int GetCountLeadsForNCL(int eventId, int daysExpired, string searchValue)
        {
            var expiredDate = DateTime.Today.AddDays(-daysExpired);
            IQueryable<Lead> queries = _db.Set<Lead>()
                .Where(m => !m.ExpiredForReopen && m.EventID == eventId && m.EntityStatus.Value == EntityStatus.Normal.Value &&
                            (m.LeadStatusRecord.Status.Value == LeadStatus.Booked.Value ||
                            ((m.User.UserStatus.Value == UserStatus.Live.Value || m.User.DirectSupervisorID > 0) &&
                             (m.LeadStatusRecord.Status.Value == LeadStatus.Blocked.Value ||
                             ((m.LeadStatusRecord.Status.Value == LeadStatus.Live.Value ||
                             m.LeadStatusRecord.Status.Value == LeadStatus.LOI.Value) &&
                                 m.LeadStatusRecord.UpdatedTime >= expiredDate)))));
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd-MMM-yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.LeadStatusRecord.UpdatedTime == dtSearch);
                else
                {
                    var searchStatus = Enumeration.GetAll<LeadStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        searchStatus.Contains(m.LeadStatusRecord.Status.Value) ||
                        m.Company.CompanyName.ToLower().Contains(searchValue) ||
                        m.Company.Country.Code.ToLower().Contains(searchValue) ||
                        m.User.DisplayName.ToLower().Contains(searchValue) ||
                        m.JobTitle.Contains(searchValue));
                }
            }
            return queries.Count();
        }
        public IEnumerable<Lead> GetAllLeadsForNCL(int eventId, int daysExpired, string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize)
        {
            var expiredDate = DateTime.Today.AddDays(-daysExpired);
            IQueryable<Lead> queries = _db.Set<Lead>()
                .Where(m => !m.ExpiredForReopen && m.EventID == eventId && m.EntityStatus.Value == EntityStatus.Normal.Value &&
                            (m.LeadStatusRecord.Status.Value == LeadStatus.Booked.Value ||
                             ((m.User.UserStatus.Value == UserStatus.Live.Value || m.User.DirectSupervisorID > 0) &&
                              (m.LeadStatusRecord.Status.Value == LeadStatus.Blocked.Value ||
                               ((m.LeadStatusRecord.Status.Value == LeadStatus.Live.Value ||
                                 m.LeadStatusRecord.Status.Value == LeadStatus.LOI.Value) &&
                                m.LeadStatusRecord.UpdatedTime >= expiredDate)))));
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd-MMM-yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.LeadStatusRecord.UpdatedTime == dtSearch);
                else
                {
                    var searchStatus = Enumeration.GetAll<LeadStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        searchStatus.Contains(m.LeadStatusRecord.Status.Value) ||
                        m.Company.CompanyName.ToLower().Contains(searchValue) ||
                        m.Company.Country.Code.ToLower().Contains(searchValue) ||
                        m.User.DisplayName.ToLower().Contains(searchValue) ||
                        m.JobTitle.Contains(searchValue));
                }
            }

            switch (sortColumn)
            {
                case "DateCreatedDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LeadStatusRecord.UpdatedTime)
                        : queries.OrderByDescending(s => s.LeadStatusRecord.UpdatedTime);
                    break;
                case "CompanyName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.CompanyName)
                        : queries.OrderByDescending(s => s.Company.CompanyName);
                    break;
                case "CountryCode":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.Country.Code)
                        : queries.OrderByDescending(s => s.Company.Country.Code);
                    break;
                case "JobTitle":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.JobTitle)
                        : queries.OrderByDescending(s => s.JobTitle);
                    break;
                case "Salesman":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.User.DisplayName)
                        : queries.OrderByDescending(s => s.User.DisplayName);
                    break;
                case "StatusDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LeadStatusRecord.Status.Value)
                        : queries.OrderByDescending(s => s.LeadStatusRecord.Status.Value);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize).ToList();
        }

        public int GetCountLeadsForNCLManger(int eventId, string searchValue)
        {
            IQueryable<Lead> queries = _db.Set<Lead>()
                .Where(m => !m.ExpiredForReopen && m.EventID == eventId && m.EntityStatus.Value == EntityStatus.Normal.Value &&
                            (m.LeadStatusRecord.Status.Value == LeadStatus.Booked.Value ||
                             ((m.User.UserStatus.Value == UserStatus.Live.Value ||
                               m.User.DirectSupervisorID > 0) &&
                              (m.LeadStatusRecord.Status.Value == LeadStatus.Blocked.Value ||
                               m.LeadStatusRecord.Status.Value == LeadStatus.Live.Value ||
                               m.LeadStatusRecord.Status.Value == LeadStatus.LOI.Value))));

            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd-MMM-yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.LeadStatusRecord.UpdatedTime == dtSearch);
                else
                {
                    var searchStatus = Enumeration.GetAll<LeadStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchFirstFollowUpStatus = Enumeration.GetAll<FollowUpStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchFinalStatus = Enumeration.GetAll<FinalStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        searchStatus.Contains(m.LeadStatusRecord.Status.Value) ||
                        searchFirstFollowUpStatus.Contains(m.FirstFollowUpStatus.Value) ||
                        searchFinalStatus.Contains(m.FinalStatus.Value) ||
                        m.Company.CompanyName.ToLower().Contains(searchValue) ||
                        m.Company.Country.Code.ToLower().Contains(searchValue) ||
                        m.JobTitle.ToLower().Contains(searchValue) ||
                        m.DirectLine.ToLower().Contains(searchValue) ||
                        (m.Salutation != null && m.Salutation.Contains(searchValue)) ||
                        (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                        (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                        (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                        (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                        (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                        (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                        (m.WorkEmail1 != null && m.WorkEmail1.ToLower().Contains(searchValue)) ||
                        (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)));
                }
            }
            return queries.Count();
        }
        public IEnumerable<Lead> GetAllLeadsForNCLManger(int eventId, string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize)
        {
            IQueryable<Lead> queries = _db.Set<Lead>()
                .Where(m => !m.ExpiredForReopen && m.EventID == eventId && m.EntityStatus.Value == EntityStatus.Normal.Value &&
                            (m.LeadStatusRecord.Status.Value == LeadStatus.Booked.Value ||
                             ((m.User.UserStatus.Value == UserStatus.Live.Value ||
                               m.User.DirectSupervisorID > 0) &&
                              (m.LeadStatusRecord.Status.Value == LeadStatus.Blocked.Value ||
                               m.LeadStatusRecord.Status.Value == LeadStatus.Live.Value ||
                               m.LeadStatusRecord.Status.Value == LeadStatus.LOI.Value))));

            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd-MMM-yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.LeadStatusRecord.UpdatedTime == dtSearch);
                else
                {
                    var searchStatus = Enumeration.GetAll<LeadStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchFirstFollowUpStatus = Enumeration.GetAll<FollowUpStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchFinalStatus = Enumeration.GetAll<FinalStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m =>
                        searchStatus.Contains(m.LeadStatusRecord.Status.Value) ||
                        searchFirstFollowUpStatus.Contains(m.FirstFollowUpStatus.Value) ||
                        searchFinalStatus.Contains(m.FinalStatus.Value) ||
                        m.Company.CompanyName.ToLower().Contains(searchValue) ||
                        m.Company.Country.Code.ToLower().Contains(searchValue) ||
                        m.JobTitle.ToLower().Contains(searchValue) ||
                        m.DirectLine.ToLower().Contains(searchValue) ||
                        (m.Salutation != null && m.Salutation.Contains(searchValue)) ||
                        (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                        (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                        (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                        (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                        (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                        (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                        (m.WorkEmail1 != null && m.WorkEmail1.ToLower().Contains(searchValue)) ||
                        (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)));
                }
            }
            switch (sortColumn)
            {
                case "CreatedTime":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LeadStatusRecord.UpdatedTime)
                        : queries.OrderByDescending(s => s.LeadStatusRecord.UpdatedTime);
                    break;
                case "Company":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.CompanyName)
                        : queries.OrderByDescending(s => s.Company.CompanyName);
                    break;
                case "Country":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Company.Country.Code)
                        : queries.OrderByDescending(s => s.Company.Country.Code);
                    break;
                case "JobTitle":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.JobTitle)
                        : queries.OrderByDescending(s => s.JobTitle);
                    break;
                case "DirectLine":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.DirectLine)
                        : queries.OrderByDescending(s => s.DirectLine);
                    break;
                case "Salutation":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Salutation)
                        : queries.OrderByDescending(s => s.Salutation);
                    break;
                case "FirstName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FirstName)
                        : queries.OrderByDescending(s => s.FirstName);
                    break;
                case "LastName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LastName)
                        : queries.OrderByDescending(s => s.LastName);
                    break;
                case "MobilePhone1":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone1)
                        : queries.OrderByDescending(s => s.MobilePhone1);
                    break;
                case "MobilePhone2":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone2)
                        : queries.OrderByDescending(s => s.MobilePhone2);
                    break;
                case "MobilePhone3":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone3)
                        : queries.OrderByDescending(s => s.MobilePhone3);
                    break;
                case "WorkEmail":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.WorkEmail)
                        : queries.OrderByDescending(s => s.WorkEmail);
                    break;
                case "PersonalEmail":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.PersonalEmail)
                        : queries.OrderByDescending(s => s.PersonalEmail);
                    break;
                case "EstimatedDelegateNumber":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.EstimatedDelegateNumber)
                        : queries.OrderByDescending(s => s.EstimatedDelegateNumber);
                    break;
                case "TrainingBudgetPerHead":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.TrainingBudgetPerHead)
                        : queries.OrderByDescending(s => s.TrainingBudgetPerHead);
                    break;
                case "GoodTrainingMonth":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.GoodTrainingMonth)
                        : queries.OrderByDescending(s => s.GoodTrainingMonth);
                    break;
                case "StatusDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LeadStatusRecord.Status.Value)
                        : queries.OrderByDescending(s => s.LeadStatusRecord.Status.Value);
                    break;
                case "KPIRemarks":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.KPIRemarks)
                        : queries.OrderByDescending(s => s.KPIRemarks);
                    break;
                case "MarkKPI":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MarkKPI)
                        : queries.OrderByDescending(s => s.MarkKPI);
                    break;
                case "FirstFollowUpStatus":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FirstFollowUpStatus.DisplayName)
                        : queries.OrderByDescending(s => s.FirstFollowUpStatus.DisplayName);
                    break;
                case "FinalStatus":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FinalStatus.DisplayName)
                        : queries.OrderByDescending(s => s.FinalStatus.DisplayName);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LeadStatusRecord.Status.Value).ThenByDescending(s => s.LeadStatusRecord.UpdatedTime)
                        : queries.OrderByDescending(s => s.LeadStatusRecord.Status.Value).ThenByDescending(s => s.LeadStatusRecord.UpdatedTime);
                    break;
            }
            return queries.Skip(page).Take(pageSize).ToList();
        }

        public bool CheckCompaniesInNCL(int eventId, int comId, int userId, int daysExpired)
        {
            var expiredDate = DateTime.Today.AddDays(-daysExpired);
            IQueryable<Lead> queries = _db.Set<Lead>()
                .Where(m => m.CompanyID == comId && m.EventID == eventId && !m.ExpiredForReopen &&
                            m.EntityStatus.Value == EntityStatus.Normal.Value &&
                            (m.UserID != userId
                            //&& m.User.TransferUserID != userId
                            ) &&
                            (m.LeadStatusRecord.Status.Value == LeadStatus.Booked.Value ||
                             ((m.User.UserStatus.Value == UserStatus.Live.Value || m.User.DirectSupervisorID > 0) &&
                              (m.LeadStatusRecord.Status.Value == LeadStatus.Blocked.Value ||
                               ((m.LeadStatusRecord.Status.Value == LeadStatus.Live.Value ||
                                 m.LeadStatusRecord.Status.Value == LeadStatus.LOI.Value) &&
                                m.LeadStatusRecord.UpdatedTime >= expiredDate)))));
            return queries.Any();
        }
        public int CountCompaniesBlocked(int userId)
        {
            IQueryable<Lead> queries = _db.Set<Lead>()
                .Where(m => !m.ExpiredForReopen && (m.Event.EventStatus.Value == EventStatus.Live.Value ||
                                                    m.Event.EventStatus.Value == EventStatus.Confirmed.Value) &&
                            (m.User.TransferUserID == userId || m.UserID == userId) &&
                            m.LeadStatusRecord.Status.Value == LeadStatus.Blocked.Value);
            return queries.Count();
        }
        public IEnumerable<int> GetCompaniesInNCL(int eventId, int userId, int daysExpired)
        {
            var expiredDate = DateTime.Today.AddDays(-daysExpired);
            IQueryable<Lead> queries = _db.Set<Lead>()
                .Where(m => m.EventID == eventId && !m.ExpiredForReopen &&
                            m.EntityStatus.Value == EntityStatus.Normal.Value &&
                            (m.UserID != userId
                            //&& m.User.TransferUserID != userId
                            ) &&
                            (m.LeadStatusRecord.Status.Value == LeadStatus.Booked.Value ||
                             ((m.User.UserStatus.Value == UserStatus.Live.Value || m.User.DirectSupervisorID > 0) &&
                              (m.LeadStatusRecord.Status.Value == LeadStatus.Blocked.Value ||
                               ((m.LeadStatusRecord.Status.Value == LeadStatus.Live.Value ||
                                 m.LeadStatusRecord.Status.Value == LeadStatus.LOI.Value) &&
                                m.LeadStatusRecord.UpdatedTime >= expiredDate)))));
            return queries.ToList().Select(m => m.CompanyID);
        }
        public IEnumerable<Lead> GetAllLeads(Func<Lead, bool> predicate)
        {
            Func<Lead, bool> predicate2 =
                m => predicate(m) && m.LeadStatusRecord != LeadStatus.Deleted;
            return GetAll(predicate2, m => m.Event, m => m.LeadStatusRecord).AsEnumerable();
        }

        public IEnumerable<Lead> GetAllLeadsForKPI(int eventId, int userId, DateTime dateFrom, DateTime dateTo, string searchValue)
        {
            dateTo = dateTo.AddDays(1);
            var queries = _db.Set<Lead>().Where(m =>
                dateFrom <= m.CreatedTime &&
                m.CreatedTime < dateTo &&
                m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value);
            if (!string.IsNullOrEmpty(searchValue))
                queries = queries.Where(m => m.User.DisplayName.Contains(searchValue));
            if (eventId > 0)
            {
                queries = queries.Where(m => m.EventID == eventId);
            }
            if (userId > 0)
            {
                queries = queries.Where(m => m.UserID == userId);
            }
            return queries.ToList();
        }
        public IEnumerable<Lead> GetAllLeadsOfUserForMerge(int userId)
        {
            var queries = _db.Set<Lead>().Where(m =>
                m.UserID == userId &&
                m.Event.EventStatus.Value != EventStatus.Completed.Value);
            return queries.Include(m => m.Company).ToList();
        }
        public IEnumerable<Lead> GetAllLeadsForMarkKPIs(int eventId)
        {
            var queries = _db.Set<Lead>().Where(m =>
                !m.MarkKPI &&
                (m.LeadStatusRecord.Status.Value != LeadStatus.Reject.Value &&
                 m.LeadStatusRecord.Status.Value != LeadStatus.Initial.Value &&
                 m.LeadStatusRecord.Status.Value != LeadStatus.Deleted.Value));
            if (eventId > 0)
            {
                queries = queries.Where(m => m.EventID == eventId);
            }
            return queries.ToList();
        }
        public IEnumerable<Lead> GetAllLeadsForMakeExpiredLead()
        {
            var today = DateTime.Today;
            var queries = _db.Set<Lead>().Where(m => !m.ExpiredForReopen &&
                                                     (m.LeadStatusRecord.Status.Value != LeadStatus.Booked.Value) &&
                                                     m.Event.ClosingDate != null &&
                                                     m.Event.ClosingDate < today);
            return queries.ToList();
        }

        public Lead GetLead(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<Lead>().FirstOrDefault(m => m.ID == id);
            //return Get<Lead>(m => m.ID == id, u => new
            //{
            //    u.Event,
            //    u.Company
            //});
        }

        public Lead CreateLead(Lead info)
        {
            info = Create(info);
            if (info != null)
            {
                info.Company = Get<Company>(info.CompanyID);
            }
            return info;
        }

        public bool UpdateLead(Lead info)
        {
            return Update(info);
        }
        public bool UpdateAttachment(LeadStatusRecord record)
        {
            return Update(record);
        }

        public bool DeleteLead(int id)
        {
            return Delete<Lead>(id);
        }
        public PhoneCall CreatePhoneCall(PhoneCall info)
        {
            return Create(info);
        }

        public bool UpdatePhoneCall(PhoneCall info)
        {
            var exist = Get<PhoneCall>(info.ID);
            if (exist != null)
            {
                exist.Remark = info.Remark;
                exist.CallBackDate = info.CallBackDate;
                exist.CallBackTiming = info.CallBackTiming;
                return Update(exist);
            }
            return false;
        }

        public PhoneCall GetPhoneCall(int id)
        {
            return Get<PhoneCall>(id);
        }
    }
}
