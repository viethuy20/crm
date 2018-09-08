using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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

        public IEnumerable<Candidate> GetAllCandidates(Func<Candidate, bool> predicate)
        {
            Func<Candidate, bool> predicate2 =
                m => predicate(m) && m.CandidateStatusRecord != CandidateStatus.Deleted;
            return GetAll(predicate2, m => m.CandidateStatusRecord).AsEnumerable();
        }

        public Candidate GetCandidate(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return Get<Candidate>(m => m.ID == id, u => new
            {
                u.CandidateStatusRecord,
                u.RecruitmentPosition,
                u.PsSummary,
                u.OneFaceToFaceSummary,
                u.TwoFaceToFaceSummary,
                u.User,
                u.OfficeLocation,
            });
        }

        public Candidate CreateCandidate(Candidate info)
        {
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
