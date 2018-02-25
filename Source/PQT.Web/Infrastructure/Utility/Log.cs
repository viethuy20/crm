using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;
using System.Web.Routing;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Helpers;
using PQT.Web.Infrastructure.Helpers;

namespace PQT.Web.Infrastructure.Utility
{
    public class Log
    {
        #region Repository

        private static IAuditTracker AuditTracker
        {
            get { return DependencyHelper.GetService<IAuditTracker>(); }
        }

        #endregion

        public static void Info(string message = "")
        {
            RouteData routeData = HttpContext.Current.Request.RequestContext.RouteData;
            string controllerName = routeData.GetRequiredString("controller");
            string action = routeData.GetRequiredString("action");
            int actionId = routeData.Values["id"] != null ? Convert.ToInt32(routeData.Values["id"]) : 0;
            //if (routeData.Values["id"] != null && int.TryParse(routeData.Values["id"].ToString(), out actionId))
            //    Info(message, controllerName, actionId);
            if ((controllerName == "NewBooking" && action == "AddedConfirm") ||
                (controllerName == "Booking" && action == "AddedConfirm") ||
                (HttpContext.Current.Request.HttpMethod == "POST" &&
                controllerName != "Account" &&
                controllerName != "Report" &&
                controllerName != "NewReport" &&
                !action.ToLower().Contains("ajax") &&
                !action.ToLower().Contains("get") &&
                (!HttpContext.Current.Request.IsAjaxRequest() ||
                action.ToLower().Contains("create") ||
                action.ToLower().Contains("add") ||
                action.ToLower().Contains("edit") ||
                action.ToLower().Contains("modify") ||
                action.ToLower().Contains("delete") ||
                action.ToLower().Contains("cancel") ||
                action.ToLower().Contains("submit") ||
                action.ToLower().Contains("approve") ||
                action.ToLower().Contains("approval") ||
                action.ToLower().Contains("verify") ||
                action.ToLower().Contains("reject"))))
            {
                Info(message, controllerName, actionId);
            }
        }

        public static void Info(string message, int actionId)
        {
            RouteData routeData = HttpContext.Current.Request.RequestContext.RouteData;
            string controllerName = routeData.GetRequiredString("controller");
            Info(message, controllerName, actionId);
        }

        public static void Info(string message, string controllerName, int actionId = 0)
        {
            HttpRequest request = HttpContext.Current.Request;

            string username = "";
            string email = "";
            var indentityUser = CurrentUser.Identity;
            if (indentityUser != null)
            {
                username = indentityUser.DisplayName;
                email = indentityUser.Email;
            }

            string[] keys = request.Form.AllKeys;
            List<string> dataForm = keys.Select(t => t + ": " + request.Form[t]).ToList();
            var record = new Audit
            {
                Username = username,
                Email = email,
                IPAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress,
                UrlAccessed = request.RawUrl,
                TimeAccessed = DateTime.Now,
                SessionId = HttpContext.Current.Session.SessionID,
                Data = Json.Encode(new { request.Cookies, request.Headers, request.Files, dataForm, request.QueryString }),
                Message = message,
                Type = (int)AuditType.Manual,
                Controller = controllerName,
                ActionId = actionId
            };

            AuditTracker.CreateRecord(record);
        }

        public static void ApiInfo(string message, string data)
        {
            HttpRequest request = HttpContext.Current.Request;

            string username = "Api";
            string email = "Api";
            var record = new Audit
            {
                Username = username,
                Email = email,
                IPAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress,
                UrlAccessed = request.RawUrl,
                TimeAccessed = DateTime.Now,
                SessionId = HttpContext.Current.Session.SessionID,
                Data = Json.Encode(data),
                Message = message,
                Type = (int)AuditType.Auto,
                Controller = "",
                ActionId = 0
            };

            AuditTracker.CreateRecord(record);
        }
        //public static void Debug(bool flag, string message)
        //{
        //    if (flag)
        //        Debug(message);
        //}

        //public static void Debug(string message)
        //{
        //    string fileName = HttpContext.Current.Server.MapPath("~/debug.txt");
        //    File.AppendAllText(fileName, "\n---------------------\n" + DateTime.Now + "\n---------------------\n" + message);
        //}
    }
}
