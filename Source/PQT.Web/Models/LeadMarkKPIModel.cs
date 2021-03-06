using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using PQT.Domain;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Hubs;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Models
{
    public class LeadMarkKPIModel
    {
        public int EventID { get; set; }
        public Event Event { get; set; }
        public string SessionName { get; set; }
        public string FilePath { get; set; }
        public HttpPostedFileBase FileImport { get; set; }
        public List<VoIpJson> ImportVoIps { get; set; }
        public IEnumerable<Lead> Leads { get; set; }
        public string Message { get; set; }
        public bool ImportError { get; set; }
        public bool IsSuccess { get; set; }
        public LeadMarkKPIModel()
        {
            ImportVoIps = new List<VoIpJson>();
            Leads = new List<Lead>();
        }

        public bool SaveFile()
        {
            if (FileImport != null)
            {
                FilePath = ExcelUploadHelper.SaveFile(FileImport, FolderUpload.KPI);
                return true;
            }
            return false;
        }
        public void ImportVoIp()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                return;
            }
            DataTable dtSheetName = ExcelUploadHelper.GetUploadedExcelSpreadsheetName(FilePath);
            ImportVoIps = new List<VoIpJson>();
            foreach (DataRow row in dtSheetName.Rows)
            {
                string strSheetName = row["TABLE_NAME"].ToString();
                if (strSheetName.Contains("FilterDatabase"))
                {
                    continue;
                }
                DataTable dt = ExcelUploadHelper.GetDataFromExcel(FilePath, strSheetName);

                foreach (DataRow dtRow in dt.Rows)
                {
                    if (dtRow[0] != null && !string.IsNullOrEmpty(dtRow[0].ToString().Trim()))
                    {
                        var temp = new VoIpJson();
                        try
                        {
                            temp.calldate = dtRow[0].ToString();
                            if (!string.IsNullOrEmpty(temp.calldate))
                            {
                                temp.CallDateTime = Convert.ToDateTime(temp.calldate);
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Call Date is invalid<br/>";
                        }

                        try
                        {
                            temp.clid = dtRow[1].ToString();

                            if (string.IsNullOrEmpty(temp.clid))
                            {
                                temp.Error += "- clid is required<br/>";
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- clid is required<br/>";
                        }
                        try
                        {
                            temp.dst = dtRow[2].ToString();
                            if (string.IsNullOrEmpty(temp.dst))
                            {
                                temp.Error += "- dst is required<br/>";
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- dst is required<br/>";
                        }
                        try
                        {
                            temp.lastapp = dtRow[3].ToString();
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- lastapp is required<br/>";
                        }
                        try
                        {
                            temp.duration = dtRow[4].ToString();
                            if (string.IsNullOrEmpty(temp.duration))
                            {
                                temp.Error += "- duration is required<br/>";
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- duration is required<br/>";
                        }
                        try
                        {
                            temp.disposition = dtRow[5].ToString();
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- disposition is required<br/>";
                        }
                        ImportVoIps.Add(temp);
                    }
                }
            }
        }
        public IEnumerable<Lead> MarkKPI(IEnumerable<Lead> leads)
        {
            var voipBuffer = Settings.KPI.VoIpBuffer();
            var exceptCodes = Settings.KPI.ExceptCodes();
            var eventKeyworks = new List<string>();
            var firstLead = leads.FirstOrDefault();
            var eventData = firstLead != null ? firstLead.Event : null;
            if (eventData != null)
            {
                eventKeyworks = eventData.PrimaryJobtitleKeywords != null
                    ? eventData.PrimaryJobtitleKeywords.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(m => m.ToLower().Trim()).ToList()
                    : new List<string>();
                eventKeyworks.AddRange(eventData.SecondaryJobtitleKeywords != null
                    ? eventData.SecondaryJobtitleKeywords.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(m => m.ToLower().Trim())
                    : new List<string>());
            }
            foreach (var lead in leads)
            {
                CheckKPI(lead, voipBuffer, exceptCodes, eventKeyworks.ToArray());
            }
            return leads;
        }


        public bool ConfirmKPI()
        {
            return TransactionWrapper.Do(() =>
            {
                var leadRepo = DependencyHelper.GetService<ILeadService>();
                var eventRepo = DependencyHelper.GetService<IEventService>();
                var leads = leadRepo.GetAllLeadsForMarkKPIs(EventID);
                var count = 0;
                var totalCount = leads.Count();
                var userId = CurrentUser.Identity.ID;
                var voipBuffer = Settings.KPI.VoIpBuffer();
                var exceptCodes = Settings.KPI.ExceptCodes();
                var eventKeyworks = new List<string>();
                var eventData = eventRepo.GetEvent(EventID);
                if (eventData != null)
                {
                    eventKeyworks = eventData.PrimaryJobtitleKeywords != null
                        ? eventData.PrimaryJobtitleKeywords.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim()).ToList()
                        : new List<string>();
                    eventKeyworks.AddRange(eventData.SecondaryJobtitleKeywords != null
                        ? eventData.SecondaryJobtitleKeywords.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(m => m.ToLower().Trim())
                        : new List<string>());
                }

                foreach (var lead in leads)
                {
                    count += 1;
                    CheckKPI(lead, voipBuffer, exceptCodes, eventKeyworks.ToArray());
                    lead.FileNameImportKPI = Path.GetFileName(FilePath);
                    leadRepo.UpdateLead(lead);
                    var json = new
                    {
                        count,
                        totalCount
                    };
                    ProgressHub.SendMessage(userId, Json.Encode(json));
                }
                return true;
            });
        }

        private void CheckKPI(Lead lead, int voipBuffer, string[] exceptCodes, string[] eventKeyworks)
        {
            var jobTitle = lead.JobTitle.ToLower().Trim();
            if (!eventKeyworks.Any() || eventKeyworks.Any(m => m.Contains(jobTitle)) || eventKeyworks.Any(m => jobTitle.Contains(m)))
            {
                if (!string.IsNullOrEmpty(lead.PersonalEmail) ||
                    !string.IsNullOrEmpty(lead.WorkEmail) ||
                    !string.IsNullOrEmpty(lead.WorkEmail1))
                {
                    var mobilePhone1 = PQT.Domain.Helpers.StringHelper.RemoveSpecialCharacters(lead.DialingCode + lead.MobilePhone1);
                    var mobilePhone2 = PQT.Domain.Helpers.StringHelper.RemoveSpecialCharacters(lead.DialingCode + lead.MobilePhone2);
                    var mobilePhone3 = PQT.Domain.Helpers.StringHelper.RemoveSpecialCharacters(lead.DialingCode + lead.MobilePhone3);
                    var directLine = PQT.Domain.Helpers.StringHelper.RemoveSpecialCharacters(lead.DialingCode + lead.DirectLine);
                    //foreach (var exceptCode in exceptCodes)
                    //{
                    //    if (mobilePhone1 != null && mobilePhone1.Substring(0, exceptCode.Length) == exceptCode)
                    //        mobilePhone1 = mobilePhone1.Substring(exceptCode.Length);
                    //    if (mobilePhone2 != null && mobilePhone2.Substring(0, exceptCode.Length) == exceptCode)
                    //        mobilePhone2 = mobilePhone2.Substring(exceptCode.Length);
                    //    if (mobilePhone3 != null && mobilePhone3.Substring(0, exceptCode.Length) == exceptCode)
                    //        mobilePhone3 = mobilePhone3.Substring(exceptCode.Length);
                    //    if (directLine != null && directLine.Substring(0, exceptCode.Length) == exceptCode)
                    //        directLine = directLine.Substring(exceptCode.Length);
                    //}
                    var voips = ImportVoIps.Where(m =>
                        m.clid == lead.User.Extension && !string.IsNullOrEmpty(m.dst) && (
                            m.dstExceptCode(exceptCodes) == directLine ||
                            m.dstExceptCode(exceptCodes) == mobilePhone1 ||
                            m.dstExceptCode(exceptCodes) == mobilePhone2 ||
                            m.dstExceptCode(exceptCodes) == mobilePhone3
                        ) && !string.IsNullOrEmpty(m.disposition) && m.disposition.Trim().ToUpper() == "ANSWERED");
                    if (voips.Any())
                    {
                        if (lead.PhoneCalls.Any(m => voips.Any(v => m.StartTime.AddSeconds(-voipBuffer) <= v.CallDateTime && v.CallDateTime <= m.StartTime.AddSeconds(voipBuffer))))
                        {
                            lead.MarkKPI = true;
                            lead.KPIRemarks = "";
                        }
                        else
                        {
                            lead.KPIRemarks = "Call Time incorrect";
                        }
                    }
                    else
                    {
                        lead.KPIRemarks = "Call doesn't exist in VOIP File";
                    }
                }
                else
                {
                    lead.KPIRemarks = "Email must not be empty";
                }
            }
            else
            {
                lead.KPIRemarks = "Job title does not match keywords";
            }
        }
    }

    public class VoIpJson
    {
        public string calldate { get; set; }
        public DateTime CallDateTime { get; set; }
        public string clid { get; set; } // = user extension
        public string dst { get; set; }
        public string lastapp { get; set; }//Dial,Playback,Congestion,Busy
        public string duration { get; set; }
        public string disposition { get; set; } //ANSWERED, FAILED, NO ANSWER , BUSY
        public string Error { get; set; }
        private string _dstRemoveCode;
        public string dstExceptCode(string[] exceptCodes)
        {
            if (string.IsNullOrEmpty(_dstRemoveCode) && !string.IsNullOrEmpty(dst))
            {
                _dstRemoveCode = dst;
                foreach (var exceptCode in exceptCodes)
                {
                    if (_dstRemoveCode != null && _dstRemoveCode.Length > exceptCode.Length && _dstRemoveCode.Substring(0, exceptCode.Length) == exceptCode)
                    {
                        _dstRemoveCode = _dstRemoveCode.Substring(exceptCode.Length);
                        break;
                    }
                }
            }
            return _dstRemoveCode;
        }
    }
}