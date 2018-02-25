using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PQT.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Login",
                url: "Login",
                defaults: new { controller = "Account", action = "Login" });

            routes.MapRoute(
                name: "Logout",
                url: "Logout",
                defaults: new { controller = "Account", action = "Logout" });

            routes.MapRoute(
                name: "BypassAuth",
                url: "n/{token}",
                defaults: new { controller = "Account", action = "BypassLogin" });
            routes.MapRoute(
                "/",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "PQT.Web.Controllers" });
        }
    }
}
