using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IRecruitmentService
    {
        IEnumerable<Candidate> GetAllCandidates(Func<Candidate, bool> predicate);
        Candidate GetCandidate(int id);
        Candidate CreateCandidate(Candidate info);
        bool UpdateCandidate(Candidate info);
        bool DeleteCandidate(int id);
    }
}
