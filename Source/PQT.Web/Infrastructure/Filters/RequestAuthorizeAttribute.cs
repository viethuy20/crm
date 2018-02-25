using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using PQT.Domain.Entities;
using PQT.Web.Infrastructure.Utility;
using NS;

namespace PQT.Web.Infrastructure.Filters
{
    public class RequestAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            RouteData rd = filterContext.RouteData;
            string controller = rd.GetRequiredString("controller");
            string action = rd.GetRequiredString("action");

           
            ////////////////////////////////////////
            // Bypass if this action has excluded the RequestAuthorize filter
            // aka allow anonymous access
            ////////////////////////////////////////

            var filters = FilterProviders.Providers.GetFilters(filterContext.Controller.ControllerContext, filterContext.ActionDescriptor);
            if (!filters.Select(filter => filter.Instance.GetType()).Contains(typeof(RequestAuthorizeAttribute)))
                return;

            ////////////////////////////////////////
            // Login by token
            ////////////////////////////////////////

            string token = filterContext.RequestContext.HttpContext.Request.QueryString.Get("token");
            if (token != null)
            {
                var urlHelper = new UrlHelper(filterContext.RequestContext);
                if (TokenLogin.ValidateToken(token))
                    return;

                HandleUnauthorizedRequest(filterContext);
            }

            ////////////////////////////////////////
            // Normal request
            ////////////////////////////////////////

            if (RequestPermissionProvider.LoginRequired.Contains(controller))
            {
                if (!CurrentUser.IsAuthenticated)
                    HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                if (!CurrentUser.IsAuthenticated)
                    HandleUnauthorizedRequest(filterContext);
                else if (!MvcHelper.CheckActionIsAjaxOnly(controller, action) && !CurrentUser.HasPermission(controller, action))
                    HandleUnauthorizedRequest(filterContext);
            }
            //SetLanguages();
        }

        protected void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var urlHelper = new UrlHelper(filterContext.RequestContext);
            string loginUrl = urlHelper.Action("Login", "Account");

            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    Data = new
                    {
                        Error = "NotAUthorized",
                        LogOnUrl = loginUrl
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                filterContext.HttpContext.Response.StatusCode = 403;
            }
            else
            {
                if (CurrentUser.IsAuthenticated)
                {
                    if (filterContext.IsChildAction)
                        filterContext.Result = new EmptyResult();
                    else
                        filterContext.Result = new ViewResult { ViewName = "Forbidden"};
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "Account",
                        action = "Login",
                        area = "",
                        returnUrl = filterContext.HttpContext.Request.RawUrl
                    }));
                }
            }
        }

    }
}
