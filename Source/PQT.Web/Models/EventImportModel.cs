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
    public class EventImportModel
    {
        public List<Event> Events { get; set; }
        public string SessionName { get; set; }
        public string FilePath { get; set; }
        public HttpPostedFileBase FileImport { get; set; }
        public List<EventJson> ImportRows { get; set; }

        public bool IsValid { get; set; }

        public EventImportModel()
        {
            IsValid = false;
            Events = new List<Event>();
            ImportRows = new List<EventJson>();
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
                        var temp = new EventJson();

                        try
                        {
                            temp.EventCode = dtRow[0].ToString();
                            if (string.IsNullOrEmpty(temp.EventCode))
                            {
                                temp.Error += "- Event Code is required.<br/>";
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Event with code: " + dtRow[0] + " is wrong.<br/>";
                        }
                        try
                        {
                            temp.EventStatusStr = dtRow[1].ToString();
                            if (!string.IsNullOrEmpty(temp.EventStatusStr))
                            {
                                temp.EventStatus = Enumeration.FromName<EventStatus>(temp.EventStatusStr);
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Event Status: " + dtRow[1] + " is wrong.<br/>";
                        }
                        try
                        {
                            temp.EventCategoryStr = dtRow[2].ToString();
                            if (string.IsNullOrEmpty(temp.EventCategoryStr))
                            {
                                temp.Error += "- Event Category is required.<br/>";
                            }
                            var cate = unitRepo.GetEventCategory(temp.EventCategoryStr);
                            if (cate == null)
                            {
                                cate = unitRepo.CreateEventCategory(new EventCategory {Name = temp.EventCategoryStr});
                                if (cate == null)
                                {
                                    temp.Error += "- Event Category does not exist.<br/>";
                                }
                                else
                                {
                                    temp.EventCategoryID = cate.ID;
                                }
                            }
                            else
                            {
                                temp.EventCategoryID = cate.ID;
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Event Category: " + dtRow[1] + " is wrong.<br/>";
                        }
                        temp.EventName = dtRow[3].ToString();
                        if (string.IsNullOrEmpty(temp.EventName))
                        {
                            temp.Error += "- Event Name is required.<br/>";
                        }
                        try
                        {
                            temp.StartDateStr = dtRow[4].ToString();
                            if (string.IsNullOrEmpty(temp.StartDateStr))
                            {
                                temp.Error += "- Event First Date is required.<br/>";
                            }
                            else
                            {
                                temp.StartDate = Convert.ToDateTime(temp.StartDateStr);
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Event First Date : " + dtRow[1] + " is wrong format.<br/>";
                        }
                        try
                        {
                            temp.EndDateStr = dtRow[5].ToString();
                            if (string.IsNullOrEmpty(temp.EndDateStr))
                            {
                                temp.Error += "- Event Last Date is required.<br/>";
                            }
                            else
                            {
                                temp.EndDate = Convert.ToDateTime(temp.EndDateStr);
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Event Last Date : " + dtRow[1] + " is wrong format.<br/>";
                        }
                        try
                        {
                            temp.DateOfConfirmationStr = dtRow[6].ToString();
                            if (string.IsNullOrEmpty(temp.DateOfConfirmationStr))
                            {
                                temp.Error += "- Date Of Confirmation is required.<br/>";
                            }
                            else
                            {
                                temp.DateOfConfirmation = Convert.ToDateTime(temp.DateOfConfirmationStr);
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Date Of Confirmation : " + dtRow[1] + " is wrong format.<br/>";
                        }
                        try
                        {
                            temp.ClosingDateStr = dtRow[7].ToString();
                            if (string.IsNullOrEmpty(temp.ClosingDateStr))
                            {
                                temp.Error += "- Date Of Closing Sales is required.<br/>";
                            }
                            else
                            {
                                temp.ClosingDate = Convert.ToDateTime(temp.ClosingDateStr);
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Date Of Closing Sales : " + dtRow[1] + " is wrong format.<br/>";
                        }
                        try
                        {
                            temp.DateOfOpenStr = dtRow[8].ToString();
                            if (string.IsNullOrEmpty(temp.DateOfOpenStr))
                            {
                                temp.Error += "- Date Of Open is required.<br/>";
                            }
                            else
                            {
                                temp.DateOfOpen = Convert.ToDateTime(temp.DateOfOpenStr);
                            }
                        }
                        catch (Exception e)
                        {
                            temp.Error += "- Date Of Open : " + dtRow[1] + " is wrong format.<br/>";
                        }
                        temp.Location = dtRow[9].ToString();
                        temp.Summary = dtRow[10].ToString();
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
                Events = new List<Event>();
                foreach (var row in ImportRows)
                {
                    var com = Mapper.Map<Event>(row);
                    Events.Add(com);
                }
            }
        }

        public void ConfirmImport()
        {
            var comRepo = DependencyHelper.GetService<IEventService>();
            var count = 0;
            var totalCount = Events.Count();
            var userId = CurrentUser.Identity.ID;
            foreach (var com in Events)
            {
                comRepo.CreateEvent(com, new List<int>(), new List<int>());
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

    public class EventJson
    {
        public EventJson()
        {
            EventStatus = EventStatus.Initial;
        }
        public string EventCode { get; set; }
        public EventStatus EventStatus { get; set; }
        public string EventStatusStr { get; set; }
        public string EventCategoryStr { get; set; }
        public int EventCategoryID { get; set; }
        public string EventName { get; set; }
        public DateTime StartDate { get; set; }//Sales start - Event First Date
        public string StartDateStr { get; set; }//Sales start - Event First Date
        public DateTime EndDate { get; set; }//Sales end - Event Last Date
        public string EndDateStr { get; set; }//Sales end - Event Last Date
        public DateTime DateOfConfirmation { get; set; }
        public string DateOfConfirmationStr { get; set; }
        public DateTime ClosingDate { get; set; } //Date of Closing Sales
        public string ClosingDateStr { get; set; } //Date of Closing Sales
        public DateTime DateOfOpen { get; set; }//Date of Open (Cross Sell)
        public string DateOfOpenStr { get; set; }//Date of Open (Cross Sell)
        public string Location { get; set; }
        public string Summary { get; set; }
        public string Error { get; set; }
    }

}