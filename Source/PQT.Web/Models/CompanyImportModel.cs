using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using AutoMapper;
using NS;
using NS.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Models
{
    public class CompanyImportModel
    {
        public List<Company> Companies { get; set; }
        public string SessionName { get; set; }
        public string FilePath { get; set; }
        public HttpPostedFileBase FileImport { get; set; }
        public List<CompanyJson> ImportRows { get; set; }

        public bool IsValid { get; set; }

        public CompanyImportModel()
        {
            IsValid = false;
            Companies = new List<Company>();
            ImportRows = new List<CompanyJson>();
        }

        public void check_data()
        {
            var unitRepo = DependencyHelper.GetService<IUnitRepository>();

            DataTable dtSheetName = ExcelUploadHelper.GetUploadedExcelSpreadsheetName(FilePath);
            foreach (DataRow row in dtSheetName.Rows)
            {
                string strSheetName = row["TABLE_NAME"].ToString();
                DataTable dt = ExcelUploadHelper.GetDataFromExcel(FilePath, strSheetName);

                foreach (DataRow dtRow in dt.Rows)
                {
                    if (dtRow[0] != null && !string.IsNullOrEmpty(dtRow[0].ToString().Trim()))
                    {
                        var temp = new CompanyJson();

                        try
                        {
                            temp.CountryCode = dtRow[0].ToString();
                            temp.CountryID = unitRepo.GetCountry(temp.CountryCode.Trim().ToUpper()).ID;
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Country with code: " + dtRow[0] + " does not exist<br/>";
                        }
                        try
                        {
                            temp.CompanyName = dtRow[1].ToString();
                            if (string.IsNullOrEmpty(temp.CompanyName))
                            {
                                temp.Error += "- Company name must be empty<br/>";
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Company name: " + dtRow[1] + " is wrong.<br/>";
                        }
                        temp.ProductOrService = dtRow[2].ToString();
                        temp.Sector = dtRow[3].ToString();
                        temp.Industry = dtRow[4].ToString();
                        temp.Designation = dtRow[5].ToString();
                        temp.Ownership = dtRow[6].ToString();
                        temp.BusinessUnits = dtRow[7].ToString();
                        temp.Email = dtRow[8].ToString();
                        temp.PersonalContact = dtRow[9].ToString();
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
                Companies = new List<Company>();
                Company com = null;
                foreach (var row in ImportRows)
                {
                    Companies.Add(Mapper.Map<Company>(row));
                }
            }
        }

        public void ConfirmImport()
        {
            var comRepo = DependencyHelper.GetService<ICompanyRepository>();
            foreach (var com in Companies)
            {
                comRepo.CreateCompany(com);
            }
        }
    }

    public class CompanyJson
    {
        public int CountryID { get; set; }
        public string CountryCode { get; set; }
        public string CompanyName { get; set; }
        public string ProductOrService { get; set; }
        public string Sector { get; set; }
        public string Industry { get; set; }
        public string Ownership { get; set; }
        public string BusinessUnits { get; set; }
        public string Designation { get; set; }
        public string Email { get; set; }
        public string PersonalContact { get; set; }
        public string Error { get; set; }
    }

}