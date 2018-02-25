using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;

namespace PQT.Domain.Helpers
{
    public class UrlHelper
    {
        public static string Root
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    return ConfigurationManager.AppSettings["DomainRoot"];
                }
                var urlHelper = new System.Web.Mvc.UrlHelper(HttpContext.Current.Request.RequestContext);
                return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + urlHelper.Content("~");
            }
        }
    }
}
