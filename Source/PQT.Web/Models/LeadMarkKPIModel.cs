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
            foreach (var lead in leads)
            {
                CheckKPI(lead);
            }
            return leads;
        }


        public bool ConfirmKPI()
        {
            return TransactionWrapper.Do(() =>
            {
                var leadRepo = DependencyHelper.GetService<ILeadService>();
                var leads = leadRepo.GetAllLeads(m =>
                    (EventID == 0 || m.EventID == EventID) &&
                    //m.CreatedTime.Date >= DateFrom.Date &&
                    //m.CreatedTime.Date <= DateTo.Date &&
                    !m.MarkKPI &&
                    (m.LeadStatusRecord != LeadStatus.Reject &&
                     m.LeadStatusRecord != LeadStatus.Initial));
                var count = 0;
                var totalCount = leads.Count();
                var userId = CurrentUser.Identity.ID;
                foreach (var lead in leads)
                {
                    count += 1;
                    CheckKPI(lead);
                    lead.FileNameImportKPI = Path.GetFileName(FilePath);
                    leadRepo.UpdateLead(lead);
                    var json = new
                    {
                        count,
                        totalCount
                    };
                    ProgressHub.SendMessage(userId,Json.Encode(json));
                }
                return true;
            });
        }

        private void CheckKPI(Lead lead)
        {
            var jobTitle = lead.ClientName.ToLower().Trim();
            var eventKeyworks = lead.Event.CallFilterKeywords != null
                ? lead.Event.CallFilterKeywords.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim())
                : new List<string>();
            if (!eventKeyworks.Any() || eventKeyworks.Any(m => m.Contains(jobTitle)) || eventKeyworks.Any(m => jobTitle.Contains(m)))
            {
                if (!string.IsNullOrEmpty(lead.PersonalEmailAddress) ||
                    !string.IsNullOrEmpty(lead.WorkEmailAddress) ||
                    !string.IsNullOrEmpty(lead.WorkEmailAddress1))
                {

                    var voips = ImportVoIps.Where(m => m.clid == lead.User.Extension && m.dst == PQT.Domain.Helpers.StringHelper.RemoveSpecialCharacters(lead.GeneralLine) && !string.IsNullOrEmpty(m.disposition) && m.disposition.Trim().ToUpper() == "ANSWERED");
                    if (lead.PhoneCalls.Any(m => voips.Any(v => m.StartTime.AddSeconds(-Settings.System.VoIpBuffer()) <= v.CallDateTime && v.CallDateTime <= m.StartTime.AddSeconds(Settings.System.VoIpBuffer()))))
                    {
                        lead.MarkKPI = true;
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
    }
}