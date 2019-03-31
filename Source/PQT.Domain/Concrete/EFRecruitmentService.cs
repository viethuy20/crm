using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Concrete
{
    public class EFRecruitmentService : Repository, IRecruitmentService
    {
        public EFRecruitmentService(DbContext db)
            : base(db)
        {
        }

        //public string GetTempCandidateNo()
        //{
        //    return string.Format("CAN{0}", GetNextTempCounter("Candidate", 1).ToString("D3"));
        //}

        public int GetCountCandidates(string searchValue)
        {
            var queries = _db.Set<Candidate>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value &&
                m.CandidateStatusRecord.Status.Value != CandidateStatus.Deleted.Value);
            if (string.IsNullOrEmpty(searchValue)) return queries.Count();
            bool isValid = DateTime.TryParseExact(
                searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dtSearch);
            if (isValid)
                queries = queries.Where(m => m.PsSummary.DateSelected == dtSearch ||
                                            m.OneFaceToFaceSummary.DateSelected == dtSearch ||
                                            m.TwoFaceToFaceSummary.DateSelected == dtSearch);
            else
                queries = queries.Where(m => m.CandidateNo.ToLower().Contains(searchValue) ||
                                        m.EnglishName != null && m.EnglishName.ToLower().Contains(searchValue) ||
                                        m.FirstName.ToLower().Contains(searchValue) ||
                                        m.LastName.ToLower().Contains(searchValue) ||
                                        m.MobileNumber.Contains(searchValue) ||
                                        m.PersonalEmail.Contains(searchValue) ||
                                        m.ApplicationSource != null &&
                                        m.ApplicationSource.ToLower().Contains(searchValue) ||
                                        m.OfficeLocation != null &&
                                        m.OfficeLocation.Name.ToLower().Contains(searchValue));
            return queries.Count();
        }
        public int GetCountCandidatesInterviewToday(string searchValue)
        {
            var today = DateTime.Today;
            var queries = _db.Set<Candidate>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value &&
                m.CandidateStatusRecord.Status.Value != CandidateStatus.Deleted.Value &&
                ((m.PsSummary != null && m.PsSummary.DateSelected == today) ||
                (m.OneFaceToFaceSummary != null && m.OneFaceToFaceSummary.DateSelected == today) ||
                (m.TwoFaceToFaceSummary != null && m.TwoFaceToFaceSummary.DateSelected == today)));
            if (string.IsNullOrEmpty(searchValue)) return queries.Count();
            bool isValid = DateTime.TryParseExact(
                searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dtSearch);
            if (isValid)
                queries = queries.Where(m => m.PsSummary.DateSelected == dtSearch ||
                                             m.OneFaceToFaceSummary.DateSelected == dtSearch ||
                                             m.TwoFaceToFaceSummary.DateSelected == dtSearch);
            else
                queries = queries.Where(m => m.CandidateNo.ToLower().Contains(searchValue) ||
                                         m.EnglishName != null && m.EnglishName.ToLower().Contains(searchValue) ||
                                         m.FirstName.ToLower().Contains(searchValue) ||
                                         m.LastName.ToLower().Contains(searchValue) ||
                                         m.MobileNumber.Contains(searchValue) ||
                                         m.PersonalEmail.Contains(searchValue) ||
                                         m.ApplicationSource != null &&
                                         m.ApplicationSource.ToLower().Contains(searchValue) ||
                                         m.OfficeLocation != null &&
                                         m.OfficeLocation.Name.ToLower().Contains(searchValue));
            return queries.Count();
        }
        public IEnumerable<Candidate> GetAllCandidates(string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize)
        {
            var queries = _db.Set<Candidate>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value &&
                m.CandidateStatusRecord.Status.Value != CandidateStatus.Deleted.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                {
                    queries = queries.Where(m => m.PsSummary.DateSelected == dtSearch ||
                             m.OneFaceToFaceSummary.DateSelected == dtSearch ||
                             m.TwoFaceToFaceSummary.DateSelected == dtSearch);
                }
                else
                {
                    queries = queries.Where(m => m.CandidateNo.ToLower().Contains(searchValue) ||
                                       m.EnglishName != null && m.EnglishName.ToLower().Contains(searchValue) ||
                                       m.FirstName.ToLower().Contains(searchValue) ||
                                       m.LastName.ToLower().Contains(searchValue) ||
                                       m.MobileNumber.Contains(searchValue) ||
                                       m.PersonalEmail.Contains(searchValue) ||
                                       m.ApplicationSource != null &&
                                       m.ApplicationSource.ToLower().Contains(searchValue) ||
                                       m.OfficeLocation != null &&
                                       m.OfficeLocation.Name.ToLower().Contains(searchValue));
                }

            }
            switch (sortColumn)
            {
                case "CandidateNo":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.CandidateNo) : queries.OrderByDescending(s => s.CandidateNo);
                    break;
                case "CreatedTime":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.CreatedTime) : queries.OrderByDescending(s => s.CreatedTime);
                    break;
                case "EnglishName":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.EnglishName) : queries.OrderByDescending(s => s.EnglishName);
                    break;
                case "FirstName":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.FirstName) : queries.OrderByDescending(s => s.FirstName);
                    break;
                case "LastName":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.LastName) : queries.OrderByDescending(s => s.LastName);
                    break;
                case "MobileNumber":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.MobileNumber) : queries.OrderByDescending(s => s.MobileNumber);
                    break;
                case "PersonalEmail":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.PersonalEmail) : queries.OrderByDescending(s => s.PersonalEmail);
                    break;
                case "ApplicationSource":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.ApplicationSource) : queries.OrderByDescending(s => s.ApplicationSource);
                    break;
                case "OfficeLocationDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.OfficeLocation.Name) : queries.OrderByDescending(s => s.OfficeLocation.Name);
                    break;
                case "PsSummaryDateDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.PsSummary.DateSelected) : queries.OrderByDescending(s => s.PsSummary.DateSelected);
                    break;
                case "PsSummaryInterviewer":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.PsSummary.User.DisplayName) : queries.OrderByDescending(s => s.PsSummary.User.DisplayName);
                    break;
                case "PsSummaryStatusDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.PsSummary.Status.Value) : queries.OrderByDescending(s => s.PsSummary.Status.Value);
                    break;
                case "PsSummaryStatusReason":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.PsSummary.ReasonRejected) : queries.OrderByDescending(s => s.PsSummary.ReasonRejected);
                    break;
                case "OneFaceToFaceSummaryDateDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.OneFaceToFaceSummary.DateSelected) : queries.OrderByDescending(s => s.OneFaceToFaceSummary.DateSelected);
                    break;
                case "OneFaceToFaceSummaryInterviewer":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.OneFaceToFaceSummary.User.DisplayName) : queries.OrderByDescending(s => s.OneFaceToFaceSummary.User.DisplayName);
                    break;
                case "OneFaceToFaceSummaryStatusDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.OneFaceToFaceSummary.Status.Value) : queries.OrderByDescending(s => s.OneFaceToFaceSummary.Status.Value);
                    break;
                case "OneFaceToFaceSummaryStatusReason":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.OneFaceToFaceSummary.ReasonRejected) : queries.OrderByDescending(s => s.OneFaceToFaceSummary.ReasonRejected);
                    break;
                case "TwoFaceToFaceSummaryDateDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.TwoFaceToFaceSummary.DateSelected) : queries.OrderByDescending(s => s.TwoFaceToFaceSummary.DateSelected);
                    break;
                case "TwoFaceToFaceSummaryInterviewer":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.TwoFaceToFaceSummary.User.DisplayName) : queries.OrderByDescending(s => s.TwoFaceToFaceSummary.User.DisplayName);
                    break;
                case "TwoFaceToFaceSummaryStatusDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.TwoFaceToFaceSummary.Status.Value) : queries.OrderByDescending(s => s.TwoFaceToFaceSummary.Status.Value);
                    break;
                case "TwoFaceToFaceSummaryStatusReason":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.TwoFaceToFaceSummary.ReasonRejected) : queries.OrderByDescending(s => s.TwoFaceToFaceSummary.ReasonRejected);
                    break;
                case "UserDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.User.DisplayName) : queries.OrderByDescending(s => s.User.DisplayName);
                    break;
                case "StatusDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.CandidateStatusRecord.Status.Value) : queries.OrderByDescending(s => s.CandidateStatusRecord.Status.Value);
                    break;
                default:
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.ID) : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize)
                .ToList();
        }
        public IEnumerable<Candidate> GetAllCandidatesInterviewToday(string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize)
        {
            var today = DateTime.Today;
            var queries = _db.Set<Candidate>().Where(m => m.EntityStatus.Value == EntityStatus.Normal.Value &&
                m.CandidateStatusRecord.Status.Value != CandidateStatus.Deleted.Value &&
                ((m.PsSummary != null && m.PsSummary.DateSelected == today) ||
                 (m.OneFaceToFaceSummary != null && m.OneFaceToFaceSummary.DateSelected == today) ||
                 (m.TwoFaceToFaceSummary != null && m.TwoFaceToFaceSummary.DateSelected == today)));
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                {
                    queries = queries.Where(m => m.PsSummary.DateSelected == dtSearch ||
                             m.OneFaceToFaceSummary.DateSelected == dtSearch ||
                             m.TwoFaceToFaceSummary.DateSelected == dtSearch);
                }
                else
                {
                    queries = queries.Where(m => m.CandidateNo.ToLower().Contains(searchValue) ||
                                       m.EnglishName != null && m.EnglishName.ToLower().Contains(searchValue) ||
                                       m.FirstName.ToLower().Contains(searchValue) ||
                                       m.LastName.ToLower().Contains(searchValue) ||
                                       m.MobileNumber.Contains(searchValue) ||
                                       m.PersonalEmail.Contains(searchValue) ||
                                       m.ApplicationSource != null &&
                                       m.ApplicationSource.ToLower().Contains(searchValue) ||
                                       m.OfficeLocation != null &&
                                       m.OfficeLocation.Name.ToLower().Contains(searchValue));
                }

            }
            switch (sortColumn)
            {
                case "CandidateNo":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.CandidateNo) : queries.OrderByDescending(s => s.CandidateNo);
                    break;
                case "CreatedTime":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.CreatedTime) : queries.OrderByDescending(s => s.CreatedTime);
                    break;
                case "EnglishName":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.EnglishName) : queries.OrderByDescending(s => s.EnglishName);
                    break;
                case "FirstName":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.FirstName) : queries.OrderByDescending(s => s.FirstName);
                    break;
                case "LastName":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.LastName) : queries.OrderByDescending(s => s.LastName);
                    break;
                case "MobileNumber":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.MobileNumber) : queries.OrderByDescending(s => s.MobileNumber);
                    break;
                case "PersonalEmail":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.PersonalEmail) : queries.OrderByDescending(s => s.PersonalEmail);
                    break;
                case "ApplicationSource":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.ApplicationSource) : queries.OrderByDescending(s => s.ApplicationSource);
                    break;
                case "OfficeLocationDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.OfficeLocation.Name) : queries.OrderByDescending(s => s.OfficeLocation.Name);
                    break;
                case "PsSummaryDateDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.PsSummary.DateSelected) : queries.OrderByDescending(s => s.PsSummary.DateSelected);
                    break;
                case "PsSummaryInterviewer":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.PsSummary.User.DisplayName) : queries.OrderByDescending(s => s.PsSummary.User.DisplayName);
                    break;
                case "PsSummaryStatusDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.PsSummary.Status.Value) : queries.OrderByDescending(s => s.PsSummary.Status.Value);
                    break;
                case "PsSummaryStatusReason":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.PsSummary.ReasonRejected) : queries.OrderByDescending(s => s.PsSummary.ReasonRejected);
                    break;
                case "OneFaceToFaceSummaryDateDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.OneFaceToFaceSummary.DateSelected) : queries.OrderByDescending(s => s.OneFaceToFaceSummary.DateSelected);
                    break;
                case "OneFaceToFaceSummaryInterviewer":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.OneFaceToFaceSummary.User.DisplayName) : queries.OrderByDescending(s => s.OneFaceToFaceSummary.User.DisplayName);
                    break;
                case "OneFaceToFaceSummaryStatusDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.OneFaceToFaceSummary.Status.Value) : queries.OrderByDescending(s => s.OneFaceToFaceSummary.Status.Value);
                    break;
                case "OneFaceToFaceSummaryStatusReason":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.OneFaceToFaceSummary.ReasonRejected) : queries.OrderByDescending(s => s.OneFaceToFaceSummary.ReasonRejected);
                    break;
                case "TwoFaceToFaceSummaryDateDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.TwoFaceToFaceSummary.DateSelected) : queries.OrderByDescending(s => s.TwoFaceToFaceSummary.DateSelected);
                    break;
                case "TwoFaceToFaceSummaryInterviewer":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.TwoFaceToFaceSummary.User.DisplayName) : queries.OrderByDescending(s => s.TwoFaceToFaceSummary.User.DisplayName);
                    break;
                case "TwoFaceToFaceSummaryStatusDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.TwoFaceToFaceSummary.Status.Value) : queries.OrderByDescending(s => s.TwoFaceToFaceSummary.Status.Value);
                    break;
                case "TwoFaceToFaceSummaryStatusReason":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.TwoFaceToFaceSummary.ReasonRejected) : queries.OrderByDescending(s => s.TwoFaceToFaceSummary.ReasonRejected);
                    break;
                case "UserDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.User.DisplayName) : queries.OrderByDescending(s => s.User.DisplayName);
                    break;
                case "StatusDisplay":
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.CandidateStatusRecord.Status.Value) : queries.OrderByDescending(s => s.CandidateStatusRecord.Status.Value);
                    break;
                default:
                    queries = sortColumnDir == "asc" ? queries.OrderBy(s => s.ID) : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize)
                .ToList();
        }
        public IEnumerable<Candidate> GetAllCandidatesForKpis(string searchValue, int userId, DateTime dateFrom, DateTime dateTo)
        {
            dateTo = dateTo.AddDays(1);
            var queries = _db.Set<Candidate>()
                .Where(m => dateFrom <= m.CreatedTime &&
                            m.CreatedTime < dateTo && m.EntityStatus.Value == EntityStatus.Normal.Value &&
                            m.CandidateStatusRecord.Status.Value != CandidateStatus.Deleted.Value);
            if (userId > 0)
            {
                queries = queries.Where(m => m.UserID == userId);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                queries = queries.Where(m => m.User.DisplayName.ToLower().Contains(searchValue));
            }
            return queries
                .ToList();
        }
        public Candidate GetCandidate(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _db.Set<Candidate>().FirstOrDefault(m => m.EntityStatus.Value == EntityStatus.Normal.Value && m.ID == id);
        }
        public Candidate GetCandidateByNo(string number)
        {
            if (number == null)
            {
                return null;
            }
            return _db.Set<Candidate>()
                .FirstOrDefault(m => m.EntityStatus.Value == EntityStatus.Normal.Value && m.CandidateNo == number);
        }
        public Candidate GetExistCandidatesByMobile(int positionId, int locationId, string national, string mobileNumber)
        {
            if (mobileNumber == null)
            {
                return null;
            }
            return _db.Set<Candidate>()
                .FirstOrDefault(m => m.EntityStatus.Value == EntityStatus.Normal.Value &&
                    m.RecruitmentPositionID == positionId &&
                    m.OfficeLocationID == locationId &&
                    m.Nationality == national &&
                    m.MobileNumber != null && 
                    m.MobileNumber == mobileNumber);
        }
        public Candidate GetExistCandidatesByEmail(int positionId, int locationId, string national, string email)
        {
            if (email == null)
            {
                return null;
            }

            return _db.Set<Candidate>()
                .FirstOrDefault(m => m.EntityStatus.Value == EntityStatus.Normal.Value &&
                    m.RecruitmentPositionID == positionId &&
                    m.OfficeLocationID == locationId &&
                    m.Nationality == national &&
                    m.PersonalEmail != null &&
                    m.PersonalEmail == email);
        }
        public Candidate CreateCandidate(Candidate info)
        {
            //var tempNo = GetTempCandidateNo();
            //if (tempNo == info.CandidateNo)
            //{
            info.CandidateNo = string.Format("CAN{0}", GetNextCounter("Candidate", 1).ToString("D3"));
            //}
            //else
            //{
            //    SetCounter("Candidate", info.CandidateNo);
            //}
            info = Create(info);
            return info;
        }
        public bool UpdateCandidate(Candidate info)
        {
            Update(info.PsSummary);
            Update(info.OneFaceToFaceSummary);
            Update(info.TwoFaceToFaceSummary);
            return Update(info);
        }
        public bool DeleteCandidate(int id)
        {
            return Delete<Candidate>(id);
        }
    }
}
