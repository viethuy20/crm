using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface ILeadService
    {

        int CountLeadsByTier(int eventId, int tier);
        int CountLeadsTotalBooked(int eventId, int saleId);
        int CountLeadsTotalLOINCL(int eventId,int saleId, int daysExpired);
        int CountLeadsTotalBlockedNCL(int eventId,int saleId);
        int CountLeadsTotalTotalKPI(int eventId,int saleId);
        IEnumerable<Lead> GetAllLeadsForCallResources(int comId, int userId, string name, string role, string email, string phone);
        IEnumerable<Lead> GetAllLeadsByEvent(int eventId);
        IEnumerable<Lead> GetAllLeadsByEventExcludeReopen(int eventId, int excludeLeaveId);
        int GetCountEnquireLeadsForKpi(bool isSales, User currentUser, int eventId, int userId, string statusCode, DateTime datefrom, DateTime dateto, string searchValue);
        IEnumerable<Lead> GetAllEnquireLeadsForKpi(bool isSales, User currentUser, int eventId, int userId, string statusCode, DateTime datefrom, DateTime dateto, string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        int GetCountLeadsForKpiReview(int eventId, string searchValue);
        IEnumerable<Lead> GetAllLeadsForKpiReview(int eventId, string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        int GetCountLeads(int eventId, int salesId, string searchValue);
        IEnumerable<Lead> GetAllLeads(int eventId, int salesId, string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        int GetCountLeadsForNCL(int eventId, int daysExpired, string searchValue);
        IEnumerable<Lead> GetAllLeadsForNCL(int eventId, int daysExpired, string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        int GetCountLeadsForNCLManger(int eventId,  string searchValue);
        IEnumerable<Lead> GetAllLeadsForNCLManger(int eventId, string searchValue, string sortColumnDir, string sortColumn, int page, int pageSize);
        bool CheckCompaniesInNCL(int eventId,int comId,  int userId, int daysExpired);
        int CountCompaniesBlocked(int userId);
        IEnumerable<int> GetCompaniesInNCL(int eventId, int userId, int daysExpired);
        IEnumerable<Lead> GetAllLeads(Func<Lead, bool> predicate);
        IEnumerable<Lead> GetAllLeadsForKPI(int eventId, int userId, DateTime dateFrom, DateTime dateTo, string searchValue);
        IEnumerable<Lead> GetAllLeadsOfUserForMerge(int userId);
        IEnumerable<Lead> GetAllLeadsForMarkKPIs(int eventId);
        IEnumerable<Lead> GetAllLeadsForMakeExpiredLead();
        Lead GetLead(int id);
        Lead CreateLead(Lead info);
        bool UpdateLead(Lead info);
        bool UpdateAttachment(LeadStatusRecord record);
        bool DeleteLead(int id);


        PhoneCall CreatePhoneCall(PhoneCall info);
        bool UpdatePhoneCall(PhoneCall info);
        PhoneCall GetPhoneCall(int id);
    }
}
