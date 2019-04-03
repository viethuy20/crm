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
    public class EFLeadNewService : Repository, ILeadNewService
    {
        public EFLeadNewService(DbContext db)
            : base(db)
        {
        }


        public int GetCountLeadNews(int saleId, string searchValue)
        {
            var queries = _db.Set<LeadNew>().Where(m => m.AssignUserID == null && m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (saleId > 0)
            {
                queries = queries.Where(m => m.UserID == saleId);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.CreatedTime == dtSearch ||
                                                 m.NewDateFrom == dtSearch ||
                                                 m.NewDateTo == dtSearch);
                else
                {
                    var searchFirstFollowUpStatuss = Enumeration.GetAll<FollowUpStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchFinalStatuss = Enumeration.GetAll<FinalStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m => m.Company.CompanyName.ToLower().Contains(searchValue) ||
                                                 m.Company.Country.Code.ToLower().Contains(searchValue) ||
                                                 m.JobTitle.ToLower().Contains(searchValue) ||
                                                 m.DirectLine.ToLower().Contains(searchValue) ||
                                                 (m.Salutation != null && m.Salutation.ToLower().Contains(searchValue)) ||
                                                 (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                                                 (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                                                 (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                                                 (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                                                 (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                                                 (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                                                 (m.WorkEmail1 != null && m.WorkEmail1.ToLower().Contains(searchValue)) ||
                                                 (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                                                 (m.NewTopics != null && m.NewTopics.ToLower().Contains(searchValue)) ||
                                                 (m.NewLocations != null && m.NewLocations.ToLower().Contains(searchValue)) ||
                                                 searchFirstFollowUpStatuss.Contains(m.FirstFollowUpStatus.Value) ||
                                                 searchFinalStatuss.Contains(m.FinalStatus.Value));

                }
            }

            return queries.Include(m => m.AssignUser).Count();
        }
        public IEnumerable<LeadNew> GetAllLeadNews(int saleId, string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize)
        {
            var queries = _db.Set<LeadNew>().Where(m => m.AssignUserID == null && m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (saleId > 0)
            {
                queries = queries.Where(m => m.UserID == saleId);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.CreatedTime == dtSearch ||
                                                 m.NewDateFrom == dtSearch ||
                                                 m.NewDateTo == dtSearch);
                else
                {
                    var searchFirstFollowUpStatuss = Enumeration.GetAll<FollowUpStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchFinalStatuss = Enumeration.GetAll<FinalStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m => m.Company.CompanyName.ToLower().Contains(searchValue) ||
                                                 m.Company.Country.Code.ToLower().Contains(searchValue) ||
                                                 m.JobTitle.ToLower().Contains(searchValue) ||
                                                 m.DirectLine.ToLower().Contains(searchValue) ||
                                                 (m.Salutation != null && m.Salutation.ToLower().Contains(searchValue)) ||
                                                 (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                                                 (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                                                 (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                                                 (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                                                 (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                                                 (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                                                 (m.WorkEmail1 != null && m.WorkEmail1.ToLower().Contains(searchValue)) ||
                                                 (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                                                 (m.NewTopics != null && m.NewTopics.ToLower().Contains(searchValue)) ||
                                                 (m.NewLocations != null && m.NewLocations.ToLower().Contains(searchValue)) ||
                                                 searchFirstFollowUpStatuss.Contains(m.FirstFollowUpStatus.Value) ||
                                                 searchFinalStatuss.Contains(m.FinalStatus.Value));

                }
            }
            switch (sortColumn)
            {
                case "CreatedTime":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.CreatedTime)
                        : queries.OrderByDescending(s => s.CreatedTime);
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
                case "Sales":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.AssignUser.DisplayName)
                        : queries.OrderByDescending(s => s.AssignUser.DisplayName);
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
                case "FirstFollowUpStatus":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FirstFollowUpStatus.Value)
                        : queries.OrderByDescending(s => s.FirstFollowUpStatus.Value);
                    break;
                case "FinalStatus":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FinalStatus.Value)
                        : queries.OrderByDescending(s => s.FinalStatus.Value);
                    break;
                case "NewTopics":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.NewTopics)
                        : queries.OrderByDescending(s => s.NewTopics);
                    break;
                case "NewLocations":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.NewLocations)
                        : queries.OrderByDescending(s => s.NewLocations);
                    break;
                case "NewDateFrom":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.NewDateFrom)
                        : queries.OrderByDescending(s => s.NewDateFrom);
                    break;
                case "NewTrainingTypeDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.NewTrainingType)
                        : queries.OrderByDescending(s => s.NewTrainingType);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize).Include(m => m.AssignUser)
                .ToList();
        }

        public int GetCountLeadNewsForAssigned(int saleId, string searchValue)
        {
            var queries = _db.Set<LeadNew>().Where(m => m.AssignUserID != null && m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (saleId > 0)
            {
                queries = queries.Where(m => m.AssignUserID == saleId ||
                                             m.AssignUser.TransferUserID == saleId);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.AssignDate == dtSearch ||
                                                 m.NewDateFrom == dtSearch ||
                                                 m.NewDateTo == dtSearch);
                else
                {
                    var searchFirstFollowUpStatuss = Enumeration.GetAll<FollowUpStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchFinalStatuss = Enumeration.GetAll<FinalStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m => m.Company.CompanyName.ToLower().Contains(searchValue) ||
                                                 m.Company.Country.Code.ToLower().Contains(searchValue) ||
                                                 m.JobTitle.ToLower().Contains(searchValue) ||
                                                 m.DirectLine.ToLower().Contains(searchValue) ||
                                                 (m.AssignUser != null && m.AssignUser.DisplayName.ToLower().Contains(searchValue)) ||
                                                 (m.Salutation != null && m.Salutation.ToLower().Contains(searchValue)) ||
                                                 (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                                                 (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                                                 (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                                                 (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                                                 (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                                                 (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                                                 (m.WorkEmail1 != null && m.WorkEmail1.ToLower().Contains(searchValue)) ||
                                                 (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                                                 (m.NewTopics != null && m.NewTopics.ToLower().Contains(searchValue)) ||
                                                 (m.NewLocations != null && m.NewLocations.ToLower().Contains(searchValue)) ||
                                                 searchFirstFollowUpStatuss.Contains(m.FirstFollowUpStatus.Value) ||
                                                 searchFinalStatuss.Contains(m.FinalStatus.Value));

                }
            }

            return queries.Include(m => m.AssignUser).Count();
        }
        public IEnumerable<LeadNew> GetAllLeadNewsForAssigned(int saleId, string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize)
        {
            var queries = _db.Set<LeadNew>().Where(m => m.AssignUserID != null && m.EntityStatus.Value == EntityStatus.Normal.Value);
            if (saleId > 0)
            {
                queries = queries.Where(m => m.AssignUserID == saleId ||
                                             m.AssignUser.TransferUserID == saleId);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m => m.AssignDate == dtSearch ||
                                                 m.NewDateFrom == dtSearch ||
                                                 m.NewDateTo == dtSearch);
                else
                {
                    var searchFirstFollowUpStatuss = Enumeration.GetAll<FollowUpStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    var searchFinalStatuss = Enumeration.GetAll<FinalStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m => m.Company.CompanyName.ToLower().Contains(searchValue) ||
                                                 m.Company.Country.Code.ToLower().Contains(searchValue) ||
                                                 m.JobTitle.ToLower().Contains(searchValue) ||
                                                 m.DirectLine.ToLower().Contains(searchValue) ||
                                                 (m.AssignUser != null && m.AssignUser.DisplayName.ToLower().Contains(searchValue)) ||
                                                 (m.Salutation != null && m.Salutation.ToLower().Contains(searchValue)) ||
                                                 (m.FirstName != null && m.FirstName.ToLower().Contains(searchValue)) ||
                                                 (m.LastName != null && m.LastName.ToLower().Contains(searchValue)) ||
                                                 (m.MobilePhone1 != null && m.MobilePhone1.Contains(searchValue)) ||
                                                 (m.MobilePhone2 != null && m.MobilePhone2.Contains(searchValue)) ||
                                                 (m.MobilePhone3 != null && m.MobilePhone3.Contains(searchValue)) ||
                                                 (m.WorkEmail != null && m.WorkEmail.ToLower().Contains(searchValue)) ||
                                                 (m.WorkEmail1 != null && m.WorkEmail1.ToLower().Contains(searchValue)) ||
                                                 (m.PersonalEmail != null && m.PersonalEmail.ToLower().Contains(searchValue)) ||
                                                 (m.NewTopics != null && m.NewTopics.ToLower().Contains(searchValue)) ||
                                                 (m.NewLocations != null && m.NewLocations.ToLower().Contains(searchValue)) ||
                                                 searchFirstFollowUpStatuss.Contains(m.FirstFollowUpStatus.Value) ||
                                                 searchFinalStatuss.Contains(m.FinalStatus.Value));

                }
            }
            switch (sortColumn)
            {
                case "AssignDate":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.AssignDate)
                        : queries.OrderByDescending(s => s.AssignDate);
                    break;
                case "CreatedTime":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.CreatedTime)
                        : queries.OrderByDescending(s => s.CreatedTime);
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
                case "Sales":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.AssignUser.DisplayName)
                        : queries.OrderByDescending(s => s.AssignUser.DisplayName);
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
                case "FirstFollowUpStatus":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FirstFollowUpStatus.Value)
                        : queries.OrderByDescending(s => s.FirstFollowUpStatus.Value);
                    break;
                case "FinalStatus":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FinalStatus.Value)
                        : queries.OrderByDescending(s => s.FinalStatus.Value);
                    break;
                case "NewTopics":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.NewTopics)
                        : queries.OrderByDescending(s => s.NewTopics);
                    break;
                case "NewLocations":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.NewLocations)
                        : queries.OrderByDescending(s => s.NewLocations);
                    break;
                case "NewDateFrom":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.NewDateFrom)
                        : queries.OrderByDescending(s => s.NewDateFrom);
                    break;
                case "NewTrainingTypeDisplay":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.NewTrainingType)
                        : queries.OrderByDescending(s => s.NewTrainingType);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize).Include(m => m.AssignUser)
                .ToList();
        }

        public IEnumerable<LeadNew> GetAllLeadNews(Func<LeadNew, bool> predicate)
        {
            Func<LeadNew, bool> predicate2 =
                m => predicate(m);
            return GetAll(predicate2, m => m.Event, m => m.AssignUser).AsEnumerable();
        }

        public IEnumerable<LeadNew> GetAllLeadNewsForKPI(int eventId, int userId, DateTime dateFrom, DateTime dateTo, string searchValue)
        {
            dateTo = dateTo.AddDays(1);
            var queries = _db.Set<LeadNew>().Where(m =>
                dateFrom <= m.FirstAssignDate &&
                m.FirstAssignDate < dateTo &&
                m.AssignUserID > 0);
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
            return queries.Include(m => m.User).ToList();
        }
        public LeadNew GetLeadNew(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<LeadNew>().FirstOrDefault(m => m.ID == id);
            //return Get<LeadNew>(m => m.ID == id, u => new
            //{
            //    u.Event,
            //    u.Company
            //});
        }

        public LeadNew CreateLeadNew(LeadNew info)
        {
            info = Create(info);
            if (info != null)
            {
                info.Company = _db.Set<Company>().FirstOrDefault(m => m.ID == info.CompanyID);
            }
            return info;
        }

        public bool UpdateLeadNew(LeadNew info)
        {
            return Update(info);
        }
        public bool DeleteLeadNew(int id)
        {
            return Delete<LeadNew>(id);
        }
    }
}

