using System;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;

namespace PQT.Web.Infrastructure.Helpers
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

        public static string Absolute(string root, string url)
        {
            if (url.StartsWith("/"))
                url = url.Substring(1);
            if (string.IsNullOrEmpty(root))
            {
                root = ConfigurationManager.AppSettings["DomainRoot"];
            }
            return root + url;
        }
        public static string Absolute(string url)
        {
            if (url.StartsWith("/"))
                url = url.Substring(1);
            var root = Root;
            if (string.IsNullOrEmpty(root))
            {
                root = ConfigurationManager.AppSettings["DomainRoot"];
            }
            return root + url;
        }
        public static string ToQueryString(object request, string separator = ",")
        {
            if (request == null)
                throw new ArgumentNullException("request");

            // Get all properties on the object
            var properties = request.GetType().GetProperties()
                .Where(x => x.CanRead)
                .Where(x => x.GetValue(request, null) != null)
                .ToDictionary(x => x.Name, x => x.GetValue(request, null));

            // Get names for all IEnumerable properties (excl. string)
            var propertyNames = properties
                .Where(x => !(x.Value is string) && x.Value is IEnumerable)
                .Select(x => x.Key)
                .ToList();

            // Concat all IEnumerable properties into a comma separated string
            foreach (var key in propertyNames)
            {
                var valueType = properties[key].GetType();
                var valueElemType = valueType.IsGenericType
                                        ? valueType.GetGenericArguments()[0]
                                        : valueType.GetElementType();
                if (valueElemType.IsPrimitive || valueElemType == typeof(string))
                {
                    var enumerable = properties[key] as IEnumerable;
                    properties[key] = string.Join(separator, enumerable.Cast<object>());
                }
            }

            // Concat all key/value pairs into a string separated by ampersand
            return string.Join("&", properties
                .Select(x => string.Concat(
                    Uri.EscapeDataString(x.Key), "=",
                    Uri.EscapeDataString(x.Value.ToString()))));
        }
    }
}
