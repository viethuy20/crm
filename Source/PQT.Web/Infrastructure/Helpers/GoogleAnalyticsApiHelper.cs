using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace CPO.Web.Infrastructure.Helpers
{
    public class GoogleAnalyticsApiHelper
    {
        static string YoutubeApiKey = "AIzaSyAkW4wvsItiMmTZDUHPlW3mfvItbd35Ju4";
        static string YoutubeApplicationName = "Server key 1";
        public static IEnumerable<ReportRow> GetAnalytics()
        {
            try
            {
                //ClientSecrets secrets = new ClientSecrets()
                //{
                //    ClientId = "734997467198-lv79t37kl5jmoec1f32gtrntqj8t8f8k.apps.googleusercontent.com",
                //    ClientSecret = "6NSDSJRCSQCnsf_9Dj-CQH7W"
                //};

                //var token = new TokenResponse { RefreshToken = REFRESH_TOKEN };
                //var credential = new UserCredential(new GoogleAuthorizationCodeFlow(
                //    new GoogleAuthorizationCodeFlow.Initializer
                //    {
                //        ClientSecrets = secrets
                //    }),
                //    "phanvulinhtn@gmail.com",
                //    token);

                ////var credential = GetCredential().Result;
                //using (var svc = new AnalyticsReportingService(
                //    new BaseClientService.Initializer
                //    {
                //        HttpClientInitializer = credential,
                //        ApplicationName = "Google Analytics API Console"
                //    }))
                //{    
                using (var svc = new AnalyticsReportingService(
                    new BaseClientService.Initializer
                    {
                        ApiKey = YoutubeApiKey,
                        ApplicationName = YoutubeApplicationName
                    }))
                {
                    var dateRange = new DateRange
                    {
                        StartDate = "2016-05-01",
                        EndDate = "2016-05-31"
                    };
                    var sessions = new Metric
                    {
                        Expression = "ga:sessions",
                        Alias = "Sessions"
                    };
                    var date = new Dimension { Name = "ga:date" };

                    var reportRequest = new ReportRequest
                    {
                        DateRanges = new List<DateRange> { dateRange },
                        Dimensions = new List<Dimension> { date },
                        Metrics = new List<Metric> { sessions },
                        ViewId = "UA-88358623-1"
                    };
                    var getReportsRequest = new GetReportsRequest
                    {
                        ReportRequests = new List<ReportRequest> { reportRequest }
                    };
                    var batchRequest = svc.Reports.BatchGet(getReportsRequest);
                    var response = batchRequest.Execute();
                    return response.Reports.First().Data.Rows;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return new List<ReportRow>();
        }

        static async Task<UserCredential> GetCredential()
        {
            using (var stream = new FileStream(HttpContext.Current.Server.MapPath("client_secret.json"),
                 FileMode.Open, FileAccess.Read))
            {
                const string loginEmailAddress = "phanvulinhtn@gmail.com";
                var authResult =
                    await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        new[] {AnalyticsReportingService.Scope.Analytics},
                        loginEmailAddress, CancellationToken.None,
                        new FileDataStore("GoogleAnalyticsApiConsole"));
                return authResult;
            }
        }
    }
}