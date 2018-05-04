using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using PQT.Domain;
using PQT.Domain.Abstract;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Models
{
    public class LeadMarkKPIModel
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }


        public string SessionName { get; set; }
        public string FilePath { get; set; }
        public HttpPostedFileBase FileImport { get; set; }
        public List<VoIpJson> ImportVoIps { get; set; }
        public LeadMarkKPIModel()
        {
            DateTo = DateTime.Today;
            DateFrom = DateTime.Today.AddDays(-30);
        }

        public void ImportVoIp()
        {
            DataTable dtSheetName = ExcelUploadHelper.GetUploadedExcelSpreadsheetName(FilePath);
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
                        temp.clid = dtRow[1].ToString();
                        temp.dst = dtRow[2].ToString();
                        temp.lastapp = dtRow[3].ToString();
                        temp.duration = dtRow[4].ToString();
                        temp.disposition = dtRow[5].ToString();
                        ImportVoIps.Add(temp);
                    }
                }
            }
        }
        public string MarkKPI(int eventId)
        {
            return TransactionWrapper.Do(() =>
            {
                var leadRepo = DependencyHelper.GetService<ILeadService>();
                var leads = leadRepo.GetAllLeads(m =>
                    (eventId == 0 || m.EventID == eventId) &&
                    m.CreatedTime.Date >= DateFrom.Date &&
                    m.CreatedTime.Date <= DateTo.Date &&
                    (m.LeadStatusRecord == LeadStatus.Live ||
                     m.LeadStatusRecord == LeadStatus.LOI ||
                     m.LeadStatusRecord == LeadStatus.Booked));
                foreach (var lead in leads)
                {
                    var jobTitle = lead.ClientName.ToLower().Trim();
                    var eventKeyworks = lead.Event.CallFilterKeywords != null
                        ? lead.Event.CallFilterKeywords.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToLower().Trim())
                        : new List<string>();
                    if (eventKeyworks.Any(m => m.Contains(jobTitle)) || eventKeyworks.Any(m => jobTitle.Contains(m)))
                    {
                        if (!string.IsNullOrEmpty(lead.PersonalEmailAddress) ||
                            !string.IsNullOrEmpty(lead.WorkEmailAddress) ||
                            !string.IsNullOrEmpty(lead.WorkEmailAddress1))
                        {

                            var voips = ImportVoIps.Where(m => m.clid == lead.User.Extension && m.dst == lead.GeneralLine && !string.IsNullOrEmpty(m.disposition) && m.disposition.Trim().ToUpper() == "ANSWERED");
                            if (lead.PhoneCalls.Any(m => voips.Any(v => m.StartTime.AddSeconds(-Settings.System.VoIpBuffer()) <= v.CallDateTime && v.CallDateTime <= m.StartTime.AddSeconds(Settings.System.VoIpBuffer()))))
                            {
                                lead.MarkKPI = true;
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

                    leadRepo.UpdateLead(lead);
                }
                return "";
            });
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