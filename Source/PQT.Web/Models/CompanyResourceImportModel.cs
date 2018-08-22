using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using AutoMapper;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Hubs;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Models
{
    public class CompanyResourceImportModel
    {
        public List<CompanyResource> CompanyResources { get; set; }
        public string SessionName { get; set; }
        public string FilePath { get; set; }
        public HttpPostedFileBase FileImport { get; set; }
        public List<CompanyResourceJson> ImportRows { get; set; }

        public bool IsValid { get; set; }

        public CompanyResourceImportModel()
        {
            IsValid = false;
            CompanyResources = new List<CompanyResource>();
            ImportRows = new List<CompanyResourceJson>();
        }

        public void check_data()
        {
            var unitRepo = DependencyHelper.GetService<IUnitRepository>();
            DataTable dtSheetName = ExcelUploadHelper.GetUploadedExcelSpreadsheetName(FilePath);
            foreach (DataRow row in dtSheetName.Rows)
            {
                string strSheetName = row["TABLE_NAME"].ToString();
                if (strSheetName.Contains("FilterDatabase"))
                {
                    continue;
                }
                DataTable dt = ExcelUploadHelper.GetDataFromExcel(FilePath, strSheetName);
                var count = 0;
                foreach (DataRow dtRow in dt.Rows)
                {
                    if (dtRow[4] != null && !string.IsNullOrEmpty(dtRow[4].ToString().Trim()))
                    {
                        count++;
                        var temp = new CompanyResourceJson();
                        try
                        {
                            temp.Country = dtRow[0].ToString();
                            if (!string.IsNullOrEmpty(temp.Country))
                            {
                                temp.CountryID = unitRepo.GetCountry(temp.Country.Trim().ToUpper()).ID;
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Country with code: " + dtRow[0] + " does not exist<br/>";
                        }
                        temp.Salutation = dtRow[1].ToString();
                        temp.FirstName = dtRow[2].ToString();
                        temp.LastName = dtRow[3].ToString();
                        try
                        {
                            temp.Organisation = dtRow[4].ToString();
                            if (string.IsNullOrEmpty(temp.Organisation))
                            {
                                temp.Error += "- Organisation must be empty<br/>";
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Organisation: " + dtRow[1] + " is wrong.<br/>";
                        }
                        temp.Role = dtRow[5].ToString();
                        temp.DirectLine = dtRow[6].ToString();
                        temp.MobilePhone1 = dtRow[7].ToString();
                        temp.MobilePhone2 = dtRow[8].ToString();
                        temp.MobilePhone3 = dtRow[9].ToString();
                        temp.WorkEmail = dtRow[10].ToString();
                        temp.PersonalEmail = dtRow[11].ToString();
                        temp.BusinessUnit = dtRow[12].ToString();
                        temp.BudgetMonthStr = dtRow[13].ToString();
                        if (!string.IsNullOrEmpty(temp.BudgetMonthStr))
                        {
                            try
                            {
                                temp.BudgetMonth = Convert.ToInt32(temp.BudgetMonthStr);
                            }
                            catch (Exception e)
                            {
                                temp.Error += "- Budget Month must be number.<br/>";
                            }
                        }
                        else
                        {
                            temp.BudgetMonth = 0;
                        }
                        temp.Number = count;
                        ImportRows.Add(temp);
                    }
                }
            }
        }
        /// <summary>
        /// check all indent must ETA date greater than  Indent Date
        /// </summary>
        /// <returns></returns>

        public void ParseValue()
        {
            if (ImportRows != null && ImportRows.Count > 0)
            {
                CompanyResources = new List<CompanyResource>();
                foreach (var row in ImportRows)
                {
                    CompanyResources.Add(Mapper.Map<CompanyResource>(row));
                }
            }
        }

        public void ConfirmImport()
        {
            var comRepo = DependencyHelper.GetService<ICompanyRepository>();

            var count = 0;
            var totalCount = CompanyResources.Count();
            var userId = CurrentUser.Identity.ID;
            foreach (var com in CompanyResources)
            {
                var comExist = comRepo.GetCompany(com.Organisation);
                if (comExist == null)
                {
                    var newCom = new Company
                    {
                        CountryID = com.CountryID,
                        CompanyName = com.Organisation,
                    };
                    newCom = comRepo.CreateCompany(newCom, new List<int>());
                    com.CompanyID = newCom.ID;
                }
                else
                {
                    com.CompanyID = comExist.ID;
                    //comExist.BusinessUnit = com.BusinessUnit;
                    //comExist.BudgetMonth = com.BudgetMonth;
                    //comRepo.UpdateCompany(comExist);
                }
                comRepo.CreateCompanyResource(com);
                count++;
                var json = new
                {
                    count,
                    totalCount
                };
                ProgressHub.SendMessage(userId, System.Web.Helpers.Json.Encode(json));
            }
        }
    }

    public class CompanyResourceJson
    {
        public int Number { get; set; }
        public string Country { get; set; }
        public int? CountryID { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Organisation { get; set; }
        public string Role { get; set; }
        public string DirectLine { get; set; }
        public string MobilePhone1 { get; set; }
        public string MobilePhone2 { get; set; }
        public string MobilePhone3 { get; set; }
        public string WorkEmail { get; set; }
        public string PersonalEmail { get; set; }
        public string BusinessUnit { get; set; }
        public string BudgetMonthStr { get; set; }
        public int BudgetMonth { get; set; }
        public string Error { get; set; }
    }
}