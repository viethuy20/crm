using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IRecruitmentService
    {
        //string GetTempCandidateNo();
        int GetCountCandidates(string searchValue);
        int GetCountCandidatesInterviewToday(string searchValue);
        IEnumerable<Candidate> GetAllCandidates(string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        IEnumerable<Candidate> GetAllCandidatesInterviewToday(string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        IEnumerable<Candidate> GetAllCandidatesForKpis(string searchValue, int userId, DateTime dateFrom, DateTime dateTo);
        Candidate GetCandidate(int id);
        Candidate GetCandidateByNo(string number);
        Candidate GetExistCandidatesByMobile(int positionId, int locationId,string national,string mobileNumber);
        Candidate GetExistCandidatesByEmail(int positionId, int locationId, string national, string email);
        Candidate CreateCandidate(Candidate info);
        bool UpdateCandidate(Candidate info);
        bool DeleteCandidate(int id);
    }
}
