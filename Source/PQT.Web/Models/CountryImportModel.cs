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
using PQT.Web.Hubs;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Models
{
    public class CountryImportModel
    {
        public List<Country> Countries { get; set; }
        public string SessionName { get; set; }
        public string FilePath { get; set; }
        public HttpPostedFileBase FileImport { get; set; }
        public List<CountryJson> ImportRows { get; set; }

        public bool IsValid { get; set; }

        public CountryImportModel()
        {
            IsValid = false;
            Countries = new List<Country>();
            ImportRows = new List<CountryJson>();
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
                        var temp = new CountryJson();

                        try
                        {
                            temp.Code = dtRow[0].ToString();
                            if (string.IsNullOrEmpty(temp.Code.Trim()))
                            {
                                temp.Error += "- Country Code is required.<br/>";
                            }
                            else if (unitRepo.GetCountry(temp.Code.Trim().ToUpper()) != null)
                            {
                                temp.Error += "- Country Code exists.<br/>";
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Country with code: " + dtRow[0] + " is wrong.<br/>";
                        }
                        try
                        {
                            temp.Name = dtRow[1].ToString();
                            if (string.IsNullOrEmpty(temp.Name))
                            {
                                temp.Error += "- Country name is required.<br/>";
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Country name: " + dtRow[1] + " is wrong.<br/>";
                        }
                        try
                        {
                            temp.DialingCode = dtRow[2].ToString();
                            if (string.IsNullOrEmpty(temp.DialingCode))
                            {
                                temp.Error += "- Dialing Code is required.<br/>";
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Dialing Code: " + dtRow[1] + " is wrong.<br/>";
                        }
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
                Countries = new List<Country>();
                foreach (var row in ImportRows)
                {
                    var com = Mapper.Map<Country>(row);
                    Countries.Add(com);
                }
            }
        }

        public void ConfirmImport()
        {
            var comRepo = DependencyHelper.GetService<IUnitRepository>();
            var count = 0;
            var totalCount = Countries.Count();
            var userId = CurrentUser.Identity.ID;
            foreach (var com in Countries)
            {
                comRepo.CreateCountry(com);
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

    public class CountryJson
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string DialingCode { get; set; }
        public string Error { get; set; }
    }

}